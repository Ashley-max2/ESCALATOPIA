using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Debug.Log("Enter Air");
    }

    public override void UpdateState()
    {
        // Air Control logic same as movement but maybe reduced?
         float x = _ctx.InputHandler.MoveX;
        float z = _ctx.InputHandler.MoveZ;
        
        Vector3 move = _ctx.CameraTransform.right * x + _ctx.CameraTransform.forward * z;
        move.y = 0;
        move.Normalize();

        if (move.magnitude > 0.1f)
        {
            // Reduced control in air
            _ctx.Rb.AddForce(move * 5f, ForceMode.Force);
            
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, toRotation, _ctx.rotationSmoothTime);
        }
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.isGrounded && _ctx.Rb.velocity.y <= 0.1f)
        {
            SwitchState(_factory.Grounded());
        }
        else if (_ctx.InputHandler.ClimbHeld && CheckWall())
        {
             SwitchState(_factory.Climb());
        }
    }
    
    private bool CheckWall()
    {
         RaycastHit hit;
         if (Physics.Raycast(_ctx.transform.position + Vector3.up * 0.5f, _ctx.transform.forward, out hit, _ctx.climbCheckDistance, _ctx.climbableMask))
         {
             _ctx.wallNormal = hit.normal;
             return true;
         }
         return false;
    }
}
