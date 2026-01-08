using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerStateMachine : MonoBehaviour
{
    // State Management
    public PlayerState CurrentState { get; set; }
    private PlayerStateFactory _states;

    // Components
    public Rigidbody Rb { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public HookSystem HookSystem { get; private set; }
    public ResistenceController Stamina { get; private set; }
    public Transform CameraTransform { get; private set; }

    [Header("Movement Config")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float climbSpeed = 3f;
    public float jumpForce = 12f;
    public float rotationSmoothTime = 0.1f;
    public float wallJumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Climbing")]
    public float climbCheckDistance = 1f;
    public LayerMask climbableMask;

    // Runtime vars
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isClimbing;
    [HideInInspector] public Vector3 wallNormal;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        InputHandler = GetComponent<PlayerInputHandler>();
        HookSystem = GetComponentInChildren<HookSystem>(); // Often child
        Stamina = GetComponent<ResistenceController>();
        CameraTransform = Camera.main.transform;

        _states = new PlayerStateFactory(this);
    }

    private void Start()
    {
        // Default state
        CurrentState = _states.Grounded();
        CurrentState.EnterState();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        CurrentState.UpdateState();
        CurrentState.CheckSwitchStates();
        
        // Check for Hook Override
        if (HookSystem != null && HookSystem.IsHooking && !(CurrentState is PlayerHookState))
        {
            // Transition to Hook State
            CurrentState.ExitState();
            CurrentState = _states.Hook();
            CurrentState.EnterState();
        }
    }

    public PlayerStateFactory StateFactory => _states;

    // Helper to get factory
    
    // Gizmos
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
