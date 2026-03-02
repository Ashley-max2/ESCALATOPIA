using UnityEngine;

/// <summary>
/// Estado de salto del jugador.
/// Aplica fuerza de salto y transiciona a estado de aire.
/// Incluye jump buffering y coyote time.
/// </summary>
public class PlayerJumpState : PlayerBaseState
{
    private bool _jumpApplied;
    
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        _jumpApplied = false;
        ctx.LastJumpPressTime = 0; // Consume the buffered jump
    }
    
    public override void Execute()
    {
        // Short state - just applies jump and transitions
    }
    
    public override void FixedExecute()
    {
        if (!_jumpApplied)
        {
            ApplyJump();
            _jumpApplied = true;
            SwitchState(factory.Airborne());
        }
    }
    
    public override void Exit()
    {
    }
    
    private void ApplyJump()
    {
        // Reset vertical velocity before jump
        Vector3 vel = ctx.Rb.velocity;
        vel.y = 0;
        ctx.Rb.velocity = vel;
        
        // Apply jump force
        ctx.Rb.AddForce(Vector3.up * ctx.JumpForce, ForceMode.Impulse);
        
        // Set fall start height for tracking
        ctx.FallStartHeight = ctx.transform.position.y;
        
        Debug.Log("Jump!");
    }
}
