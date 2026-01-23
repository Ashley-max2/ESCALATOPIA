using UnityEngine;

/// <summary>
/// Estado de escalada inspirado en Jusant.
/// El jugador se adhiere a superficies escalables y se mueve en cualquier dirección.
/// Consume estamina mientras escala y puede saltar de la pared.
/// </summary>
public class PlayerClimbState : PlayerBaseState
{
    private Vector3 _climbSurfaceNormal;
    private Vector3 _climbSurfaceRight;
    private Vector3 _climbSurfaceUp;
    private float _staminaConsumeTimer;
    
    public PlayerClimbState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        ctx.IsClimbing = true;
        
        // Stop all movement and disable gravity
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.useGravity = false;
        
        // Calculate climbing axes based on wall normal
        _climbSurfaceNormal = ctx.WallNormal;
        _climbSurfaceRight = Vector3.Cross(Vector3.up, _climbSurfaceNormal).normalized;
        _climbSurfaceUp = Vector3.up;
        
        // Rotate to face away from wall
        Quaternion targetRotation = Quaternion.LookRotation(-_climbSurfaceNormal, Vector3.up);
        ctx.transform.rotation = targetRotation;
        
        // Reset fall height (climbing is safe position)
        ctx.FallStartHeight = 0;
        ctx.LastGroundedPosition = ctx.transform.position;
        
        _staminaConsumeTimer = 0;
        
        GameEvents.ClimbStart();
        Debug.Log("Started climbing");
    }
    
    public override void Execute()
    {
        // Consume stamina over time
        _staminaConsumeTimer += Time.deltaTime;
        if (_staminaConsumeTimer >= 0.5f) // Every 0.5 seconds
        {
            if (ctx.Stamina != null)
            {
                ctx.Stamina.ConsumeStamina(ctx.ClimbStaminaCost * 0.5f);
            }
            _staminaConsumeTimer = 0;
        }
        
        CheckTransitions();
    }
    
    public override void FixedExecute()
    {
        HandleClimbingMovement();
        MaintainWallContact();
    }
    
    public override void Exit()
    {
        ctx.IsClimbing = false;
        ctx.Rb.useGravity = true;
        
        GameEvents.ClimbEnd();
    }
    
    private void HandleClimbingMovement()
    {
        // Get input
        float horizontal = ctx.Input.MoveX;
        float vertical = ctx.Input.MoveZ;
        
        // Calculate movement along the wall surface
        Vector3 climbMovement = (_climbSurfaceRight * horizontal + _climbSurfaceUp * vertical).normalized;
        
        // Apply climbing speed
        Vector3 targetVelocity = climbMovement * ctx.ClimbSpeed;
        ctx.Rb.velocity = Vector3.Lerp(ctx.Rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);
        
        // Consume extra stamina while moving
        if (climbMovement.magnitude > 0.1f && ctx.Stamina != null)
        {
            ctx.Stamina.ConsumeStamina(ctx.ClimbStaminaCost * Time.fixedDeltaTime);
        }
    }
    
    private void MaintainWallContact()
    {
        // Raycast to keep attached to wall
        RaycastHit hit;
        Vector3 rayOrigin = ctx.transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayOrigin, ctx.transform.forward, out hit, ctx.ClimbCheckDistance * 1.5f, ctx.ClimbableMask))
        {
            // Update surface normal
            _climbSurfaceNormal = hit.normal;
            _climbSurfaceRight = Vector3.Cross(Vector3.up, _climbSurfaceNormal).normalized;
            
            // Rotate to face away from wall
            Quaternion targetRotation = Quaternion.LookRotation(-_climbSurfaceNormal, Vector3.up);
            ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            
            // Move slightly towards wall to maintain contact
            ctx.Rb.AddForce(ctx.transform.forward * 2f, ForceMode.Acceleration);
            
            // Update wall normal in context
            ctx.WallNormal = _climbSurfaceNormal;
            
            // Update safe position
            ctx.LastGroundedPosition = ctx.transform.position;
        }
    }
    
    private void CheckTransitions()
    {
        // Jump off wall (estilo Jusant)
        if (ctx.Input.JumpPressed)
        {
            SwitchState(factory.WallJump());
            return;
        }
        
        // Release climb key
        if (!ctx.Input.ClimbHeld)
        {
            SwitchState(factory.Airborne());
            return;
        }
        
        // Out of stamina
        if (ctx.Stamina != null && !ctx.Stamina.HasStamina())
        {
            Debug.Log("Out of stamina! Falling...");
            SwitchState(factory.Airborne());
            return;
        }
        
        // Lost wall contact
        if (!ctx.CheckClimbableSurface(out _))
        {
            // Check if we reached the top (can mantle)
            RaycastHit groundHit;
            Vector3 mantleCheckPos = ctx.transform.position + Vector3.up * 1.5f - ctx.transform.forward * 0.5f;
            if (Physics.Raycast(mantleCheckPos, Vector3.down, out groundHit, 2f, ctx.GroundMask))
            {
                // Mantle up
                ctx.transform.position = groundHit.point + Vector3.up * 0.1f;
                ctx.Rb.velocity = Vector3.zero;
                SwitchState(factory.Grounded());
                Debug.Log("Mantled up!");
            }
            else
            {
                // Lost wall contact, fall
                SwitchState(factory.Airborne());
            }
            return;
        }
        
        // Landed on ground while climbing
        if (ctx.IsGrounded)
        {
            SwitchState(factory.Grounded());
            return;
        }
        
        // Use hook while climbing
        if (ctx.Input.HookPressed && ctx.GrapplingHook != null && ctx.GrapplingHook.CanFire())
        {
            SwitchState(factory.Hook());
            return;
        }
    }
}
