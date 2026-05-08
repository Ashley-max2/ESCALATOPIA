using UnityEngine;

/// <summary>
/// Estado cuando el jugador está en el aire (cayendo o saltando).
/// Gestiona control aéreo limitado, física de caída mejorada, y transiciones.
/// Si colisiona con una superficie Climbable, transiciona automaticamente a escalada.
/// </summary>
public class PlayerAirborneState : PlayerBaseState
{
    public PlayerAirborneState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        // Record fall start if not already set
        if (ctx.FallStartHeight <= 0)
        {
            ctx.FallStartHeight = ctx.transform.position.y;
        }
    }
    
    public override void Execute()
    {
        // Update fall height tracking if falling
        if (ctx.Rb.velocity.y < 0 && ctx.transform.position.y > ctx.FallStartHeight)
        {
            ctx.FallStartHeight = ctx.transform.position.y;
        }
        
        CheckTransitions();
        
        // Track fall for events
        if (ctx.Rb.velocity.y < -1f)
        {
            float currentFallDistance = ctx.FallStartHeight - ctx.transform.position.y;
            GameEvents.PlayerFalling(currentFallDistance);
        }
    }
    
    public override void FixedExecute()
    {
        HandleAirControl();
        ApplyFallGravity();
    }
    
    public override void Exit()
    {
    }
    
    private void HandleAirControl()
    {
        Vector3 inputDir = new Vector3(ctx.Input.MoveX, 0, ctx.Input.MoveZ).normalized;
        
        if (inputDir.magnitude > 0.1f)
        {
            // Apply limited air control (estilo Zelda BotW)
            float speed = ctx.Input.SprintHeld ? ctx.RunSpeed : ctx.WalkSpeed;
            ctx.MoveRelativeToCamera(inputDir, speed, ctx.AirControl);
        }
    }
    
    /// <summary>
    /// Solo aplica gravedad extra al caer. El salto siempre sube lo mismo.
    /// </summary>
    private void ApplyFallGravity()
    {
        if (ctx.Rb.velocity.y < 0)
        {
            // Caída - aplica gravedad extra para que caiga más rápido
            ctx.Rb.velocity += Vector3.up * Physics.gravity.y * (ctx.FallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    private void CheckTransitions()
    {
        // Landed - Verificar específicamente con raycast a layer Ground
        if (ctx.IsGrounded && ctx.Rb.velocity.y <= 0.1f)
        {
            // Verificar que realmente está tocando Ground layer
            Vector3 origin = ctx.GroundCheck.position + Vector3.up * ctx.GroundCheckRadius;
            if (Physics.Raycast(origin, Vector3.down, ctx.GroundCheckRadius * 2.5f, ctx.GroundMask))
            {
                // Activar parámetro Landing para reproducir EndJump inmediatamente
                ctx.Animator.SetBool("Landing", true);
                Debug.Log("Landing activado - tocando Ground layer");
                SwitchState(factory.Grounded());
                return;
            }
        }
        
        // Coyote time jump
        if (ctx.Input.JumpPressed && IsCoyoteTimeActive())
        {
            SwitchState(factory.Jump());
            return;
        }
        
        // Auto-climb: si chocamos con una superficie escalable y no estamos exhaustos
        RaycastHit hit;
        if (ctx.CheckClimbableSurface(out hit))
        {
            if (ctx.Stamina == null || !ctx.Stamina.IsExhausted)
            {
                ctx.WallNormal = hit.normal;
                SwitchState(factory.Climb());
                return;
            }
        }
        
        // Hook while in air
        if (ctx.Input.HookPressed && ctx.GrapplingHook != null && ctx.GrapplingHook.CanFire())
        {
            SwitchState(factory.Hook());
            return;
        }
    }
}
