using UnityEngine;

/// <summary>
/// Estado de salto desde pared (Wall Jump).
/// Aplica fuerza en dirección opuesta a la pared más fuerza hacia arriba.
/// </summary>
public class PlayerWallJumpState : PlayerBaseState
{
    private bool _jumpApplied;
    private float _wallJumpTimer;
    private const float WALL_JUMP_LOCK_DURATION = 0.2f; // Tiempo mínimo antes de poder re-escalar
    
    public PlayerWallJumpState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        _jumpApplied = false;
        _wallJumpTimer = 0;
        
        // Consume stamina for wall jump
        if (ctx.Stamina != null)
        {
            ctx.Stamina.ConsumeStamina(ctx.ClimbStaminaCost * 2f);
        }
    }
    
    public override void Execute()
    {
        _wallJumpTimer += Time.deltaTime;
        
        // Transition after brief lock
        if (_jumpApplied && _wallJumpTimer >= WALL_JUMP_LOCK_DURATION)
        {
            SwitchState(factory.Airborne());
        }
    }
    
    public override void FixedExecute()
    {
        if (!_jumpApplied)
        {
            ApplyWallJump();
            _jumpApplied = true;
        }
    }
    
    public override void Exit()
    {
    }
    
    private void ApplyWallJump()
    {
        // Reset velocity
        ctx.Rb.velocity = Vector3.zero;
        
        // Calculate jump direction (away from wall + up)
        Vector3 jumpDirection = ctx.WallNormal * ctx.WallJumpForce + Vector3.up * ctx.WallJumpUpwardForce;
        
        // Apply force
        ctx.Rb.AddForce(jumpDirection, ForceMode.Impulse);
        
        // Rotate to face jump direction
        Vector3 facingDir = new Vector3(ctx.WallNormal.x, 0, ctx.WallNormal.z).normalized;
        if (facingDir.magnitude > 0.1f)
        {
            ctx.transform.rotation = Quaternion.LookRotation(facingDir);
        }
        
        // Set fall tracking from new position
        ctx.FallStartHeight = ctx.transform.position.y;
        
        Debug.Log("Wall Jump!");
    }
}
