using UnityEngine;

/// <summary>
/// Estado de escalada renovado.
/// Se activa automaticamente al colisionar con Climbable mientras estas en el aire.
/// Movimiento absoluto en la pared: W=arriba, S=abajo, A=izquierda, D=derecha.
/// El player SIEMPRE mira hacia la pared, asi WASD siempre funciona igual.
/// </summary>
public class PlayerClimbState : PlayerBaseState
{
    // Ejes de la superficie (se recalculan al cambiar de pared)
    private Vector3 _wallNormal;
    private Vector3 _wallRight;  // derecha en la superficie (A/D)
    private Vector3 _wallUp;     // arriba en la superficie (W/S)
    
    private float _staminaConsumeTimer;
    
    // Mantle
    private const float MANTLE_FORWARD_DIST = 1.0f;
    private const float MANTLE_DOWN_DIST = 3f;
    private const float MANTLE_MOVE_SPEED = 8f;
    
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
        
        // Parar movimiento y quitar gravedad
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.useGravity = false;
        
        // Calcular ejes de la pared
        UpdateWallAxes(ctx.WallNormal);
        
        // Rotar el player para que MIRE HACIA la pared (forward = -wallNormal)
        SnapRotationToWall();
        
        // Reset caida
        ctx.FallStartHeight = 0;
        ctx.LastGroundedPosition = ctx.transform.position;
        
        _staminaConsumeTimer = 0;
        
        GameEvents.ClimbStart();
        Debug.Log("Started climbing");
    }
    
    public override void Execute()
    {
        // Mantle en progreso
        if (_isMantling)
        {
            UpdateMantle();
            return;
        }
        
        // Consumir stamina pasiva
        _staminaConsumeTimer += Time.deltaTime;
        if (_staminaConsumeTimer >= 0.5f)
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
    
    /// <summary>
    /// Calcula los ejes de movimiento en la pared
    /// </summary>
    private void UpdateWallAxes(Vector3 normal)
    {
        _wallNormal = normal.normalized;
        
        // Right desde la perspectiva del player mirando la pared
        // Cross(Normal, Up) da la derecha correcta del jugador
        _wallRight = Vector3.Cross(_wallNormal, Vector3.up).normalized;
        
        // Up real alineado a la pendiente (evita separarse al escalar hacia arriba)
        _wallUp = Vector3.Cross(_wallRight, _wallNormal).normalized;
    }
    
    /// <summary>
    /// Rota el player para que mire HACIA la pared (forward apunta a la pared)
    /// </summary>
    private void SnapRotationToWall()
    {
        // El player mira en la direccion opuesta a la normal (hacia la pared)
        Vector3 lookDir = -_wallNormal;
        lookDir.y = 0; // mantener rotacion horizontal
        if (lookDir.sqrMagnitude > 0.01f)
        {
            ctx.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
    
    /// <summary>
    /// Movimiento en la pared: WASD absolute
    /// W = arriba, S = abajo, A = izquierda, D = derecha
    /// No depende de la camara, siempre es relativo a la pared
    /// </summary>
    private void HandleClimbingMovement()
    {
        // Input crudo: MoveX = A(-1)/D(+1), MoveZ = S(-1)/W(+1)
        float horizontal = ctx.Input.MoveX; // A/D = izquierda/derecha en la pared
        float vertical = ctx.Input.MoveZ;   // S/W = abajo/arriba en la pared
        
        // Calcular direccion de movimiento en la pared
        Vector3 climbDir = (_wallRight * horizontal + _wallUp * vertical).normalized;
        
        // Aplicar velocidad de escalada
        Vector3 targetVel = climbDir * ctx.ClimbSpeed;
        ctx.Rb.velocity = Vector3.Lerp(ctx.Rb.velocity, targetVel, Time.fixedDeltaTime * 10f);
        
        // Consumir stamina extra al moverse
        if (climbDir.magnitude > 0.1f && ctx.Stamina != null)
        {
            ctx.Stamina.ConsumeStamina(ctx.ClimbStaminaCost * Time.fixedDeltaTime);
        }
    }
    
    /// <summary>
    /// Mantiene al player pegado a la pared con raycast continuo
    /// </summary>
    private void MaintainWallContact()
    {
        RaycastHit hit;
        if (ctx.CheckClimbableSurface(out hit))
        {
            // Actualizar ejes de la pared
            UpdateWallAxes(hit.normal);
            
            // Rotar suavemente hacia la pared
            Vector3 lookDir = -hit.normal;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRot, Time.fixedDeltaTime * 10f);
            }
            
            // Empujar levemente hacia la pared para mantener contacto usando la normal real
            ctx.Rb.AddForce(-hit.normal * 2f, ForceMode.Acceleration);
            
            // Actualizar normal en el contexto
            ctx.WallNormal = _wallNormal;
            
            // Posicion segura
            ctx.LastGroundedPosition = ctx.transform.position;
        }
    }
    
    #region Mantle
    
    /// <summary>
    /// Detecta si el player esta cerca del borde superior de la pared
    /// </summary>
    private bool TryDetectLedge(out Vector3 ledgePoint)
    {
        ledgePoint = Vector3.zero;
        
        // Altura real del collider del player
        float playerHeight = ctx.Collider != null ? ctx.Collider.height : 2f;
        
        // 1. Comprobar a varias alturas si aun hay pared
        // Si no hay pared a la altura del pecho o cabeza = estamos en el borde
        float checkHeight = playerHeight * 0.85f; // cerca de la cabeza
        Vector3 headOrigin = ctx.transform.position + Vector3.up * checkHeight;
        float checkDist = ctx.ClimbCheckDistance * 2f;
        
        bool wallAtHead = Physics.Raycast(headOrigin, ctx.transform.forward,
            checkDist, ctx.ClimbableMask);
        
        if (wallAtHead) return false; // aun hay pared arriba, seguir escalando
        
        // 2. Buscar suelo encima para poner al player ahi
        // Usar tanto GroundMask como ClimbableMask para detectar la superficie
        LayerMask combinedMask = ctx.GroundMask | ctx.ClimbableMask;
        
        Vector3 forwardDir = -_wallNormal;
        Vector3 overLedge = headOrigin + forwardDir * MANTLE_FORWARD_DIST;
        
        RaycastHit groundHit;
        if (Physics.Raycast(overLedge, Vector3.down, out groundHit, MANTLE_DOWN_DIST, combinedMask))
        {
            if (groundHit.normal.y > 0.5f) // mas permisivo con la inclinacion
            {
                ledgePoint = groundHit.point + Vector3.up * ctx.MantleExtraHeight;
                return true;
            }
        }
        
        return false;
    }
    
    private void StartMantle(Vector3 target)
    {
        _isMantling = true;
        _mantleTarget = target;
        _mantleTimer = 0;
        
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.useGravity = false;
        
        Debug.Log("Mantling up!");
    }
    
    private void UpdateMantle()
    {
        _mantleTimer += Time.deltaTime;
        
        Vector3 currentPos = ctx.transform.position;
        float step = MANTLE_MOVE_SPEED * Time.deltaTime;
        
        // Fase 1: subir, Fase 2: avanzar
        if (currentPos.y < _mantleTarget.y - 0.1f)
        {
            Vector3 upPos = new Vector3(currentPos.x, _mantleTarget.y, currentPos.z);
            ctx.transform.position = Vector3.MoveTowards(currentPos, upPos, step);
        }
        else
        {
            ctx.transform.position = Vector3.MoveTowards(ctx.transform.position, _mantleTarget, step);
        }
        
        // Completar
        float dist = Vector3.Distance(ctx.transform.position, _mantleTarget);
        if (dist < 0.1f || _mantleTimer > 1.5f)
        {
            ctx.transform.position = _mantleTarget;
            ctx.Rb.velocity = Vector3.zero;
            SwitchState(factory.Grounded());
            Debug.Log("Mantle complete!");
        }
    }
    
    #endregion
    
    private void CheckTransitions()
    {
        // Saltar desde la pared
        if (ctx.Input.JumpPressed)
        {
            SwitchState(factory.WallJump());
            return;
        }
        
        // Sin stamina -> caer
        if (ctx.Stamina != null && !ctx.Stamina.HasStamina())
        {
            Debug.Log("Out of stamina! Falling...");
            SwitchState(factory.Airborne());
            return;
        }
        
        // Check mantle siempre (no solo al subir, tambien si ya estamos arriba)
        {
            Vector3 ledgePoint;
            if (TryDetectLedge(out ledgePoint))
            {
                StartMantle(ledgePoint);
                return;
            }
        }
        
        // Perder contacto con la pared
        if (!ctx.CheckClimbableSurface(out _))
        {
            // Intentar mantle antes de caer
            Vector3 fallbackLedge;
            if (TryDetectLedge(out fallbackLedge))
            {
                StartMantle(fallbackLedge);
                return;
            }
            
            // Caer
            SwitchState(factory.Airborne());
            return;
        }
        
        // Si toca el suelo escalando -> grounded
        if (ctx.IsGrounded)
        {
            SwitchState(factory.Grounded());
            return;
        }
        
        // Hook mientras escala
        if (ctx.Input.HookPressed && ctx.GrapplingHook != null && ctx.GrapplingHook.CanFire())
        {
            SwitchState(factory.Hook());
            return;
        }
    }
}
