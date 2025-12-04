using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clase base para todos los bosses de escalada con IA avanzada
/// Utiliza GOAP (Goal-Oriented Action Planning) para toma de decisiones
/// </summary>
public abstract class BossAIBase : MonoBehaviour
{
    [Header("Boss Configuration")]
    [SerializeField] protected string bossName = "Boss";
    [SerializeField] protected BossDifficulty difficulty = BossDifficulty.Medium;
    [SerializeField] protected float baseSpeed = 5f;
    
    [Header("AI Settings")]
    [Range(0f, 1f)]
    [SerializeField] protected float aiIntelligence = 0.7f; // 0 = muy tonto, 1 = perfecto
    [Range(0.5f, 2f)]
    [SerializeField] protected float speedModifier = 1f; // Modificador de velocidad
    [Range(0f, 1f)]
    [SerializeField] protected float decisionDelay = 0.3f; // Retraso en toma de decisiones
    [SerializeField] protected bool allowMistakes = true; // Permite errores ocasionales

    [Header("Stamina System")]
    [SerializeField] protected float maxStamina = 100f;
    [SerializeField] protected float currentStamina = 100f;
    [SerializeField] protected float staminaDrainRate = 5f;
    [SerializeField] protected float staminaRecoveryRate = 10f;

    [Header("Components")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected ClimbingPathfinder pathfinder;

    [Header("Race Settings")]
    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform goalPoint;
    [SerializeField] protected bool raceStarted = false;

    // GOAP System
    protected GOAPPlanner planner;
    protected Queue<GOAPAction> currentPlan;
    protected GOAPAction currentAction;
    protected List<GOAPAction> availableActions;
    protected Dictionary<string, object> worldState;
    protected Dictionary<string, object> goals;

    // Pathfinding
    protected List<ClimbPoint> currentPath;
    protected int currentPathIndex = 0;
    protected Transform nextClimbPoint;

    // State tracking
    protected BossState currentState = BossState.Idle;
    protected float lastDecisionTime = 0f;
    protected Vector3 lastPosition;
    protected bool isStuck = false;
    protected float stuckTimer = 0f;

    // Hook system simulation
    protected bool isUsingHook = false;
    protected Vector3 hookTarget;

    // Public properties
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public Transform NextClimbPoint => nextClimbPoint;
    public BossState CurrentState => currentState;
    public bool IsRaceActive => raceStarted;

    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeGOAP();
    }

    protected virtual void Start()
    {
        if (pathfinder == null)
        {
            pathfinder = FindObjectOfType<ClimbingPathfinder>();
            if (pathfinder != null)
            {
                pathfinder.InitializeClimbPoints();
            }
        }

        currentStamina = maxStamina;
        lastPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (!raceStarted) return;

        UpdateStamina();
        CheckIfStuck();
        ExecuteGOAP();
        UpdateAnimation();
    }

    /// <summary>
    /// Inicializa los componentes necesarios
    /// </summary>
    protected virtual void InitializeComponents()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();
        
        planner = new GOAPPlanner();
        availableActions = new List<GOAPAction>();
        worldState = new Dictionary<string, object>();
        goals = new Dictionary<string, object>();
    }

    /// <summary>
    /// Inicializa el sistema GOAP con las acciones disponibles
    /// </summary>
    protected virtual void InitializeGOAP()
    {
        // Acciones básicas disponibles para todos los bosses
        availableActions.Add(new MoveToClimbPointAction());
        availableActions.Add(new ClimbAction());
        availableActions.Add(new UseHookAction());
        availableActions.Add(new RestAction());

        // Objetivo principal: llegar a la meta
        goals.Add("atGoal", true);
    }

    /// <summary>
    /// Ejecuta el ciclo GOAP
    /// </summary>
    protected virtual void ExecuteGOAP()
    {
        // Aplicar retraso en decisiones basado en inteligencia
        if (Time.time - lastDecisionTime < decisionDelay * (1f - aiIntelligence))
        {
            return;
        }

        // Si no hay plan o el plan falló, crear uno nuevo
        if (currentPlan == null || currentPlan.Count == 0)
        {
            UpdateWorldState();
            currentPlan = planner.Plan(this, availableActions, worldState, goals);
            
            if (currentPlan == null || currentPlan.Count == 0)
            {
                HandleNoPlanFound();
                return;
            }
        }

        // Ejecutar la acción actual
        if (currentAction == null && currentPlan.Count > 0)
        {
            currentAction = currentPlan.Dequeue();
        }

        if (currentAction != null)
        {
            bool success = currentAction.Perform(this);
            
            if (!success || currentAction.IsComplete(this))
            {
                currentAction.Reset();
                currentAction = null;
            }

            // Simular errores ocasionales
            if (allowMistakes && Random.value > aiIntelligence)
            {
                SimulateMistake();
            }
        }

        lastDecisionTime = Time.time;
    }

    /// <summary>
    /// Actualiza el estado del mundo para GOAP
    /// </summary>
    protected virtual void UpdateWorldState()
    {
        worldState.Clear();
        
        worldState["hasStamina"] = currentStamina > 10f;
        worldState["lowStamina"] = currentStamina < maxStamina * 0.3f;
        worldState["onWall"] = IsOnClimbableWall();
        worldState["hookPointAvailable"] = FindNearestHookPoint() != null;
        worldState["atGoal"] = IsAtGoal();
        worldState["atClimbPoint"] = nextClimbPoint != null && 
                                     Vector3.Distance(transform.position, nextClimbPoint.position) < 1f;
    }

    /// <summary>
    /// Actualiza el sistema de resistencia
    /// </summary>
    protected virtual void UpdateStamina()
    {
        if (currentState == BossState.Climbing || currentState == BossState.Hooking)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (currentState == BossState.Resting)
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    /// <summary>
    /// Verifica si el boss está atascado
    /// </summary>
    protected virtual void CheckIfStuck()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 3f)
            {
                isStuck = true;
                HandleStuckState();
            }
        }
        else
        {
            stuckTimer = 0f;
            isStuck = false;
            lastPosition = transform.position;
        }
    }

    /// <summary>
    /// Mueve el boss hacia una posición
    /// </summary>
    public virtual void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        float adjustedSpeed = baseSpeed * speedModifier;
        
        rb.velocity = direction * adjustedSpeed;
        currentState = BossState.Moving;

        // Rotar hacia el objetivo
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Escala verticalmente
    /// </summary>
    public virtual void Climb()
    {
        if (currentStamina <= 0) return;

        Vector3 climbDirection = Vector3.up;
        float adjustedSpeed = baseSpeed * speedModifier * 0.6f; // Escalada más lenta
        
        rb.velocity = climbDirection * adjustedSpeed;
        currentState = BossState.Climbing;
    }

    /// <summary>
    /// Lanza el gancho hacia un objetivo
    /// </summary>
    public virtual void LaunchHook(Vector3 target)
    {
        hookTarget = target;
        isUsingHook = true;
        currentState = BossState.Hooking;
        
        // Simular el gancho (se movería hacia el objetivo)
        StartCoroutine(HookMovementCoroutine());
    }

    /// <summary>
    /// Corrutina para simular el movimiento del gancho
    /// </summary>
    protected System.Collections.IEnumerator HookMovementCoroutine()
    {
        float hookTime = 0f;
        float maxHookTime = 2f;
        Vector3 startPos = transform.position;

        while (hookTime < maxHookTime && isUsingHook)
        {
            hookTime += Time.deltaTime;
            float t = hookTime / maxHookTime;
            
            // Movimiento parabólico hacia el gancho
            Vector3 newPos = Vector3.Lerp(startPos, hookTarget, t);
            transform.position = newPos;

            yield return null;
        }

        isUsingHook = false;
        currentState = BossState.Moving;
    }

    /// <summary>
    /// Descansa para recuperar resistencia
    /// </summary>
    public virtual void Rest()
    {
        rb.velocity = Vector3.zero;
        currentState = BossState.Resting;
    }

    /// <summary>
    /// Verifica si está en una pared escalable
    /// </summary>
    public virtual bool IsOnClimbableWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1.5f))
        {
            return hit.collider.CompareTag("escalable");
        }
        return false;
    }

    /// <summary>
    /// Encuentra el punto de gancho más cercano
    /// </summary>
    public virtual Transform FindNearestHookPoint()
    {
        GameObject[] hookPoints = GameObject.FindGameObjectsWithTag("HookPoint");
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject hookPoint in hookPoints)
        {
            float distance = Vector3.Distance(transform.position, hookPoint.transform.position);
            if (distance < minDistance && distance < 20f)
            {
                minDistance = distance;
                nearest = hookPoint.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Verifica si llegó a la meta
    /// </summary>
    public virtual bool IsAtGoal()
    {
        if (goalPoint == null) return false;
        return Vector3.Distance(transform.position, goalPoint.position) < 2f;
    }

    /// <summary>
    /// Verifica si está usando el gancho
    /// </summary>
    public bool IsHooking()
    {
        return isUsingHook;
    }

    /// <summary>
    /// Inicia la carrera
    /// </summary>
    public virtual void StartRace()
    {
        raceStarted = true;
        currentStamina = maxStamina;
        
        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }

        // Calcular ruta inicial
        if (pathfinder != null && goalPoint != null)
        {
            currentPath = pathfinder.FindPath(transform.position, goalPoint.position, 1f - aiIntelligence);
            if (currentPath != null && currentPath.Count > 0)
            {
                currentPathIndex = 0;
                UpdateNextClimbPoint();
            }
        }

        OnRaceStart();
    }

    /// <summary>
    /// Detiene la carrera
    /// </summary>
    public virtual void StopRace()
    {
        raceStarted = false;
        rb.velocity = Vector3.zero;
        currentState = BossState.Idle;
        OnRaceEnd();
    }

    /// <summary>
    /// Actualiza el siguiente punto de escalada
    /// </summary>
    protected virtual void UpdateNextClimbPoint()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            nextClimbPoint = goalPoint;
            return;
        }

        // Crear un GameObject temporal para el siguiente punto
        GameObject tempPoint = new GameObject("TempClimbPoint");
        tempPoint.transform.position = currentPath[currentPathIndex].position;
        nextClimbPoint = tempPoint.transform;

        currentPathIndex++;
    }

    /// <summary>
    /// Maneja la situación cuando no se encuentra un plan
    /// </summary>
    protected virtual void HandleNoPlanFound()
    {
        // Movimiento directo hacia el objetivo como fallback
        if (goalPoint != null)
        {
            MoveTowards(goalPoint.position);
        }
    }

    /// <summary>
    /// Maneja cuando el boss está atascado
    /// </summary>
    protected virtual void HandleStuckState()
    {
        // Intentar usar el gancho si está disponible
        Transform hookPoint = FindNearestHookPoint();
        if (hookPoint != null)
        {
            LaunchHook(hookPoint.position);
        }
        else
        {
            // Saltar aleatoriamente
            rb.AddForce(Vector3.up * 5f + Random.insideUnitSphere * 3f, ForceMode.Impulse);
        }

        isStuck = false;
        stuckTimer = 0f;
    }

    /// <summary>
    /// Simula un error en la IA
    /// </summary>
    protected virtual void SimulateMistake()
    {
        // Pequeña pausa aleatoria
        if (Random.value < 0.3f)
        {
            Rest();
            Invoke("ResumeAfterMistake", Random.Range(0.5f, 1.5f));
        }
    }

    protected virtual void ResumeAfterMistake()
    {
        currentState = BossState.Moving;
    }

    /// <summary>
    /// Actualiza la animación según el estado
    /// </summary>
    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("IsClimbing", currentState == BossState.Climbing);
        animator.SetBool("IsMoving", currentState == BossState.Moving);
        animator.SetBool("IsResting", currentState == BossState.Resting);
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    // Métodos abstractos para ser implementados por bosses específicos
    protected abstract void OnRaceStart();
    protected abstract void OnRaceEnd();

    // Debug
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar ruta actual
        if (currentPath != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i].position, currentPath[i + 1].position);
            }
        }

        // Dibujar siguiente punto
        if (nextClimbPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(nextClimbPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, nextClimbPoint.position);
        }

        // Dibujar objetivo
        if (goalPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(goalPoint.position, 1f);
        }
    }
}

public enum BossState
{
    Idle,
    Moving,
    Climbing,
    Hooking,
    Resting,
    Finished
}

public enum BossDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert
}
