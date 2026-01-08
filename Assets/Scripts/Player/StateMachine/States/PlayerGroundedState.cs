using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Debug.Log("Enter Grounded");
    }

    public override void UpdateState()
    {
        // Movement
        float x = _ctx.InputHandler.MoveX;
        float z = _ctx.InputHandler.MoveZ;
        bool isRunning = _ctx.InputHandler.SprintHeld;

        Vector3 move = _ctx.CameraTransform.right * x + _ctx.CameraTransform.forward * z;
        move.y = 0;
        move.Normalize();

        float speed = isRunning ? _ctx.runSpeed : _ctx.walkSpeed;
        
        Vector3 targetVelocity = move * speed;
        // Preserve Y velocity
        targetVelocity.y = _ctx.Rb.velocity.y;
        
        // Simple movement application (can be smoothed)
        _ctx.Rb.velocity = Vector3.Lerp(_ctx.Rb.velocity, targetVelocity, Time.deltaTime * 10f);

        // Rotation
        if (move.magnitude > 0.1f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, toRotation, _ctx.rotationSmoothTime);
        }
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.InputHandler.JumpTriggered && _ctx.isGrounded)
        {
            SwitchState(_factory.Jump());
        }
        else if (!_ctx.isGrounded)
        {
            SwitchState(_factory.Air());
        }
        else if (_ctx.InputHandler.ClimbHeld && CheckClimb())
        {
            SwitchState(_factory.Climb());
        }
    }

    private bool CheckClimb()
    {
        // Raycast forward to check for climbable
         RaycastHit hit;
         if (Physics.Raycast(_ctx.transform.position + Vector3.up * 0.5f, _ctx.transform.forward, out hit, _ctx.climbCheckDistance, _ctx.climbableMask))
         {
             _ctx.wallNormal = hit.normal;
             return true;
         }
         return false;
    }
}
