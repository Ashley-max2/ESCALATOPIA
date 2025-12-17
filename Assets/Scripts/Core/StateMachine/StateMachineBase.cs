using UnityEngine;

/// <summary>
/// Generic state machine implementation following Open/Closed Principle.
/// Can be used for any context type (Player, Boss AI, etc.).
/// </summary>
/// <typeparam name="TContext">The context type for this state machine</typeparam>
public class StateMachineBase<TContext> : IStateMachine<TContext>
{
    private IState<TContext> currentState;
    private readonly TContext context;

    /// <summary>
    /// Get the current state.
    /// </summary>
    public IState<TContext> CurrentState => currentState;

    /// <summary>
    /// Constructor for the state machine.
    /// </summary>
    /// <param name="context">The context object for this state machine</param>
    public StateMachineBase(TContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Change to a new state.
    /// </summary>
    /// <param name="newState">The new state to transition to</param>
    public void ChangeState(IState<TContext> newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("[StateMachine] Attempted to change to null state");
            return;
        }

        string oldStateName = currentState?.StateName ?? "None";
        string newStateName = newState.StateName;

        Debug.Log($"[StateMachine] Transitioning: {oldStateName} -> {newStateName}");

        // Exit current state
        currentState?.Exit(context);

        // Change state
        currentState = newState;

        // Enter new state
        currentState?.Enter(context);
    }

    /// <summary>
    /// Update the current state.
    /// Should be called every frame.
    /// </summary>
    public void UpdateStateMachine()
    {
        currentState?.Update(context);
    }

    /// <summary>
    /// Check if currently in a specific state type.
    /// </summary>
    /// <typeparam name="TState">The state type to check</typeparam>
    /// <returns>True if in the specified state</returns>
    public bool IsInState<TState>() where TState : IState<TContext>
    {
        return currentState is TState;
    }

    /// <summary>
    /// Get the name of the current state.
    /// </summary>
    public string GetCurrentStateName()
    {
        return currentState?.StateName ?? "None";
    }

    /// <summary>
    /// Force set the initial state without calling Enter.
    /// Useful for initialization.
    /// </summary>
    /// <param name="initialState">The initial state</param>
    public void SetInitialState(IState<TContext> initialState)
    {
        currentState = initialState;
        currentState?.Enter(context);
    }
}
