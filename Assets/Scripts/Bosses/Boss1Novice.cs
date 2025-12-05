using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BOSS 1: "El Novato" - Dificultad: Fácil
/// Mecánica única: Utiliza rutas simples y predecibles, comete errores frecuentes
/// IA: GOAP básico con alta tasa de error
/// </summary>
public class Boss1Novice : BossAIBase
{
    [Header("Boss 1 Specific Settings")]
    [SerializeField] private float mistakeChance = 0.4f; // 40% de probabilidad de error
    [SerializeField] private float slowdownFactor = 0.7f; // Velocidad reducida
    [SerializeField] private bool preferSafeRoutes = true; // Prefiere rutas seguras
    
    private float nextMistakeTime = 0f;
    private bool isRecoveringFromMistake = false;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Configuración específica para novato
        aiIntelligence = 0.3f; // Baja inteligencia
        speedModifier = slowdownFactor;
        decisionDelay = 0.8f; // Toma decisiones lentas
        allowMistakes = true;
    }

    protected override void InitializeGOAP()
    {
        base.InitializeGOAP();
        
        // El novato solo usa acciones básicas
        availableActions.Clear();
        availableActions.Add(new MoveToClimbPointAction());
        availableActions.Add(new ClimbAction());
        availableActions.Add(new RestAction()); // Descansa mucho
    }

    protected override void Update()
    {
        base.Update();

        if (!raceStarted) return;

        // Simular errores frecuentes
        if (Time.time > nextMistakeTime && !isRecoveringFromMistake)
        {
            if (Random.value < mistakeChance)
            {
                MakeMistake();
            }
            nextMistakeTime = Time.time + Random.Range(3f, 8f);
        }
    }

    /// <summary>
    /// Hace un error característico del novato
    /// </summary>
    private void MakeMistake()
    {
        int mistakeType = Random.Range(0, 4);
        
        switch (mistakeType)
        {
            case 0: // Descanso innecesario
                Debug.Log($"{bossName}: Descansando innecesariamente");
                Rest();
                Invoke("RecoverFromMistake", Random.Range(2f, 4f));
                break;
                
            case 1: // Movimiento en dirección incorrecta
                Debug.Log($"{bossName}: Moviéndose en dirección incorrecta");
                Vector3 wrongDirection = transform.position + Random.insideUnitSphere * 3f;
                MoveTowards(wrongDirection);
                Invoke("RecoverFromMistake", Random.Range(1f, 2f));
                break;
                
            case 2: // Quedarse quieto confundido
                Debug.Log($"{bossName}: Confundido, quedándose quieto");
                rb.velocity = Vector3.zero;
                currentState = BossState.Idle;
                Invoke("RecoverFromMistake", Random.Range(1.5f, 3f));
                break;
                
            case 3: // Intentar escalar cuando no hay pared
                Debug.Log($"{bossName}: Intentando escalar sin pared");
                if (!IsOnClimbableWall())
                {
                    Climb(); // Esto fallará
                }
                Invoke("RecoverFromMistake", 1f);
                break;
        }
        
        isRecoveringFromMistake = true;
    }

    private void RecoverFromMistake()
    {
        isRecoveringFromMistake = false;
        currentState = BossState.Moving;
        
        // Recalcular ruta después del error
        if (pathfinder != null && goalPoint != null)
        {
            currentPath = pathfinder.FindPath(transform.position, goalPoint.position, 1.5f); // Rutas más seguras
            currentPathIndex = 0;
            UpdateNextClimbPoint();
        }
    }

    public override void MoveTowards(Vector3 target)
    {
        // Movimiento más lento y menos preciso
        Vector3 direction = (target - transform.position).normalized;
        
        // Añadir imprecisión al movimiento
        direction += Random.insideUnitSphere * 0.2f;
        direction.Normalize();
        
        float adjustedSpeed = baseSpeed * speedModifier * 0.8f; // Extra lento
        rb.velocity = direction * adjustedSpeed;
        currentState = BossState.Moving;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f); // Rotación lenta
        }
    }

    public override void Climb()
    {
        base.Climb();
        
        // Escalada más lenta y consume más resistencia
        currentStamina -= staminaDrainRate * 0.5f * Time.deltaTime;
    }

    protected override void OnRaceStart()
    {
        Debug.Log($"{bossName} (Novato): ¡Comenzando la carrera! Espero no caerme...");
        nextMistakeTime = Time.time + Random.Range(5f, 10f);
        isRecoveringFromMistake = false;
    }

    protected override void OnRaceEnd()
    {
        if (IsAtGoal())
        {
            Debug.Log($"{bossName} (Novato): ¡Lo logré! Aunque fue difícil...");
        }
        else
        {
            Debug.Log($"{bossName} (Novato): Me perdí en el camino...");
        }
    }

    protected override void UpdateWorldState()
    {
        base.UpdateWorldState();
        
        // El novato sobreestima cuando necesita descansar
        worldState["lowStamina"] = currentStamina < maxStamina * 0.5f; // Descansa antes
    }
}
