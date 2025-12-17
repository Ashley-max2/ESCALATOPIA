/// <summary>
/// Generic state machine interface following Open/Closed Principle.
/// Can be implemented for different state machine contexts.
/// </summary>
/// <typeparam name="TContext">The context type for the state machine</typeparam>
public interface IStateMachine<TContext>
{
    /// <summary>
    /// Get the current state.
    /// </summary>
    IState<TContext> CurrentState { get; }

    /// <summary>
    /// Change to a new state.
    /// </summary>
    /// <param name="newState">The new state to transition to</param>
    void ChangeState(IState<TContext> newState);

    /// <summary>
    /// Update the current state.
    /// </summary>
    void UpdateStateMachine();

    /// <summary>
    /// Check if currently in a specific state type.
    /// </summary>
    /// <typeparam name="TState">The state type to check</typeparam>
    /// <returns>True if in the specified state</returns>
    bool IsInState<TState>() where TState : IState<TContext>;

    /// <summary>
    /// Get the name of the current state.
    /// </summary>
    string GetCurrentStateName();
}
