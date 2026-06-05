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
    private float _offWallTimer = 0f;

    // Collider adjustment para escalada (evita que el mesh atraviese la pared)
    private Vector3 _originalColliderCenter;
    private float _originalColliderHeight;
    [UnityEngine.Tooltip("Desplazamiento del collider hacia la pared durante la escalada")]
    private const float CLIMB_COLLIDER_FORWARD_OFFSET = 0.2f;
    private const float CLIMB_COLLIDER_HEIGHT_REDUCTION = 0.3f;

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
        // Animator: activar capa Climb (Layer 1) y parámetro Climb
        ctx.Animator.SetLayerWeight(1, 1f);
        ctx.Animator.SetBool("Climb", true);

        // Ajustar collider para la postura de escalada:
        // Lo desplazamos hacia adelante (hacia la pared) y lo hacemos un poco más bajo
        // para que el mesh no atraviese la roca.
        if (ctx.Collider != null)
        {
            _originalColliderCenter = ctx.Collider.center;
            _originalColliderHeight = ctx.Collider.height;

            ctx.Collider.center = _originalColliderCenter + new Vector3(0f, 0f, CLIMB_COLLIDER_FORWARD_OFFSET);
            ctx.Collider.height = _originalColliderHeight - CLIMB_COLLIDER_HEIGHT_REDUCTION;
        }

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
        _offWallTimer = 0f;

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
        // Animator: desactivar parámetro Climb y capa Climb (Layer 1)
        ctx.Animator.SetBool("Climb", false);
        ctx.Animator.SetLayerWeight(1, 0f);

        // Restaurar collider original
        if (ctx.Collider != null)
        {
            ctx.Collider.center = _originalColliderCenter;
            ctx.Collider.height = _originalColliderHeight;
        }

        ctx.IsClimbing = false;
        ctx.Rb.useGravity = true;
        ctx.Rb.isKinematic = false; // Asegurar restaurar físicas
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
        // Input CRÚDO (WASD real del teclado), NUNCA de la cámara
        float horizontal = ctx.Input.MoveX; // A (-1) / D (+1)
        float vertical = ctx.Input.MoveZ;   // S (-1) / W (+1)

        if (AnalyticsManager.Instance != null && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
        {
            AnalyticsManager.Instance.RecordMovement(new Vector2(horizontal, vertical));
        }

        // Movimiento ABSOLUTO referenciado 100% a la pared actual. ¡Nunca a la cámara!
        // W siempre aplica fuerza en `_wallUp`, D siempre en `_wallRight`.
        Vector3 climbDir = (_wallRight * horizontal + _wallUp * vertical).normalized;

        // Aplicar velocidad de escalada
        Vector3 targetVel = climbDir * ctx.ClimbSpeed;

        // Un pequeño empuje hacia la pared para mantener el contacto sin rebotar
        targetVel += -_wallNormal * 2f;

        ctx.Rb.velocity = Vector3.Lerp(ctx.Rb.velocity, targetVel, Time.fixedDeltaTime * 10f);

        // Consumir stamina extra al moverse
        if (climbDir.sqrMagnitude > 0.01f && ctx.Stamina != null)
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

            // Rotar SUAVEMENTE pero más rápido hacia la pared
            Vector3 lookDir = -hit.normal;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
                ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRot, Time.fixedDeltaTime * 15f);
            }

            // Actualizar normal en el contexto
            ctx.WallNormal = _wallNormal;

            // Posicion segura
            ctx.LastGroundedPosition = ctx.transform.position;
        }
    }

    #region Mantle

    /// <summary>
    /// Detecta si el player esta cerca del borde superior de la pared.
    /// Prueba múltiples alturas y distancias hacia adelante para mayor robustez.
    /// </summary>
    private bool TryDetectLedge(out Vector3 ledgePoint)
    {
        ledgePoint = Vector3.zero;
        float playerHeight = ctx.Collider != null ? ctx.Collider.height : 2f;
        LayerMask combinedMask = ctx.GroundMask | ctx.ClimbableMask;

        // Probar varias alturas y varios offsets hacia adelante para cubrir
        // distintas configuraciones de geometría (paredes finas, bordes irregulares…)
        float[] heightFactors  = { 0.85f, 0.75f, 0.95f };
        float[] forwardOffsets = { 0.8f,  0.5f,  1.2f  };

        foreach (float heightFactor in heightFactors)
        {
            Vector3 headOrigin = ctx.transform.position + Vector3.up * (playerHeight * heightFactor);

            // Si aún hay pared a esta altura, no estamos en el borde todavía
            if (Physics.Raycast(headOrigin, -_wallNormal, ctx.ClimbCheckDistance * 2f, ctx.ClimbableMask))
                continue;

            foreach (float fwdOffset in forwardOffsets)
            {
                Vector3 overLedge = headOrigin + (-_wallNormal * fwdOffset);
                RaycastHit groundHit;
                if (Physics.Raycast(overLedge, Vector3.down, out groundHit, MANTLE_DOWN_DIST, combinedMask))
                {
                    float groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
                    if (groundAngle <= ctx.MaxWalkableSlope + 5f)
                    {
                        ledgePoint = groundHit.point + Vector3.up * ctx.MantleExtraHeight;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private Vector3 _mantleStartPos;

    private void StartMantle(Vector3 target)
    {
        _isMantling = true;
        _mantleTarget = target;
        _mantleStartPos = ctx.transform.position;
        _mantleTimer = 0;

        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.useGravity = false;

        // HACER KINEMATIC durante la animación de subir para que no haya rebotes con colliders
        ctx.Rb.isKinematic = true;

        Debug.Log("Mantling up smoothly!");
    }

    private void UpdateMantle()
    {
        // Velocidad de la transición del mantle
        _mantleTimer += Time.deltaTime * 3.5f;

        float t = Mathf.Clamp01(_mantleTimer);

        // CURVAS DE ANIMACIÓN MANUAL:
        // tY sube muy rápido al principio (Seno), para que el personaje suba su cuerpo
        float tY = Mathf.Sin(t * Mathf.PI * 0.5f);
        // tXZ avanza hacia adelante linealmente para asegurar escalar e ir forzado hacia delante
        float tXZ = t;

        Vector3 newPos = new Vector3(
            Mathf.Lerp(_mantleStartPos.x, _mantleTarget.x, tXZ),
            Mathf.Lerp(_mantleStartPos.y, _mantleTarget.y, tY),
            Mathf.Lerp(_mantleStartPos.z, _mantleTarget.z, tXZ)
        );

        ctx.Rb.MovePosition(newPos);

        // Completar
        if (t >= 1f)
        {
            ctx.transform.position = _mantleTarget;
            ctx.Rb.isKinematic = false; // Devolver a físicas reales
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

        bool hasWallContact = ctx.CheckClimbableSurface(out _);

        // Intentar hacer cima:
        // - Al subir activamente (umbral bajo para mayor responsividad)
        // - O automáticamente al perder contacto en el borde (el jugador llegó arriba)
        if (ctx.Input.MoveZ > 0.05f || !hasWallContact)
        {
            Vector3 ledgePoint;
            if (TryDetectLedge(out ledgePoint))
            {
                StartMantle(ledgePoint);
                return;
            }
        }

        // Perder contacto con la pared -> caer tras gracia de 0.5s
        if (!hasWallContact)
        {
            _offWallTimer += Time.deltaTime;
            if (_offWallTimer >= 0.5f)
            {
                SwitchState(factory.Airborne());
                return;
            }
        }
        else
        {
            _offWallTimer = 0f;
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
