using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BOSS 4: "El Profesional" - Dificultad: Experto
/// Mecánica única: Secuencias de escalada predefinidas y optimizadas con precisión perfecta
/// IA: Secuencias cronometradas + GOAP avanzado + Comportamiento adaptativo
/// </summary>
public class Boss4Professional : BossAIBase
{
    [Header("Boss 4 Specific Settings")]
    [SerializeField] private bool usePredefinedSequence = true;
    [SerializeField] private float sequencePrecision = 0.95f; // 95% de precisión
    [SerializeField] private bool perfectTiming = true;
    
    [Header("Professional Sequences")]
    [SerializeField] private List<ClimbingSequence> predefinedSequences = new List<ClimbingSequence>();
    [SerializeField] private int currentSequenceIndex = 0;
    
    [Header("Advanced AI")]
    [SerializeField] private bool predictPlayerMovement = true;
    [SerializeField] private float reactionTime = 0.1f; // Reacción casi instantánea
    [SerializeField] private bool optimizeStaminaUsage = true;

    // Secuencia actual
    private ClimbingSequence activeSequence;
    private int currentStepIndex = 0;
    private float stepStartTime = 0f;
    private bool sequenceActive = false;

    // Predicción y optimización
    private Vector3 predictedPlayerPosition;
    private float optimalSpeed;
    private Transform playerTransform;

    // Estadísticas de rendimiento
    private float totalDistance = 0f;
    private float averageSpeed = 0f;
    private int perfectMoves = 0;
    private int totalMoves = 0;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Configuración profesional
        aiIntelligence = 0.95f; // Inteligencia muy alta
        speedModifier = 1.2f; // Más rápido
        decisionDelay = 0.1f; // Decisiones casi instantáneas
        allowMistakes = false; // No comete errores
    }

    protected override void InitializeGOAP()
    {
        base.InitializeGOAP();
        
        // El profesional tiene acceso a todas las acciones optimizadas
        availableActions.Clear();
        availableActions.Add(new OptimizedMoveAction());
        availableActions.Add(new PerfectClimbAction());
        availableActions.Add(new PreciseHookAction());
        availableActions.Add(new StrategicRestAction());
    }

    protected override void Start()
    {
        base.Start();
        
        // Buscar jugador para predicción
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Inicializar secuencias si no existen
        if (predefinedSequences.Count == 0)
        {
            GenerateOptimalSequences();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!raceStarted) return;

        // Ejecutar secuencia predefinida o IA adaptativa
        if (usePredefinedSequence && sequenceActive)
        {
            ExecutePredefinedSequence();
        }

        // Predecir movimiento del jugador
        if (predictPlayerMovement && playerTransform != null)
        {
            PredictPlayerMovement();
        }

        // Optimizar velocidad
        if (optimizeStaminaUsage)
        {
            OptimizeSpeed();
        }

        // Actualizar estadísticas
        UpdatePerformanceStats();
    }

    /// <summary>
    /// Ejecuta una secuencia predefinida con timing perfecto
    /// </summary>
    private void ExecutePredefinedSequence()
    {
        if (activeSequence == null || currentStepIndex >= activeSequence.steps.Count)
        {
            // Secuencia completada, volver a IA normal
            sequenceActive = false;
            return;
        }

        SequenceStep currentStep = activeSequence.steps[currentStepIndex];
        float elapsedTime = Time.time - stepStartTime;

        // Verificar si es momento de ejecutar el paso
        if (elapsedTime >= currentStep.startTime)
        {
            ExecuteSequenceStep(currentStep);

            // Verificar si el paso se completó
            if (elapsedTime >= currentStep.startTime + currentStep.duration)
            {
                CompleteSequenceStep();
                currentStepIndex++;
                stepStartTime = Time.time;

                if (currentStepIndex < activeSequence.steps.Count)
                {
                    Debug.Log($"{bossName}: Ejecutando paso {currentStepIndex}/{activeSequence.steps.Count}");
                }
            }
        }
    }

    /// <summary>
    /// Ejecuta un paso específico de la secuencia
    /// </summary>
    private void ExecuteSequenceStep(SequenceStep step)
    {
        totalMoves++;

        switch (step.actionType)
        {
            case SequenceActionType.Move:
                MoveTowards(step.targetPosition);
                break;

            case SequenceActionType.Climb:
                Climb();
                break;

            case SequenceActionType.Hook:
                if (step.targetPosition != Vector3.zero)
                {
                    LaunchHook(step.targetPosition);
                }
                break;

            case SequenceActionType.Rest:
                Rest();
                break;

            case SequenceActionType.Sprint:
                SprintTowards(step.targetPosition);
                break;

            case SequenceActionType.WallJump:
                PerformWallJump();
                break;
        }

        // Verificar precisión
        if (step.requiresPrecision && IsMovementPrecise())
        {
            perfectMoves++;
        }
    }

    /// <summary>
    /// Completa un paso de secuencia
    /// </summary>
    private void CompleteSequenceStep()
    {
        // Aquí podrían añadirse efectos o sonidos de confirmación
    }

    /// <summary>
    /// Sprint optimizado hacia una posición
    /// </summary>
    private void SprintTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        float sprintSpeed = baseSpeed * speedModifier * 1.5f;
        
        rb.velocity = direction * sprintSpeed;
        currentState = BossState.Moving;

        // Consumir resistencia extra
        currentStamina -= staminaDrainRate * 1.5f * Time.deltaTime;
    }

    /// <summary>
    /// Ejecuta un wall jump perfecto
    /// </summary>
    private void PerformWallJump()
    {
        if (IsOnClimbableWall())
        {
            Vector3 jumpDirection = -transform.forward + Vector3.up;
            rb.AddForce(jumpDirection.normalized * 15f, ForceMode.Impulse);
            currentStamina -= 10f;
        }
    }

    /// <summary>
    /// Predice la posición futura del jugador
    /// </summary>
    private void PredictPlayerMovement()
    {
        if (playerTransform == null) return;

        // Obtener velocidad del jugador
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Predecir posición basándose en velocidad actual
            float predictionTime = 2f;
            predictedPlayerPosition = playerTransform.position + (playerRb.velocity * predictionTime);

            // Ajustar estrategia si el jugador se acerca a la meta
            if (goalPoint != null)
            {
                float playerDistanceToGoal = Vector3.Distance(predictedPlayerPosition, goalPoint.position);
                float bossDistanceToGoal = Vector3.Distance(transform.position, goalPoint.position);

                if (playerDistanceToGoal < bossDistanceToGoal)
                {
                    // Aumentar velocidad para compensar
                    speedModifier = 1.4f;
                }
            }
        }
    }

    /// <summary>
    /// Optimiza la velocidad según la resistencia disponible
    /// </summary>
    private void OptimizeSpeed()
    {
        if (goalPoint == null) return;

        float distanceToGoal = Vector3.Distance(transform.position, goalPoint.position);
        float staminaRatio = currentStamina / maxStamina;

        // Calcular velocidad óptima
        if (staminaRatio > 0.7f && distanceToGoal > 20f)
        {
            // Mucha resistencia, larga distancia: máxima velocidad
            optimalSpeed = 1.4f;
        }
        else if (staminaRatio < 0.3f)
        {
            // Poca resistencia: conservar energía
            optimalSpeed = 0.9f;
        }
        else if (distanceToGoal < 10f)
        {
            // Cerca de la meta: sprint final
            optimalSpeed = 1.5f;
        }
        else
        {
            // Balance
            optimalSpeed = 1.2f;
        }

        speedModifier = Mathf.Lerp(speedModifier, optimalSpeed, Time.deltaTime * 2f);
    }

    /// <summary>
    /// Verifica si el movimiento es preciso
    /// </summary>
    private bool IsMovementPrecise()
    {
        if (nextClimbPoint == null) return false;

        float distance = Vector3.Distance(transform.position, nextClimbPoint.position);
        return distance < 0.3f; // Muy preciso
    }

    /// <summary>
    /// Actualiza estadísticas de rendimiento
    /// </summary>
    private void UpdatePerformanceStats()
    {
        if (Time.deltaTime > 0)
        {
            float currentSpeed = rb.velocity.magnitude;
            averageSpeed = Mathf.Lerp(averageSpeed, currentSpeed, Time.deltaTime);
        }
    }

    /// <summary>
    /// Genera secuencias óptimas automáticamente
    /// </summary>
    private void GenerateOptimalSequences()
    {
        // Secuencia 1: Ruta rápida directa
        ClimbingSequence fastSequence = new ClimbingSequence
        {
            sequenceName = "Fast Route",
            description = "Ruta directa optimizada para velocidad"
        };

        // Aquí se añadirían pasos específicos basados en el diseño del nivel
        // Por ahora, una secuencia de ejemplo
        fastSequence.steps.Add(new SequenceStep
        {
            actionType = SequenceActionType.Sprint,
            targetPosition = transform.position + transform.forward * 10f,
            duration = 2f,
            startTime = 0f,
            requiresPrecision = false
        });

        predefinedSequences.Add(fastSequence);
    }

    public override void Climb()
    {
        base.Climb();

        // Escalada más eficiente
        if (optimizeStaminaUsage)
        {
            currentStamina -= staminaDrainRate * 0.6f * Time.deltaTime; // 40% menos consumo
        }
    }

    public override void LaunchHook(Vector3 target)
    {
        // Hook con precisión perfecta
        hookTarget = target;
        isUsingHook = true;
        currentState = BossState.Hooking;
        
        // Timing perfecto
        StartCoroutine(PerfectHookMovementCoroutine());
    }

    /// <summary>
    /// Movimiento de gancho con timing perfecto
    /// </summary>
    private System.Collections.IEnumerator PerfectHookMovementCoroutine()
    {
        float hookTime = 0f;
        float maxHookTime = 1.5f; // Más rápido que el base
        Vector3 startPos = transform.position;

        while (hookTime < maxHookTime && isUsingHook)
        {
            hookTime += Time.deltaTime;
            float t = hookTime / maxHookTime;
            
            // Curva de animación suave
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 newPos = Vector3.Lerp(startPos, hookTarget, smoothT);
            transform.position = newPos;

            yield return null;
        }

        isUsingHook = false;
        currentState = BossState.Moving;
        perfectMoves++; // Hook perfecto
    }

    public override void StartRace()
    {
        base.StartRace();

        // Iniciar con secuencia predefinida si está disponible
        if (usePredefinedSequence && predefinedSequences.Count > 0)
        {
            activeSequence = predefinedSequences[currentSequenceIndex];
            sequenceActive = true;
            currentStepIndex = 0;
            stepStartTime = Time.time;
        }

        // Reset estadísticas
        totalDistance = 0f;
        averageSpeed = 0f;
        perfectMoves = 0;
        totalMoves = 0;
    }

    protected override void OnRaceStart()
    {
        Debug.Log($"{bossName} (Profesional): Secuencia óptima cargada. Precisión al {sequencePrecision * 100}%");
    }

    protected override void OnRaceEnd()
    {
        if (IsAtGoal())
        {
            float accuracy = totalMoves > 0 ? (float)perfectMoves / totalMoves : 0f;
            Debug.Log($"{bossName} (Profesional): ¡Carrera completada!");
            Debug.Log($"  Precisión: {accuracy * 100:F1}%");
            Debug.Log($"  Velocidad promedio: {averageSpeed:F2}");
            Debug.Log($"  Movimientos perfectos: {perfectMoves}/{totalMoves}");
        }
        else
        {
            Debug.Log($"{bossName} (Profesional): Análisis de falla iniciado...");
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Visualizar predicción del jugador
        if (predictPlayerMovement && Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(predictedPlayerPosition, 1f);
            Gizmos.DrawLine(transform.position, predictedPlayerPosition);
        }

        // Visualizar secuencia activa
        if (sequenceActive && activeSequence != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = currentStepIndex; i < activeSequence.steps.Count && i < currentStepIndex + 3; i++)
            {
                if (activeSequence.steps[i].targetPosition != Vector3.zero)
                {
                    Gizmos.DrawSphere(activeSequence.steps[i].targetPosition, 0.3f);
                }
            }
        }
    }
}

/// <summary>
/// Secuencia de escalada predefinida
/// </summary>
[System.Serializable]
public class ClimbingSequence
{
    public string sequenceName;
    public string description;
    public List<SequenceStep> steps = new List<SequenceStep>();
}

/// <summary>
/// Paso individual en una secuencia
/// </summary>
[System.Serializable]
public class SequenceStep
{
    public SequenceActionType actionType;
    public Vector3 targetPosition;
    public float duration;
    public float startTime;
    public bool requiresPrecision;
}

public enum SequenceActionType
{
    Move,
    Climb,
    Hook,
    Rest,
    Sprint,
    WallJump
}

// Acciones optimizadas para el profesional
public class OptimizedMoveAction : MoveToClimbPointAction
{
    public OptimizedMoveAction() : base()
    {
        actionName = "OptimizedMove";
        cost = 0.8f; // Más eficiente
    }
}

public class PerfectClimbAction : ClimbAction
{
    public PerfectClimbAction() : base()
    {
        actionName = "PerfectClimb";
        cost = 1.2f;
    }
}

public class PreciseHookAction : UseHookAction
{
    public PreciseHookAction() : base()
    {
        actionName = "PreciseHook";
        cost = 1.5f;
    }
}

public class StrategicRestAction : RestAction
{
    public StrategicRestAction() : base()
    {
        actionName = "StrategicRest";
        cost = 2.5f; // Solo descansa cuando es estratégicamente necesario
    }
}
