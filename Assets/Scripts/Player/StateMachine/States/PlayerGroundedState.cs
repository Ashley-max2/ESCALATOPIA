using UnityEngine;

/// <summary>
/// Estado cuando el jugador está en el suelo.
/// Gestiona movimiento, salto, y transiciones a escalada/gancho.
/// Movimiento estilo Zelda BotW con rotación suave hacia la dirección del movimiento.
/// </summary>
public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        ctx.IsClimbing = false;
        ctx.CurrentVelocity = new Vector3(ctx.Rb.velocity.x, 0, ctx.Rb.velocity.z);
        
        // Check for fall damage
        if (ctx.FallStartHeight > 0)
        {
            float fallDistance = ctx.FallStartHeight - ctx.transform.position.y;
            HandleLanding(fallDistance);
            ctx.FallStartHeight = 0;
        }
    }
    
    public override void Execute()
    {
        CheckTransitions();
    }
    
    public override void FixedExecute()
    {
        HandleMovement();
    }
    
    public override void Exit()
    {
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
    
    private void CheckTransitions()
    {
        // Jump
        if (ctx.Input.JumpPressed || IsJumpBuffered())
        {
            SwitchState(factory.Jump());
            return;
        }
        
        // Not grounded - fall
        if (!ctx.IsGrounded)
        {
            SwitchState(factory.Airborne());
            return;
        }
        
        // Climbing
        if (ctx.Input.ClimbHeld)
        {
            RaycastHit hit;
            if (ctx.CheckClimbableSurface(out hit))
            {
                ctx.WallNormal = hit.normal;
                SwitchState(factory.Climb());
                return;
            }
        }
        
        // Hook
        if (ctx.Input.HookPressed && ctx.GrapplingHook != null && ctx.GrapplingHook.CanFire())
        {
            SwitchState(factory.Hook());
            return;
        }
    }
    
    private void HandleLanding(float fallDistance)
    {
        GameEvents.PlayerLanded(fallDistance);
        
        // Check for lethal fall
        if (fallDistance >= ctx.LethalFallHeight)
        {
            ctx.Die();
        }
        else if (fallDistance >= ctx.SafeFallHeight)
        {
            // Could add stagger/damage effects here
            Debug.Log($"Hard landing from {fallDistance:F1}m");
        }
    }
}
