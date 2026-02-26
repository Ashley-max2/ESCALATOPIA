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
    
    // Mantle settings
    private const float MANTLE_CHECK_HEIGHT = 1.2f;  // altura desde el player para buscar el borde
    private const float MANTLE_FORWARD_DIST = 0.8f;  // distancia forward para buscar suelo encima
    private const float MANTLE_DOWN_DIST = 2.5f;     // raycast hacia abajo para encontrar suelo
    private const float MANTLE_MOVE_SPEED = 8f;      // velocidad del mantle
    
    private bool _isMantling;
    private Vector3 _mantleTarget;
    private float _mantleTimer;
    
    public PlayerClimbState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        ctx.IsClimbing = true;
        _isMantling = false;
        _mantleTimer = 0;
        
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
        // Si esta haciendo mantle, solo actualizar eso
        if (_isMantling)
        {
            UpdateMantle();
            return;
        }
        
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
        if (_isMantling) return;
        
        HandleClimbingMovement();
        MaintainWallContact();
    }
    
    public override void Exit()
    {
        ctx.IsClimbing = false;
        ctx.Rb.useGravity = true;
        _isMantling = false;
        
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
    
    /// <summary>
    /// Check proactivo de borde: detecta si el player esta cerca del borde superior
    /// mientras aun tiene contacto con la pared
    /// </summary>
    private bool TryDetectLedge(out Vector3 ledgePoint)
    {
        ledgePoint = Vector3.zero;
        
        // 1. Comprobar si NO hay pared a la altura de la cabeza (borde detectado)
        Vector3 headCheckOrigin = ctx.transform.position + Vector3.up * MANTLE_CHECK_HEIGHT;
        bool wallAtHead = Physics.Raycast(headCheckOrigin, ctx.transform.forward, 
            ctx.ClimbCheckDistance * 1.5f, ctx.ClimbableMask);
        
        if (wallAtHead) return false; // aun hay pared arriba, no estamos en el borde
        
        // 2. Si no hay pared arriba, buscar suelo encima con raycast adelante+arriba
        Vector3 forwardDir = -_climbSurfaceNormal; // hacia la pared
        Vector3 overLedgeOrigin = headCheckOrigin + forwardDir * MANTLE_FORWARD_DIST;
        
        RaycastHit groundHit;
        if (Physics.Raycast(overLedgeOrigin, Vector3.down, out groundHit, MANTLE_DOWN_DIST, ctx.GroundMask))
        {
            // Encontramos suelo encima, verificar que sea pisable (no muy inclinado)
            if (groundHit.normal.y > 0.7f)
            {
                ledgePoint = groundHit.point + Vector3.up * 0.05f;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Inicia el mantle: mueve al player suavemente encima del borde
    /// </summary>
    private void StartMantle(Vector3 target)
    {
        _isMantling = true;
        _mantleTarget = target;
        _mantleTimer = 0;
        
        // Parar velocidad y desactivar gravedad durante mantle
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.useGravity = false;
        
        Debug.Log("Mantling up!");
    }
    
    /// <summary>
    /// Actualiza el movimiento de mantle frame a frame
    /// </summary>
    private void UpdateMantle()
    {
        _mantleTimer += Time.deltaTime;
        
        // Mover suave hacia el punto de mantle
        Vector3 currentPos = ctx.transform.position;
        float step = MANTLE_MOVE_SPEED * Time.deltaTime;
        
        // Primero subir, luego avanzar
        Vector3 intermediatePos = new Vector3(currentPos.x, _mantleTarget.y, currentPos.z);
        
        if (currentPos.y < _mantleTarget.y - 0.1f)
        {
            // Fase 1: subir
            ctx.transform.position = Vector3.MoveTowards(currentPos, intermediatePos, step);
        }
        else
        {
            // Fase 2: avanzar al punto final
            ctx.transform.position = Vector3.MoveTowards(ctx.transform.position, _mantleTarget, step);
        }
        
        // Completar mantle cuando lleguemos o si tarda demasiado
        float dist = Vector3.Distance(ctx.transform.position, _mantleTarget);
        if (dist < 0.1f || _mantleTimer > 1.5f)
        {
            ctx.transform.position = _mantleTarget;
            ctx.Rb.velocity = Vector3.zero;
            SwitchState(factory.Grounded());
            Debug.Log("Mantle complete!");
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
        
        // Check proactivo de borde mientras escalamos hacia arriba
        if (ctx.Input.MoveZ > 0.1f) // escalando hacia arriba
        {
            Vector3 ledgePoint;
            if (TryDetectLedge(out ledgePoint))
            {
                StartMantle(ledgePoint);
                return;
            }
        }
        
        // Lost wall contact
        if (!ctx.CheckClimbableSurface(out _))
        {
            // Check fallback: buscar suelo encima aunque no estemos subiendo activamente
            Vector3 fallbackLedge;
            if (TryDetectLedge(out fallbackLedge))
            {
                StartMantle(fallbackLedge);
                return;
            }
            
            // No hay borde, caer
            SwitchState(factory.Airborne());
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
