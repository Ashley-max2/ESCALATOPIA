using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss Manager y Controlador de IA configurable mediante el Inspector.
/// Cumple todos los requisitos avanzados de escalada, pathfinding y GOAP/Scripting.
/// </summary>
public class BossAIController : MonoBehaviour
{
    // --- ENUMS PARA COMPORTAMIENTO ---
    public enum AITechnique
    {
        AdvancedGOAP,
        VariableSimulated,
        ProfessionalScripted
    }

    public enum ActionState { Idle, Running, Climbing, Hooking }

    [Header("=== TIPO DE INTELIGENCIA ARTIFICIAL ===")]
    public AITechnique aiType = AITechnique.AdvancedGOAP;

    [Header("=== HABILIDADES DEL BOSS ===")]
    [Tooltip("¿Puede correr hacia el objetivo?")]
    public bool canRun = true;
    [Tooltip("¿Puede usar paredes para escalar obstáculos?")]
    public bool canClimb = true;
    [Tooltip("¿Puede usar el gancho para grandes distancias?")]
    public bool canUseHook = true;

    [Header("=== MODIFICADORES DE IA (PATHFINDING) ===")]
    [Tooltip("Modificador de Inteligencia: 1.0 = Escoge el camino perfecto siempre. 0.0 = Toma el camino más ineficiente/tonto (se desvía o equivoca).")]
    [Range(0f, 1f)]
    public float intelligenceLevel = 0.8f;

    [Tooltip("Modificador de Lentitud/Velocidad de reacción. Multiplica el tiempo de pensar de la IA.")]
    [Range(0.1f, 3f)]
    public float reactionTimeMultiplier = 1f;

    [Tooltip("Velocidad de movimiento física base del Boss.")]
    public float baseMoveSpeed = 5f;

    [Header("=== RUTAS Y NAVEGACIÓN ===")]
    [Tooltip("Nodos posibles por los que el boss puede navegar dinámicamente.")]
    public List<Transform> dynamicWaypoints;

    [Tooltip("Secuencia EXACTA y FIJA que usará si la IA es 'ProfessionalScripted'.")]
    public List<Transform> professionalFixedRoute;

    [Tooltip("Tiempos estrictos (en segundos) que el profesional tarda entre cada punto fijo.")]
    public List<float> professionalWaitTimes;

    // Variables internas
    private ActionState currentState = ActionState.Idle;
    private Transform currentTarget;
    private int professionalIndex = 0;
    private bool isThinking = false;

    // Para evitar que la IA oscile entre puntos que ya ha visitado (hacia adelante y hacia atrás)
    private HashSet<Transform> visitedDynamicNodes = new HashSet<Transform>();

    private Coroutine aiCoroutine;
    private Animator animator;

    private void OnEnable()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        professionalIndex = 0;
        isThinking = false;
        currentState = ActionState.Idle;
        visitedDynamicNodes.Clear();

        if (animator != null)
            animator.SetBool("Run", false);

        aiCoroutine = StartCoroutine(AILogicLoop());
    }

    private void OnDisable()
    {
        if (aiCoroutine != null)
        {
            StopCoroutine(aiCoroutine);
            aiCoroutine = null;
        }
        StopAllCoroutines();
    }

    private IEnumerator AILogicLoop()
    {
        while (true)
        {
            if (!isThinking)
            {
                isThinking = true;

                if (aiType == AITechnique.ProfessionalScripted)
                {
                    // El profesional tiene su propia corrutina que respeta los tiempos de llegada
                    StartCoroutine(ExecuteProfessionalRoutine());
                }
                else
                {
                    float thinkingDelay = Random.Range(0.2f, 1f) * reactionTimeMultiplier;
                    thinkingDelay += (1f - intelligenceLevel) * 2f;

                    yield return new WaitForSeconds(thinkingDelay);

                    if (aiType == AITechnique.AdvancedGOAP)
                    {
                        ExecuteGOAPDecisions();
                    }
                    else if (aiType == AITechnique.VariableSimulated)
                    {
                        ExecuteVariableSequence();
                    }

                    isThinking = false;
                }
            }

            PerformMovement();
            yield return null;
        }
    }

    private void ExecuteGOAPDecisions()
    {
        if (dynamicWaypoints.Count == 0) return;

        currentTarget = ChooseBestPath(dynamicWaypoints);
        if (currentTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        float heightDifference = currentTarget.position.y - transform.position.y;

        float scoreRun = canRun && heightDifference < 2f ? 100f - distanceToTarget : 0f;
        float scoreClimb = canClimb && heightDifference >= 2f ? 80f : 0f;
        float scoreHook = canUseHook && distanceToTarget > 15f && heightDifference > 5f ? 150f : 0f;

        if (scoreHook > scoreRun && scoreHook > scoreClimb)
            SetState(ActionState.Hooking);
        else if (scoreClimb > scoreRun)
            SetState(ActionState.Climbing);
        else if (scoreRun > 0)
            SetState(ActionState.Running);
        else
            SetState(ActionState.Idle);
    }

    private Transform ChooseBestPath(List<Transform> nodes)
    {
        Transform bestNode = null;
        float bestScore = float.MaxValue;

        foreach (Transform node in nodes)
        {
            // Ignorar nodos que ya hemos visitado en esta vuelta
            if (visitedDynamicNodes.Contains(node)) continue;

            float realCost = Vector3.Distance(transform.position, node.position);

            float errorMargin = (1f - intelligenceLevel) * Random.Range(10f, 50f);
            float perceivedCost = realCost + errorMargin;

            if (perceivedCost < bestScore)
            {
                bestScore = perceivedCost;
                bestNode = node;
            }
        }

        // Si ya visitó todos los nodos, resetea la memoria para que pueda volver a patrullar
        if (bestNode == null && nodes.Count > 0)
        {
            visitedDynamicNodes.Clear();
            return ChooseBestPath(nodes);
        }

        return bestNode;
    }

    private void ExecuteVariableSequence()
    {
        if (dynamicWaypoints.Count == 0) return;

        List<Transform> validNodes = new List<Transform>();
        foreach (var node in dynamicWaypoints)
        {
            if (!visitedDynamicNodes.Contains(node)) validNodes.Add(node);
        }

        if (validNodes.Count == 0)
        {
            visitedDynamicNodes.Clear();
            validNodes = dynamicWaypoints;
        }

        currentTarget = validNodes[Random.Range(0, validNodes.Count)];

        List<ActionState> possibleActions = new List<ActionState>();
        if (canRun) possibleActions.Add(ActionState.Running);
        if (canClimb) possibleActions.Add(ActionState.Climbing);
        if (canUseHook) possibleActions.Add(ActionState.Hooking);

        if (possibleActions.Count > 0)
        {
            ActionState randomAction = possibleActions[Random.Range(0, possibleActions.Count)];
            SetState(randomAction);
        }
    }

    private IEnumerator ExecuteProfessionalRoutine()
    {
        if (professionalFixedRoute.Count == 0 || professionalWaitTimes.Count != professionalFixedRoute.Count)
        {
            Debug.LogError("Ruta profesional mal configurada.");
            yield break;
        }

        while (true)
        {
            currentTarget = professionalFixedRoute[professionalIndex];

            if (currentTarget.position.y > transform.position.y + 2f)
                SetState(canClimb ? ActionState.Climbing : ActionState.Hooking);
            else
                SetState(ActionState.Running);

            // 1. ESPERA HASTA LLEGAR FÍSICAMENTE AL PUNTO
            while (Vector3.Distance(transform.position, currentTarget.position) > 0.5f)
            {
                yield return null;
            }

            // 2. YA HA LLEGADO. ESPERA SU TIEMPO DE PARADA FIJO
            SetState(ActionState.Idle);
            yield return new WaitForSeconds(professionalWaitTimes[professionalIndex]);

            // Avanzar al siguiente punto o detenerse si es el último
            if (professionalIndex < professionalFixedRoute.Count - 1)
            {
                professionalIndex++;
            }
            else
            {
                yield break; // Termina la rutina en el último punto
            }
        }
    }

    private void PerformMovement()
    {
        if (currentTarget == null || currentState == ActionState.Idle) return;

        float currentSpeed = baseMoveSpeed * intelligenceLevel;

        // Rotar al Boss para que SIEMPRE MIRE HACIA ADELANTE (Hacia su objetivo)
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        directionToTarget.y = 0; // Evitar que el personaje mire hacia el cielo al caminar

        if (directionToTarget.sqrMagnitude > 0.01f && currentState == ActionState.Running)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        switch (currentState)
        {
            case ActionState.Running:
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, currentSpeed * Time.deltaTime);
                break;

            case ActionState.Climbing:
                currentSpeed *= 0.6f;
                Vector3 climbTarget = new Vector3(transform.position.x, currentTarget.position.y, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, climbTarget, currentSpeed * Time.deltaTime);
                break;

            case ActionState.Hooking:
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, currentSpeed * 3f * Time.deltaTime);
                break;
        }

        // Marcar el nodo actual como visitado en IA dinámica si estamos cerca para que pase al siguiente
        if (aiType != AITechnique.ProfessionalScripted && Vector3.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            visitedDynamicNodes.Add(currentTarget);
            isThinking = false; // Forzar que recapacite y busque el siguiente punto inmediatamente
        }
    }

    private void SetState(ActionState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;

            // Actualizar Animator: Run activo si el boss se mueve (no Idle)
            if (animator != null)
                animator.SetBool("Run", newState != ActionState.Idle);

            Debug.Log($"BossAI [{gameObject.name}]: Ha cambiado al estado -> {newState}");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 1f);
        }
    }
}
