using UnityEngine;

/// <summary>
/// Abstract base class for all states.
/// Provides common functionality and follows Open/Closed Principle.
/// </summary>
/// <typeparam name="TContext">The context type for this state</typeparam>
public abstract class StateBase<TContext> : IState<TContext>
{
    /// <summary>
    /// The name of this state for debugging purposes.
    /// </summary>
    public virtual string StateName => GetType().Name;

    /// <summary>
    /// Called when entering this state.
    /// Override to add custom enter logic.
    /// </summary>
    public virtual void Enter(TContext context)
    {
        Debug.Log($"[STATE] Entering {StateName}");
    }

    /// <summary>
    /// Called every frame while in this state.
    /// Override to add custom update logic.
    /// </summary>
    public abstract void Update(TContext context);

    /// <summary>
    /// Called when exiting this state.
    /// Override to add custom exit logic.
    /// </summary>
    public virtual void Exit(TContext context)
    {
        Debug.Log($"[STATE] Exiting {StateName}");
    }

    /// <summary>
    /// Helper method to check if a value is approximately zero.
    /// </summary>
    protected bool IsApproximatelyZero(float value, float threshold = 0.1f)
    {
        return Mathf.Abs(value) < threshold;
    }

    /// <summary>
    /// Helper method to check if a vector is approximately zero.
    /// </summary>
    protected bool IsApproximatelyZero(Vector3 value, float threshold = 0.1f)
    {
        return value.magnitude < threshold;
    }
}
