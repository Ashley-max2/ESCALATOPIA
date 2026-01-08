using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Acción base para el sistema GOAP (Goal-Oriented Action Planning)
/// </summary>
public abstract class GOAPAction
{
    public string actionName;
    public float cost = 1f;
    public bool isInterruptible = true;

    // Precondiciones y efectos (protected backing fields)
    protected Dictionary<string, object> preconditions = new Dictionary<string, object>();
    protected Dictionary<string, object> effects = new Dictionary<string, object>();

    public GOAPAction(string name)
    {
        actionName = name;
    }

    public Dictionary<string, object> Preconditions => preconditions;
    public Dictionary<string, object> Effects => effects;

    /// <summary>
    /// Verifica si la acción puede ejecutarse dado el estado del mundo
    /// </summary>
    public bool CheckPreconditions(Dictionary<string, bool> state)
    {
        foreach (KeyValuePair<string, object> pre in preconditions)
        {
            // Si el estado no contiene la clave, no se cumple
            if (!state.ContainsKey(pre.Key)) 
                return false;

            // Verificar valor (asumiendo booleanos para simplificar integración)
            bool stateVal = state[pre.Key];
            bool preVal = false;
             
            if (pre.Value is bool)
                preVal = (bool)pre.Value;
            
            if (stateVal != preVal)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Verifica si la acción puede ejecutarse en el contexto actual (chequeos de juego)
    /// </summary>
    public abstract bool CheckProceduralPrecondition(BossAIBase agent);

    /// <summary>
    /// Ejecuta la acción
    /// </summary>
    public abstract bool Perform(BossAIBase agent);

    /// <summary>
    /// Verifica si la acción se completó
    /// </summary>
    public abstract bool IsComplete(BossAIBase agent);

    /// <summary>
    /// Reinicia el estado de la acción
    /// </summary>
    public virtual void Reset() { }
}

/// <summary>
/// Acción: Moverse al siguiente punto de escalada
/// </summary>
public class MoveToClimbPointAction : GOAPAction
{
    private Vector3 targetPosition;
    private float arrivalThreshold = 0.5f;

    public MoveToClimbPointAction() : base("MoveToClimbPoint")
    {
        cost = 1f;
        preconditions.Add("hasStamina", true);
        effects.Add("atClimbPoint", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        return agent.CurrentStamina > 0 && agent.NextClimbPoint != null;
    }

    public override bool Perform(BossAIBase agent)
    {
        if (agent.NextClimbPoint == null) return false;

        targetPosition = agent.NextClimbPoint.position;
        agent.MoveTowards(targetPosition);
        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        if (agent.NextClimbPoint == null) return true;
        return Vector3.Distance(agent.transform.position, agent.NextClimbPoint.position) < arrivalThreshold;
    }
}

/// <summary>
/// Acción: Usar gancho para alcanzar punto distante
/// </summary>
public class UseHookAction : GOAPAction
{
    private bool hookLaunched = false;

    public UseHookAction() : base("UseHook")
    {
        cost = 2f;
        preconditions.Add("hasStamina", true);
        preconditions.Add("hookPointAvailable", true);
        effects.Add("atHookPoint", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        return agent.CurrentStamina > 10f && agent.FindNearestHookPoint() != null;
    }

    public override bool Perform(BossAIBase agent)
    {
        Transform hookPoint = agent.FindNearestHookPoint();
        if (hookPoint == null) return false;

        if (!hookLaunched)
        {
            agent.LaunchHook(hookPoint.position);
            hookLaunched = true;
        }

        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        return !agent.IsHooking();
    }

    public override void Reset()
    {
        hookLaunched = false;
    }
}

/// <summary>
/// Acción: Descansar para recuperar resistencia
/// </summary>
public class RestAction : GOAPAction
{
    private float restTime = 0f;
    private float requiredRestTime = 2f;

    public RestAction() : base("Rest")
    {
        cost = 3f;
        preconditions.Add("lowStamina", true);
        effects.Add("hasStamina", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        return agent.CurrentStamina < agent.MaxStamina * 0.3f;
    }

    public override bool Perform(BossAIBase agent)
    {
        agent.Rest();
        restTime += Time.deltaTime;
        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        return restTime >= requiredRestTime || agent.CurrentStamina >= agent.MaxStamina * 0.7f;
    }

    public override void Reset()
    {
        restTime = 0f;
    }
}

/// <summary>
/// Acción: Escalar verticalmente
/// </summary>
public class ClimbAction : GOAPAction
{
    public ClimbAction() : base("Climb")
    {
        cost = 1.5f;
        preconditions.Add("hasStamina", true);
        preconditions.Add("onWall", true);
        effects.Add("gainedHeight", true);
    }

    public override bool CheckProceduralPrecondition(BossAIBase agent)
    {
        return agent.CurrentStamina > 5f && agent.IsOnClimbableWall();
    }

    public override bool Perform(BossAIBase agent)
    {
        agent.Climb();
        return true;
    }

    public override bool IsComplete(BossAIBase agent)
    {
        // Se completa cuando llegamos a un nuevo punto de agarre
        return !agent.IsOnClimbableWall() || agent.CurrentStamina <= 0;
    }
}
