using UnityEngine;

/// <summary>
/// Estado de salto del jugador.
/// Aplica fuerza de salto y transiciona a estado de aire.
/// Incluye jump buffering y coyote time.
/// </summary>
public class PlayerJumpState : PlayerBaseState
{
    private bool _jumpApplied;
    private float _jumpDelay = 1f; // Delay antes de aplicar el salto (en segundos)
    private float _jumpTimer = 0f;
    
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        _jumpApplied = false;
        _jumpTimer = 0f; // Reset timer cuando entramos al estado Jump
        ctx.LastJumpPressTime = 0; // Consume the buffered jump
        
        // Activar parámetro Jump para la transición a StartJump en el Animator Controller
        ctx.Animator.SetBool("Jump", true);
    }
    
    public override void Execute()
    {
        // Short state - just applies jump and transitions
    }
    
    public override void FixedExecute()
    {
        // Permitir movimiento horizontal mientras espera el delay
        HandleMovement();
        
        if (!_jumpApplied)
        {
            // Incrementar timer
            _jumpTimer += Time.fixedDeltaTime;
            
            // Aplicar salto cuando se alcance el delay
            if (_jumpTimer >= _jumpDelay)
            {
                ApplyJump();
                _jumpApplied = true;
                
                // Desactivar Jump para la transición a JumpLoop
                ctx.Animator.SetBool("Jump", false);
                
                SwitchState(factory.Airborne());
            }
        }
    }
    
    private void HandleMovement()
    {
        // Get input
        Vector3 inputDir = new Vector3(ctx.Input.MoveX, 0, ctx.Input.MoveZ).normalized;
        
        // Determine speed based on sprint
        float targetSpeed = ctx.Input.SprintHeld ? ctx.RunSpeed : ctx.WalkSpeed;
        
        // Apply movement relative to camera (estilo Zelda BotW)
        ctx.MoveRelativeToCamera(inputDir, targetSpeed);
    }
    
    public override void Exit()
    {
        // Asegurar que Jump está desactivado al salir del estado
        ctx.Animator.SetBool("Jump", false);
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
