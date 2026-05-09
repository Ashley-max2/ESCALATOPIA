using UnityEngine.SceneManagement;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using FMOD;
using Debug = UnityEngine.Debug;

/// <summary>
/// Máquina de estados principal del jugador.
/// Gestiona todas las transiciones entre estados y mantiene el contexto compartido.
/// Inspirado en Zelda BotW para movimiento y Jusant para escalada.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerStateMachine : MonoBehaviour
{
    #region Components
    public Rigidbody Rb { get; private set; }
    public PlayerInputHandler Input { get; private set; }
    public CapsuleCollider Collider { get; private set; }
    public Animator Animator { get; private set; }
    public GrapplingHook GrapplingHook { get; private set; }
    public StaminaSystem Stamina { get; private set; }
    public Transform CameraTarget { get; private set; }
    #endregion

    public event System.Action OnDie;

    #region State Management
    public IState CurrentState { get; private set; }
    private PlayerStateFactory _stateFactory;
    public PlayerStateFactory States {
        get {
            if (_stateFactory == null) _stateFactory = new PlayerStateFactory(this);
            return _stateFactory;
        }
    }
    #endregion

    #region Movement Settings
    [Header("=== MOVEMENT ===")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float RotationSpeed => rotationSpeed;
    public float Acceleration => acceleration;

    public void SetWalkSpeed(float val) => walkSpeed = val;
    public void SetRunSpeed(float val) => runSpeed = val;
    public void SetJumpForce(float val) => jumpForce = val;
    public void SetClimbSpeed(float val) => climbSpeed = val;
    public void SetClimbStaminaCost(float val) => climbStaminaCost = val;
    public void SetFallMultiplier(float val) => fallMultiplier = val;
    public void SetHookAccelerationTime(float val) => hookAccelerationTime = val;

    [SerializeField] private float runStaminaCost = 5f;
    public float RunStaminaCost => runStaminaCost;
public float Deceleration => deceleration;
    #endregion

    #region Jump Settings
    [Header("=== JUMP ===")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float airControl = 0.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    public float JumpForce => jumpForce;
    public float AirControl => airControl;
    public float FallMultiplier => fallMultiplier;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    #endregion

    #region Climbing Settings
    [Header("=== CLIMBING ===")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float climbStaminaCost = 3f;
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] private float wallJumpUpwardForce = 6f;
    [SerializeField] private float climbCheckDistance = 0.6f;
    [SerializeField] private LayerMask climbableMask;
    [Tooltip("Altura extra al hacer mantle para que el player suba bien encima")]
    [SerializeField] private float mantleExtraHeight = 0.5f;
    [SerializeField] private float minClimbAngle = 45f;
    [SerializeField] private float maxClimbAngle = 135f;

    public float ClimbSpeed => climbSpeed;
    public float ClimbStaminaCost => climbStaminaCost;
    public float WallJumpForce => wallJumpForce;
    public float WallJumpUpwardForce => wallJumpUpwardForce;
    public float ClimbCheckDistance => climbCheckDistance;
    public LayerMask ClimbableMask => climbableMask;
    public float MantleExtraHeight => mantleExtraHeight;
    public float MinClimbAngle => minClimbAngle;
    public float MaxClimbAngle => maxClimbAngle;
    #endregion

    #region Hook Settings
    [Header("=== HOOK ===")]
    [Tooltip("Tiempo en segundos de aceleración inicial del gancho")]
    public float hookAccelerationTime = 0.3f;

    public float HookAccelerationTime => hookAccelerationTime;
    #endregion

    #region Ground Check
    [Header("=== GROUND CHECK ===")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxWalkableSlope = 45f;

    public Transform GroundCheck => groundCheck;
    public float GroundCheckRadius => groundCheckRadius;
    public LayerMask GroundMask => groundMask;
    public float MaxWalkableSlope => maxWalkableSlope;
    #endregion

    #region Fall Death Settings
    [Header("=== FALL DEATH ===")]
    [SerializeField] private float lethalFallHeight = 15f;
    [SerializeField] private float safeFallHeight = 5f;

    public float LethalFallHeight => lethalFallHeight;
    public float SafeFallHeight => safeFallHeight;
    #endregion

    #region Runtime Variables
    [HideInInspector] public bool IsGrounded;
    [HideInInspector] public bool IsClimbing;
    [HideInInspector] public bool IsHooking;
    [HideInInspector] public Vector3 WallNormal;
    [HideInInspector] public Vector3 LastGroundedPosition;
    [HideInInspector] public float FallStartHeight;
    private Vector3 _lastAnalyticsPosition;
    [HideInInspector] public float LastGroundedTime;
    [HideInInspector] public float LastJumpPressTime = -999f;

    // Velocity tracking for smooth movement
    [HideInInspector] public Vector3 CurrentVelocity;
    private float _currentRotationVelocity;
    #endregion

    #region Debug
    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebugInfo = true;
    #endregion

    #region FMOD Audio
    [Header("=== FMOD AUDIO ===")]
    [EventRef] [SerializeField] private string walkEventPath = "event:/SFX/Player/Walking";
    [EventRef] [SerializeField] private string runEventPath = "event:/SFX/Player/Running";

    private EventInstance _movementEventInstance;
    private string _activeMovementEventPath;
    #endregion

    private void Awake()
    {
        // Get required components
        Rb = GetComponent<Rigidbody>();
        Input = GetComponent<PlayerInputHandler>();
        Collider = GetComponent<CapsuleCollider>();
        Animator = GetComponentInChildren<Animator>();
        GrapplingHook = GetComponentInChildren<GrapplingHook>();
        Stamina = GetComponent<StaminaSystem>();

        // Setup camera target
        var cameraTargetObj = transform.Find("CameraTarget");
        if (cameraTargetObj == null)
        {
            cameraTargetObj = new GameObject("CameraTarget").transform;
            cameraTargetObj.SetParent(transform);
            cameraTargetObj.localPosition = new Vector3(0, 1.5f, 0);
        }
        CameraTarget = cameraTargetObj;

        // Configure rigidbody
        Rb.freezeRotation = true;
        Rb.interpolation = RigidbodyInterpolation.Interpolate;
        Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Create state factory
        _stateFactory = new PlayerStateFactory(this);
    }

    private void Start()
    {
        // Iniciar en grounded
        LastJumpPressTime = -100f;
        TransitionToState(States.Grounded());
        LastGroundedPosition = transform.position;
        _lastAnalyticsPosition = transform.position;

        // Bloquear cursor al empezar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        StopMovementEvent();
    }

    /// <summary>
    /// Comprueba si el juego esta pausado (via GameManager)
    /// </summary>
    private bool IsGamePaused()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPaused;
    }

    private void Update()
    {
        // No procesar nada si estamos en pausa
        if (IsGamePaused()) return;

        // Track jump buffer ANTES de ejecutar el estado
        if (Cursor.lockState == CursorLockMode.Locked && Input.JumpPressed)
            LastJumpPressTime = Time.time;

        // Update ground check
        UpdateGroundCheck();

        // Update state (aqui se lee JumpPressed y IsJumpBuffered)
        CurrentState?.Execute();

        AnalyticsManager.Instance?.RecordMovementDelta(transform.position - _lastAnalyticsPosition, transform);
        _lastAnalyticsPosition = transform.position;
        AnalyticsManager.Instance?.UpdateMaxHeight(transform.position.y);

        // Update footstep/locomotion audio
        UpdateMovementAudio();

        // Player animator controller
        if (Animator == null) return;
        switch (CurrentState)
        {
            case PlayerGroundedState:
                float horizontalSpeed = new Vector2(Rb.velocity.x, Rb.velocity.z).magnitude;

                Animator.SetFloat("ClimbSpeed", 0f);

                // Set Speed parameter based on horizontal velocity
                // Normalized by walkSpeed for smooth transitions between Idle and Walk
                float speedParameter = horizontalSpeed / walkSpeed;
                Animator.SetFloat("Speed", speedParameter);
                break;

            case PlayerJumpState:
                // Animation is played in Enter(), not here
                break;

            case PlayerAirborneState:
                Animator.Play("JumpLoop");
                break;

            case PlayerHookState:
                Animator.Play("Hook");
                break;

            case PlayerClimbState:
                float verticalSpeed = Mathf.Abs(Rb.velocity.y);

                if (verticalSpeed > 0.1f)
                    Animator.SetFloat("ClimbSpeed", 1f);
                else
                    Animator.SetFloat("ClimbSpeed", 0f);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (IsGamePaused()) return;
        CurrentState?.FixedExecute();
    }

    private void UpdateMovementAudio()
    {
        if (CurrentState is PlayerGroundedState)
        {
            float horizontalSpeed = new Vector2(Rb.velocity.x, Rb.velocity.z).magnitude;

            if (horizontalSpeed > 0.1f)
            {
                string eventPath = Input.SprintHeld ? runEventPath : walkEventPath;

                bool sameEvent = eventPath == _activeMovementEventPath;
                bool isPlaying = IsMovementEventPlaying();

                if (!sameEvent)
                {
                    StopMovementEvent();
                    PlayMovementEvent(eventPath);
                    return;
                }

                if (!isPlaying)
                {
                    // El evento terminó y todavía se está moviendo: reiniciar.
                    StopMovementEvent();
                    PlayMovementEvent(eventPath);
                    return;
                }

                return;
            }
        }

        StopMovementEvent();
    }

    private bool IsMovementEventPlaying()
    {
        if (!_movementEventInstance.isValid())
            return false;

        PLAYBACK_STATE state;
        if (_movementEventInstance.getPlaybackState(out state) != RESULT.OK)
            return false;

        return state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.PLAYING;
    }

    private void PlayMovementEvent(string eventPath)
    {
        if (string.IsNullOrEmpty(eventPath))
            return;

        _movementEventInstance = RuntimeManager.CreateInstance(eventPath);
        RuntimeManager.AttachInstanceToGameObject(_movementEventInstance, transform, Rb);
        _movementEventInstance.start();
        _activeMovementEventPath = eventPath;
    }

    private void StopMovementEvent()
    {
        if (_movementEventInstance.isValid())
        {
            _movementEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _movementEventInstance.release();
        }

        _activeMovementEventPath = null;
    }

    private void UpdateGroundCheck()
    {
        bool wasGrounded = IsGrounded;

        // Initial broad check
        bool physicsHit = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        IsGrounded = false;

        if (physicsHit)
        {
            // Verify slope angle using raycast
            Vector3 origin = groundCheck.position + Vector3.up * groundCheckRadius;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckRadius * 2.5f, groundMask))
            {
                float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (slopeAngle <= maxWalkableSlope)
                {
                    IsGrounded = true;
                }
            }
            else
            {
                // Fallback if raycast misses but sphere hits (e.g. edge of platform)
                IsGrounded = true;
            }
        }

        if (IsGrounded)
        {
            LastGroundedTime = Time.time;

            // Update safe position only when stable on ground
            if (wasGrounded)
                LastGroundedPosition = transform.position;
        }
    }

    #region State Management
    public void TransitionToState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();

        GameEvents.PlayerStateChanged(CurrentState?.GetType().Name ?? "None");
    }
    #endregion

    #region Movement Helpers
    /// <summary>
    /// Mueve al jugador en una dirección relativa a la cámara (estilo Zelda BotW)
    /// </summary>
    public void MoveRelativeToCamera(Vector3 inputDirection, float speed, float controlMultiplier = 1f)
    {
        if (Camera.main == null) return;

        Transform cameraTransform = Camera.main.transform;

        // Get camera-relative directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate move direction
        Vector3 moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;

        // Apply movement with acceleration
        Vector3 targetVelocity = moveDirection * speed;
        float accel = moveDirection.magnitude > 0.1f ? acceleration : deceleration;

        CurrentVelocity = Vector3.Lerp(CurrentVelocity, targetVelocity, accel * controlMultiplier * Time.fixedDeltaTime);

        // Apply horizontal velocity, preserve vertical
        Rb.velocity = new Vector3(CurrentVelocity.x, Rb.velocity.y, CurrentVelocity.z);
        
        // Rotar siempre hacia la dirección del movimiento (estilo Honkai Star Rail / vista libre)
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            // Usamos Slerp para una rotación suave y fluida que gire completamente al personaje
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Aplica gravedad extra al caer para mejor sensación de salto
    /// </summary>
    public void ApplyBetterJumpPhysics()
    {
        if (Rb.velocity.y < 0)
        {
            // Falling - apply extra gravity
            Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Verifica si hay una superficie escalable enfrente
    /// </summary>
    public bool CheckClimbableSurface(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float actualCheckDist = IsClimbing ? climbCheckDistance * 1.5f : climbCheckDistance;

        // 1. Raycast (Precisión directa)
        if (Physics.Raycast(origin, transform.forward, out hit, actualCheckDist, climbableMask))
        {
            float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (surfaceAngle >= minClimbAngle && surfaceAngle <= maxClimbAngle)
            {
                return true;
            }
        }

        // 2. SphereCast (Cobertura y bordes)
        if (Physics.SphereCast(origin, 0.25f, transform.forward, out hit, actualCheckDist, climbableMask))
        {
            float surfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (surfaceAngle >= minClimbAngle && surfaceAngle <= maxClimbAngle)
            {
                return true;
            }
        }

        hit = default;
        return false;
    }
    #endregion

    #region Death/Respawn
    public void Die()
    {
        OnDie?.Invoke();
        TransitionToState(States.Dead());
    }

    public void Respawn()
    {
        transform.position = LastGroundedPosition;
        Rb.velocity = Vector3.zero;
        _lastAnalyticsPosition = transform.position;
        TransitionToState(States.Grounded());
        GameEvents.PlayerRespawn(transform.position);
    }

    /// <summary>
    /// Centraliza la comprobación de muerte por caída.
    /// Llamado desde PlayerGroundedState al aterrizar.
    /// Las 3 formas de morir están en este script:
    ///   1. Agua       - OnCollisionEnter / OnTriggerEnter con tag Water
    ///   2. Stamina    - HandleStaminaDepleted (evento OnStaminaDepleted)
    ///   3. Caída      - HandleLanding (fallDistance >= lethalFallHeight)
    /// </summary>
    public void HandleLanding(float fallDistance)
    {
        GameEvents.PlayerLanded(fallDistance);

        if (fallDistance >= lethalFallHeight)
        {
            DieWithCause(DeathCause.Fall);
        }
        else if (fallDistance >= safeFallHeight)
        {
            Debug.Log($"Hard landing from {fallDistance:F1}m");
        }
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Climb check
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * climbCheckDistance);

        // Safe position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LastGroundedPosition, 0.3f);
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"State: {CurrentState?.GetType().Name}");
        GUILayout.Label($"Grounded: {IsGrounded}");
        GUILayout.Label($"Velocity: {Rb.velocity:F2}");
        GUILayout.Label($"Speed: {new Vector3(Rb.velocity.x, 0, Rb.velocity.z).magnitude:F2}");
        GUILayout.EndArea();
    }
    #endregion


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            DieWithCause(DeathCause.Water);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            DieWithCause(DeathCause.Water);
        }
    }


    public void DieWithCause(DeathCause cause)
    {
        if (Input != null && Input.IsAI)
        {
            OnDie?.Invoke();
            return;
        }

        DeathManager.LastDeathCause = cause;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Pantalla de muerte");
    }


    private void OnEnable()
    {
        if (Stamina != null) Stamina.OnStaminaDepleted += HandleStaminaDepleted;
    }

    private void OnDisable()
    {
        if (Stamina != null) Stamina.OnStaminaDepleted -= HandleStaminaDepleted;
    }

    private void HandleStaminaDepleted()
    {
        DieWithCause(DeathCause.Stamina);
    }
}
