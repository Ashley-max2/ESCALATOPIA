using UnityEngine;

public class PlayerClimbState : PlayerState
{
    public PlayerClimbState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Debug.Log("Enter Climb");
        _ctx.isClimbing = true;
        _ctx.Rb.useGravity = false;
        _ctx.Rb.velocity = Vector3.zero;
    }

    public override void UpdateState()
    {
        float v = _ctx.InputHandler.MoveZ;
        float h = _ctx.InputHandler.MoveX;

        Vector3 climbDir = new Vector3(0, v, 0); // Vertical climbing
        // Optional: Horizontal climbing on wall? For now just vertical.
        
        // Face the wall
        _ctx.transform.forward = -_ctx.wallNormal;
        
        _ctx.Rb.velocity = transform.up * v * _ctx.climbSpeed + transform.right * h * _ctx.climbSpeed;

        // Drain Stamina if implementing
        if (_ctx.Stamina != null)
        {
            // _ctx.Stamina.Drain...
        }
    }

    public override void ExitState()
    {
        _ctx.isClimbing = false;
        _ctx.Rb.useGravity = true;
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.InputHandler.JumpTriggered)
        {
            // Wall Jump?
            WallJump();
        }
        else if (!_ctx.InputHandler.ClimbHeld) // Stop climbing if released? OR if grounded
        {
             // Check if we hit ground or top?
             // For now, exit to air if requested
             SwitchState(_factory.Air());
        }
    }

    private Transform transform => _ctx.transform;

    private void WallJump()
    {
        // Jump away from wall
        Vector3 jumpDir = _ctx.wallNormal + Vector3.up; 
        _ctx.Rb.velocity = Vector3.zero;
        _ctx.Rb.AddForce(jumpDir.normalized * _ctx.wallJumpForce, ForceMode.Impulse);
        SwitchState(_factory.Air()); // Or specialized WallJumpState
    }
}
