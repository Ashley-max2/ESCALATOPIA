using UnityEngine;

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
    
    #region State Management
    public IState CurrentState { get; private set; }
    private PlayerStateFactory _stateFactory;
    public PlayerStateFactory States => _stateFactory;
    #endregion
    
    #region Movement Settings
    [Header("=== MOVEMENT (Estilo Zelda BotW) ===")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;
    
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float RotationSpeed => rotationSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    #endregion
    
    #region Jump Settings
    [Header("=== JUMP ===")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float airControl = 0.5f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    public float JumpForce => jumpForce;
    public float AirControl => airControl;
    public float FallMultiplier => fallMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    #endregion
    
    #region Climbing Settings (Estilo Jusant)
    [Header("=== CLIMBING (Estilo Jusant) ===")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float climbStaminaCost = 10f;
    [SerializeField] private float wallJumpForce = 8f;
    [SerializeField] private float wallJumpUpwardForce = 6f;
    [SerializeField] private float climbCheckDistance = 0.6f;
    [SerializeField] private LayerMask climbableMask;
    
    public float ClimbSpeed => climbSpeed;
    public float ClimbStaminaCost => climbStaminaCost;
    public float WallJumpForce => wallJumpForce;
    public float WallJumpUpwardForce => wallJumpUpwardForce;
    public float ClimbCheckDistance => climbCheckDistance;
    public LayerMask ClimbableMask => climbableMask;
    #endregion
    
    #region Ground Check
    [Header("=== GROUND CHECK ===")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    
    public Transform GroundCheck => groundCheck;
    public float GroundCheckRadius => groundCheckRadius;
    public LayerMask GroundMask => groundMask;
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
    [HideInInspector] public float LastGroundedTime;
    [HideInInspector] public float LastJumpPressTime;
    
    // Velocity tracking for smooth movement
    [HideInInspector] public Vector3 CurrentVelocity;
    private float _currentRotationVelocity;
    #endregion
    
    #region Debug
    [Header("=== DEBUG ===")]
    [SerializeField] private bool showDebugInfo = true;
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
        // Initialize with grounded state
        TransitionToState(States.Grounded());
        LastGroundedPosition = transform.position;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        // Update ground check
        UpdateGroundCheck();
        
        // Update state
        CurrentState?.Execute();
        
        // Track jump buffer
        if (Input.JumpPressed)
            LastJumpPressTime = Time.time;
    }
    
    private void FixedUpdate()
    {
        CurrentState?.FixedExecute();
    }
    
    private void UpdateGroundCheck()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        
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
        
        // Rotate towards movement direction (estilo Zelda BotW)
        if (moveDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, 
                targetAngle, 
                ref _currentRotationVelocity, 
                1f / rotationSpeed
            );
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
    
    /// <summary>
    /// Aplica la mecánica de "mejor salto" estilo plataformero moderno
    /// </summary>
    public void ApplyBetterJumpPhysics()
    {
        if (Rb.velocity.y < 0)
        {
            // Falling - apply extra gravity
            Rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (Rb.velocity.y > 0 && !Input.JumpHeld)
        {
            // Rising but not holding jump - cut jump short
            Rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    /// <summary>
    /// Verifica si hay una superficie escalable enfrente
    /// </summary>
    public bool CheckClimbableSurface(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, transform.forward, out hit, climbCheckDistance, climbableMask);
    }
    #endregion
    
    #region Death/Respawn
    public void Die()
    {
        TransitionToState(States.Dead());
    }
    
    public void Respawn()
    {
        transform.position = LastGroundedPosition;
        Rb.velocity = Vector3.zero;
        TransitionToState(States.Grounded());
        GameEvents.PlayerRespawn(transform.position);
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
}
