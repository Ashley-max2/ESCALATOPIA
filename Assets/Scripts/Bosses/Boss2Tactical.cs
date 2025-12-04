using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BOSS 2: "El Táctico" - Dificultad: Media
/// Mecánica única: Evalúa múltiples rutas y elige la más eficiente según el contexto
/// IA: GOAP con evaluación de rutas y decisiones tácticas
/// </summary>
public class Boss2Tactical : BossAIBase
{
    [Header("Boss 2 Specific Settings")]
    [SerializeField] private float routeEvaluationInterval = 5f; // Reevalúa rutas cada 5 segundos
    [SerializeField] private int routeAlternatives = 3; // Evalúa 3 rutas diferentes
    [SerializeField] private bool adaptToPlayer = true; // Se adapta a la posición del jugador
    
    [Header("Tactical Preferences")]
    [Range(0f, 1f)]
    [SerializeField] private float speedPreference = 0.6f; // Preferencia por velocidad vs seguridad
    [Range(0f, 1f)]
    [SerializeField] private float staminaConservation = 0.5f; // Cuánto conserva la resistencia

    private float lastRouteEvaluation = 0f;
    private List<List<ClimbPoint>> evaluatedRoutes = new List<List<ClimbPoint>>();
    private Transform playerTransform;
    private TacticalDecision currentDecision = TacticalDecision.Balanced;

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Configuración táctica
        aiIntelligence = 0.6f; // Inteligencia media
        speedModifier = 1.0f; // Velocidad normal
        decisionDelay = 0.4f; // Decisiones moderadas
        allowMistakes = true; // Algunos errores menores
    }

    protected override void InitializeGOAP()
    {
        base.InitializeGOAP();
        
        // El táctico usa todas las acciones disponibles
        availableActions.Clear();
        availableActions.Add(new MoveToClimbPointAction());
        availableActions.Add(new ClimbAction());
        availableActions.Add(new UseHookAction());
        availableActions.Add(new RestAction());
        availableActions.Add(new TacticalRouteEvaluationAction());
    }

    protected override void Start()
    {
        base.Start();
        
        // Buscar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!raceStarted) return;

        // Reevaluar rutas periódicamente
        if (Time.time - lastRouteEvaluation > routeEvaluationInterval)
        {
            EvaluateRoutes();
            lastRouteEvaluation = Time.time;
        }

        // Adaptarse al jugador
        if (adaptToPlayer && playerTransform != null)
        {
            AdaptToPlayerPosition();
        }

        // Ajustar decisión táctica según el contexto
        UpdateTacticalDecision();
    }

    /// <summary>
    /// Evalúa múltiples rutas y elige la mejor
    /// </summary>
    private void EvaluateRoutes()
    {
        if (pathfinder == null || goalPoint == null) return;

        evaluatedRoutes.Clear();

        // Generar diferentes rutas con diferentes parámetros
        for (int i = 0; i < routeAlternatives; i++)
        {
            float difficultyMod = Random.Range(0.3f, 1.2f);
            List<ClimbPoint> route = pathfinder.FindPath(transform.position, goalPoint.position, difficultyMod);
            
            if (route != null)
            {
                evaluatedRoutes.Add(route);
            }
        }

        // Seleccionar la mejor ruta según preferencias
        if (evaluatedRoutes.Count > 0)
        {
            currentPath = SelectBestRoute();
            currentPathIndex = 0;
            UpdateNextClimbPoint();
        }

        Debug.Log($"{bossName}: Evaluadas {evaluatedRoutes.Count} rutas alternativas");
    }

    /// <summary>
    /// Selecciona la mejor ruta según las preferencias tácticas
    /// </summary>
    private List<ClimbPoint> SelectBestRoute()
    {
        List<ClimbPoint> bestRoute = null;
        float bestScore = float.MinValue;

        foreach (List<ClimbPoint> route in evaluatedRoutes)
        {
            float score = EvaluateRouteScore(route);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestRoute = route;
            }
        }

        return bestRoute;
    }

    /// <summary>
    /// Evalúa una ruta según múltiples factores
    /// </summary>
    private float EvaluateRouteScore(List<ClimbPoint> route)
    {
        if (route == null || route.Count == 0) return float.MinValue;

        float totalDistance = 0f;
        float totalDifficulty = 0f;
        int hookPoints = 0;
        int restPoints = 0;

        for (int i = 0; i < route.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(route[i].position, route[i + 1].position);
            totalDifficulty += route[i].difficulty;
            
            if (route[i].type == ClimbPointType.HookPoint) hookPoints++;
            if (route[i].type == ClimbPointType.LedgeRest) restPoints++;
        }

        // Calcular puntuación balanceada
        float distanceScore = 1f / (totalDistance + 1f); // Preferir rutas cortas
        float difficultyScore = 1f / (totalDifficulty + 1f); // Preferir rutas fáciles
        float hookScore = hookPoints * 0.5f; // Bonificación por puntos de gancho
        float restScore = restPoints * 0.3f; // Bonificación por puntos de descanso

        // Aplicar preferencias
        float score = (distanceScore * speedPreference) + 
                     (difficultyScore * (1f - speedPreference)) +
                     hookScore + 
                     (restScore * staminaConservation);

        return score;
    }

    /// <summary>
    /// Se adapta a la posición del jugador
    /// </summary>
    private void AdaptToPlayerPosition()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Si el jugador está adelante, aumentar velocidad
        if (IsPlayerAhead())
        {
            speedModifier = Mathf.Lerp(speedModifier, 1.3f, Time.deltaTime * 0.5f);
            currentDecision = TacticalDecision.Aggressive;
        }
        // Si estamos adelante, mantener velocidad normal
        else
        {
            speedModifier = Mathf.Lerp(speedModifier, 1.0f, Time.deltaTime * 0.5f);
            currentDecision = TacticalDecision.Balanced;
        }
    }

    /// <summary>
    /// Verifica si el jugador está adelante
    /// </summary>
    private bool IsPlayerAhead()
    {
        if (playerTransform == null || goalPoint == null) return false;

        float playerDistanceToGoal = Vector3.Distance(playerTransform.position, goalPoint.position);
        float bossDistanceToGoal = Vector3.Distance(transform.position, goalPoint.position);

        return playerDistanceToGoal < bossDistanceToGoal;
    }

    /// <summary>
    /// Actualiza la decisión táctica según el contexto
    /// </summary>
    private void UpdateTacticalDecision()
    {
        // Evaluar resistencia
        if (currentStamina < maxStamina * 0.25f)
        {
            currentDecision = TacticalDecision.Conservative;
            speedModifier = 0.7f;
        }
        else if (currentStamina > maxStamina * 0.75f && IsPlayerAhead())
        {
            currentDecision = TacticalDecision.Aggressive;
            speedModifier = 1.2f;
        }
        else
        {
            currentDecision = TacticalDecision.Balanced;
            speedModifier = 1.0f;
        }
    }

    public override void Climb()
    {
        base.Climb();

        // Ajustar consumo de resistencia según decisión táctica
        float staminaMultiplier = 1f;
        switch (currentDecision)
        {
            case TacticalDecision.Conservative:
                staminaMultiplier = 0.7f; // Consume menos
                break;
            case TacticalDecision.Aggressive:
                staminaMultiplier = 1.3f; // Consume más
                break;
            default:
                staminaMultiplier = 1f;
                break;
        }

        currentStamina -= staminaDrainRate * staminaMultiplier * Time.deltaTime;
    }

    protected override void HandleNoPlanFound()
    {
        // El táctico intenta evaluar rutas alternativas
        EvaluateRoutes();
        
        if (currentPath == null || currentPath.Count == 0)
        {
            base.HandleNoPlanFound();
        }
    }

    protected override void OnRaceStart()
    {
        Debug.Log($"{bossName} (Táctico): Analizando rutas... ¡Encontraré el mejor camino!");
        lastRouteEvaluation = 0f;
        EvaluateRoutes();
    }

    protected override void OnRaceEnd()
    {
        if (IsAtGoal())
        {
            Debug.Log($"{bossName} (Táctico): ¡La estrategia funcionó perfectamente!");
        }
        else
        {
            Debug.Log($"{bossName} (Táctico): Necesito revisar mi análisis...");
        }
    }
}

/// <summary>
/// Acción táctica personalizada para evaluar rutas
/// </summary>
public class TacticalRouteEvaluationAction : GOAPAction
{
    public TacticalRouteEvaluationAction() : base("EvaluateRoute")
    {
        cost = 0.5f;
        effects.Add("routeEvaluated", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        return true; // Siempre puede evaluar
    }

    public override bool Perform(BossAIBase agent)
    {
        // Esta acción es manejada por el boss directamente
        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        return true;
    }
}

public enum TacticalDecision
{
    Conservative,  // Conservar energía, rutas seguras
    Balanced,      // Balance entre velocidad y seguridad
    Aggressive     // Máxima velocidad, asumir riesgos
}
