using UnityEngine;

public class PlayerHookState : PlayerState
{
    public PlayerHookState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Debug.Log("Enter Hook State");
        // Control is yielded to HookSystem logic mostly, or we coordinate here.
    }

    public override void UpdateState()
    {
        // HookSystem handles RB physics during pull usually.
        // We just ensure we don't interfere.
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        // If HookSystem says it's done
        if (_ctx.HookSystem != null && !_ctx.HookSystem.IsHooking)
        {
            if (_ctx.isGrounded)
                SwitchState(_factory.Grounded());
            else
                SwitchState(_factory.Air());
        }
    }
}
