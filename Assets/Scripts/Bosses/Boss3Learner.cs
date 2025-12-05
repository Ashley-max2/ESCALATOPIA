using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// BOSS 3: "El Aprendiz" - Dificultad: Alta
/// Mecánica única: Utiliza Imitation Learning para aprender de acciones exitosas
/// IA: GOAP + Sistema de aprendizaje basado en recompensas
/// </summary>
public class Boss3Learner : BossAIBase
{
    [Header("Boss 3 Specific Settings")]
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private int maxMemorySize = 50;
    [SerializeField] private bool enableLearning = true;
    
    [Header("Learning System")]
    [SerializeField] private float explorationRate = 0.2f; // 20% de exploración
    [Range(0f, 1f)]
    [SerializeField] private float confidenceThreshold = 0.7f;

    // Sistema de memoria y aprendizaje
    private List<ActionMemory> actionMemory = new List<ActionMemory>();
    private Dictionary<string, float> actionSuccessRate = new Dictionary<string, float>();
    private Dictionary<string, int> actionAttempts = new Dictionary<string, int>();
    private ActionMemory lastAction = null;
    
    // Machine Learning simplificado
    private Dictionary<StateActionPair, float> qTable = new Dictionary<StateActionPair, float>();
    private float discountFactor = 0.9f;
    private string currentStateKey = "";

    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Configuración de aprendizaje
        aiIntelligence = 0.75f; // Alta inteligencia
        speedModifier = 1.1f; // Ligeramente más rápido
        decisionDelay = 0.3f; // Decisiones rápidas
        allowMistakes = true; // Puede cometer errores pero aprende de ellos
    }

    protected override void InitializeGOAP()
    {
        base.InitializeGOAP();
        
        // Inicializar tasas de éxito
        foreach (GOAPAction action in availableActions)
        {
            actionSuccessRate[action.actionName] = 0.5f; // Neutral al inicio
            actionAttempts[action.actionName] = 0;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!raceStarted) return;

        // Aprender de la acción anterior
        if (lastAction != null && enableLearning)
        {
            EvaluateLastAction();
        }

        // Actualizar Q-Learning
        if (enableLearning)
        {
            UpdateQLearning();
        }
    }

    protected override void ExecuteGOAP()
    {
        // Guardar estado actual
        currentStateKey = GetCurrentStateKey();

        // Decidir si explorar o explotar
        bool shouldExplore = Random.value < explorationRate;

        if (shouldExplore && enableLearning)
        {
            // Exploración: probar acciones aleatorias
            ExploreAction();
        }
        else
        {
            // Explotación: usar el conocimiento aprendido
            ExploitKnowledge();
        }
    }

    /// <summary>
    /// Explora acciones nuevas o poco probadas
    /// </summary>
    private void ExploreAction()
    {
        // Seleccionar acción con menos intentos
        GOAPAction leastTriedAction = null;
        int minAttempts = int.MaxValue;

        foreach (GOAPAction action in availableActions)
        {
            if (action.CheckProceduralPrecondition(this))
            {
                int attempts = actionAttempts.GetValueOrDefault(action.actionName, 0);
                if (attempts < minAttempts)
                {
                    minAttempts = attempts;
                    leastTriedAction = action;
                }
            }
        }

        if (leastTriedAction != null)
        {
            ExecuteAction(leastTriedAction);
            Debug.Log($"{bossName}: Explorando - {leastTriedAction.actionName}");
        }
    }

    /// <summary>
    /// Explota el conocimiento aprendido
    /// </summary>
    private void ExploitKnowledge()
    {
        // Usar Q-Learning para seleccionar la mejor acción
        GOAPAction bestAction = SelectBestActionFromQLearning();

        if (bestAction != null)
        {
            ExecuteAction(bestAction);
        }
        else
        {
            // Fallback al GOAP estándar
            base.ExecuteGOAP();
        }
    }

    /// <summary>
    /// Ejecuta una acción y la registra en memoria
    /// </summary>
    private void ExecuteAction(GOAPAction action)
    {
        Vector3 positionBefore = transform.position;
        float staminaBefore = currentStamina;
        float timeBefore = Time.time;

        bool success = action.Perform(this);

        // Registrar la acción en memoria
        ActionMemory memory = new ActionMemory
        {
            actionName = action.actionName,
            stateBefore = GetCurrentStateKey(),
            positionBefore = positionBefore,
            positionAfter = transform.position,
            staminaBefore = staminaBefore,
            staminaAfter = currentStamina,
            timeTaken = Time.time - timeBefore,
            wasSuccessful = success && !action.IsComplete(this)
        };

        AddToMemory(memory);
        lastAction = memory;

        actionAttempts[action.actionName]++;
        currentAction = action;
    }

    /// <summary>
    /// Evalúa la última acción realizada
    /// </summary>
    private void EvaluateLastAction()
    {
        if (lastAction == null) return;

        // Calcular recompensa
        float reward = CalculateReward(lastAction);

        // Actualizar tasa de éxito
        float currentRate = actionSuccessRate.GetValueOrDefault(lastAction.actionName, 0.5f);
        float newRate = currentRate + learningRate * (reward - currentRate);
        actionSuccessRate[lastAction.actionName] = Mathf.Clamp01(newRate);

        Debug.Log($"{bossName}: Aprendizaje - {lastAction.actionName} = {newRate:F2}");

        lastAction = null;
    }

    /// <summary>
    /// Calcula la recompensa de una acción
    /// </summary>
    private float CalculateReward(ActionMemory memory)
    {
        float reward = 0f;

        // Recompensa por acercarse al objetivo
        if (goalPoint != null)
        {
            float distanceBefore = Vector3.Distance(memory.positionBefore, goalPoint.position);
            float distanceAfter = Vector3.Distance(memory.positionAfter, goalPoint.position);
            
            if (distanceAfter < distanceBefore)
            {
                reward += 1f; // Progreso positivo
            }
            else
            {
                reward -= 0.5f; // Retroceso
            }
        }

        // Recompensa por eficiencia de resistencia
        float staminaUsed = memory.staminaBefore - memory.staminaAfter;
        if (staminaUsed < 0) // Recuperó resistencia
        {
            reward += 0.5f;
        }
        else if (staminaUsed > 20f) // Gastó mucha resistencia
        {
            reward -= 0.3f;
        }

        // Penalización por tiempo excesivo
        if (memory.timeTaken > 2f)
        {
            reward -= 0.2f;
        }

        // Recompensa por éxito
        if (memory.wasSuccessful)
        {
            reward += 0.5f;
        }

        return reward;
    }

    /// <summary>
    /// Actualiza la tabla Q-Learning
    /// </summary>
    private void UpdateQLearning()
    {
        if (currentAction == null) return;

        string stateKey = currentStateKey;
        string actionName = currentAction.actionName;
        StateActionPair pair = new StateActionPair(stateKey, actionName);

        // Obtener recompensa actual
        float reward = lastAction != null ? CalculateReward(lastAction) : 0f;

        // Obtener Q-value actual
        float currentQ = qTable.GetValueOrDefault(pair, 0f);

        // Obtener máximo Q-value del siguiente estado
        float maxNextQ = GetMaxQValue(GetCurrentStateKey());

        // Actualizar Q-value: Q(s,a) = Q(s,a) + α[r + γ·max(Q(s',a')) - Q(s,a)]
        float newQ = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
        qTable[pair] = newQ;
    }

    /// <summary>
    /// Obtiene el máximo Q-value para un estado
    /// </summary>
    private float GetMaxQValue(string stateKey)
    {
        float maxQ = float.MinValue;

        foreach (GOAPAction action in availableActions)
        {
            StateActionPair pair = new StateActionPair(stateKey, action.actionName);
            float q = qTable.GetValueOrDefault(pair, 0f);
            
            if (q > maxQ)
            {
                maxQ = q;
            }
        }

        return maxQ == float.MinValue ? 0f : maxQ;
    }

    /// <summary>
    /// Selecciona la mejor acción usando Q-Learning
    /// </summary>
    private GOAPAction SelectBestActionFromQLearning()
    {
        GOAPAction bestAction = null;
        float bestQ = float.MinValue;
        string stateKey = GetCurrentStateKey();

        foreach (GOAPAction action in availableActions)
        {
            if (!action.CheckProceduralPrecondition(this)) continue;

            StateActionPair pair = new StateActionPair(stateKey, action.actionName);
            float q = qTable.GetValueOrDefault(pair, 0f);

            // Combinar Q-value con tasa de éxito aprendida
            float successRate = actionSuccessRate.GetValueOrDefault(action.actionName, 0.5f);
            float combinedScore = q * 0.7f + successRate * 0.3f;

            if (combinedScore > bestQ)
            {
                bestQ = combinedScore;
                bestAction = action;
            }
        }

        return bestAction;
    }

    /// <summary>
    /// Genera una clave única para el estado actual
    /// </summary>
    private string GetCurrentStateKey()
    {
        bool hasStamina = currentStamina > 10f;
        bool lowStamina = currentStamina < maxStamina * 0.3f;
        bool onWall = IsOnClimbableWall();
        bool nearGoal = goalPoint != null && Vector3.Distance(transform.position, goalPoint.position) < 10f;

        return $"S:{hasStamina}|L:{lowStamina}|W:{onWall}|G:{nearGoal}";
    }

    /// <summary>
    /// Añade una acción a la memoria
    /// </summary>
    private void AddToMemory(ActionMemory memory)
    {
        actionMemory.Add(memory);

        // Limitar tamaño de memoria
        if (actionMemory.Count > maxMemorySize)
        {
            actionMemory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Obtiene estadísticas de aprendizaje
    /// </summary>
    public Dictionary<string, float> GetLearningStats()
    {
        return new Dictionary<string, float>(actionSuccessRate);
    }

    protected override void OnRaceStart()
    {
        Debug.Log($"{bossName} (Aprendiz): ¡Usaré todo lo que he aprendido!");
        
        // Reset parcial para nueva carrera (mantiene algo de conocimiento)
        if (enableLearning)
        {
            explorationRate = 0.15f; // Reducir exploración en carrera
        }
    }

    protected override void OnRaceEnd()
    {
        if (IsAtGoal())
        {
            Debug.Log($"{bossName} (Aprendiz): ¡El aprendizaje vale la pena!");
            
            // Incrementar confianza
            aiIntelligence = Mathf.Min(0.9f, aiIntelligence + 0.05f);
        }
        else
        {
            Debug.Log($"{bossName} (Aprendiz): Debo seguir aprendiendo...");
        }

        // Mostrar estadísticas
        Debug.Log($"{bossName} - Estadísticas de aprendizaje:");
        foreach (var stat in actionSuccessRate)
        {
            Debug.Log($"  {stat.Key}: {stat.Value:F2}");
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Visualizar memoria de acciones exitosas
        if (actionMemory != null && actionMemory.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var memory in actionMemory.TakeLast(10))
            {
                if (memory.wasSuccessful)
                {
                    Gizmos.DrawLine(memory.positionBefore, memory.positionAfter);
                }
            }
        }
    }
}

/// <summary>
/// Estructura para almacenar memoria de acciones
/// </summary>
[System.Serializable]
public class ActionMemory
{
    public string actionName;
    public string stateBefore;
    public Vector3 positionBefore;
    public Vector3 positionAfter;
    public float staminaBefore;
    public float staminaAfter;
    public float timeTaken;
    public bool wasSuccessful;
}

/// <summary>
/// Par estado-acción para Q-Learning
/// </summary>
public struct StateActionPair
{
    public string state;
    public string action;

    public StateActionPair(string s, string a)
    {
        state = s;
        action = a;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is StateActionPair)) return false;
        StateActionPair other = (StateActionPair)obj;
        return state == other.state && action == other.action;
    }

    public override int GetHashCode()
    {
        return state.GetHashCode() ^ action.GetHashCode();
    }
}
