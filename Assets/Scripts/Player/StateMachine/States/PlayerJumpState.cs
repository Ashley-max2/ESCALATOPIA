using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Debug.Log("Enter Jump");
        _ctx.Rb.velocity = new Vector3(_ctx.Rb.velocity.x, 0, _ctx.Rb.velocity.z); // Reset Y
        _ctx.Rb.AddForce(Vector3.up * _ctx.jumpForce, ForceMode.Impulse);
    }

    public override void UpdateState()
    {
        // Allow some air control?
        SwitchState(_factory.Air());
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
         // Handled in Update
    }
}
