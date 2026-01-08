using UnityEngine;

public abstract class PlayerState
{
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;

    public PlayerState(PlayerStateMachine ctx, PlayerStateFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();

    protected void SwitchState(PlayerState newState)
    {
        _ctx.CurrentState.ExitState();
        _ctx.CurrentState = newState;
        _ctx.CurrentState.EnterState();
    }
}
