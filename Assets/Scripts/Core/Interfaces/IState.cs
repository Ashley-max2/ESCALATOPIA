/// <summary>
/// Generic state interface for state machines.
/// Replaces the old IState that was tied to PlayerController.
/// </summary>
/// <typeparam name="TContext">The context type (e.g., PlayerController, BossAI)</typeparam>
public interface IState<TContext>
{
    /// <summary>
    /// Called when entering this state.
    /// </summary>
    /// <param name="context">The state machine context</param>
    void Enter(TContext context);

    /// <summary>
    /// Called every frame while in this state.
    /// </summary>
    /// <param name="context">The state machine context</param>
    void Update(TContext context);

    /// <summary>
    /// Called when exiting this state.
    /// </summary>
    /// <param name="context">The state machine context</param>
    void Exit(TContext context);

    /// <summary>
    /// Get the name of this state for debugging.
    /// </summary>
    string StateName { get; }
}
