using UnityEngine;

/// <summary>
/// Refactored PlayerController following SOLID principles.
/// Acts as a coordinator/facade for player components.
/// Single Responsibility: Coordinate components and manage state machine.
/// </summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovementComponent))]
[RequireComponent(typeof(PlayerClimbingComponent))]
[RequireComponent(typeof(PlayerStaminaComponent))]
public class PlayerControllerRefactored : MonoBehaviour, IHookUser
{
    [Header("Component References")]
    private PlayerInputHandler inputHandler;
    private PlayerMovementComponent movementComponent;
    private PlayerClimbingComponent climbingComponent;
    private PlayerStaminaComponent staminaComponent;
    private HookSystem hookSystem;

    [Header("State Machine")]
    private StateMachineBase<PlayerControllerRefactored> stateMachine;

    [Header("Hook Integration")]
    [SerializeField] private Transform hookOrigin;
    private bool isBeingImpulsed = false;

    // Public properties for components (Dependency Inversion)
    public PlayerInputHandler Input => inputHandler;
    public IMovementController Movement => movementComponent;
    public IClimbingController Climbing => climbingComponent;
    public IStaminaUser Stamina => staminaComponent;
    public HookSystem Hook => hookSystem;

    // IHookUser implementation
    public Transform Transform => transform;
    public Rigidbody Rigidbody => movementComponent?.Rigidbody;
    public Vector3 HookOriginPosition => hookOrigin != null ? hookOrigin.position : transform.position;
    public bool IsBeingImpulsed 
    { 
        get => isBeingImpulsed; 
        set => isBeingImpulsed = value; 
    }

    // State machine access
    public IStateMachine<PlayerControllerRefactored> StateMachine => stateMachine;

    private void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    private void Start()
    {
        // Start in idle state
        stateMachine.SetInitialState(new PlayerIdleState());
    }

    private void Update()
    {
        // Block input during hook impulse
        if (IsHookActive())
        {
            inputHandler.BlockMovementInput(true);
            
            // Automatically transition to hook impulse state if not already in it
            if (!stateMachine.IsInState<PlayerHookImpulseState>())
            {
                stateMachine.ChangeState(new PlayerHookImpulseState());
            }
        }
        else
        {
            inputHandler.BlockMovementInput(false);
        }

        // Update state machine
        stateMachine.UpdateStateMachine();

        // Debug input (P key)
        if (UnityEngine.Input.GetKeyDown(KeyCode.P))
        {
            staminaComponent.DebugStaminaState();
        }
    }

    /// <summary>
    /// Initialize all component references.
    /// </summary>
    private void InitializeComponents()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        movementComponent = GetComponent<PlayerMovementComponent>();
        climbingComponent = GetComponent<PlayerClimbingComponent>();
        staminaComponent = GetComponent<PlayerStaminaComponent>();
        hookSystem = GetComponentInChildren<HookSystem>();

        // Validate components
        if (inputHandler == null)
            Debug.LogError("[PlayerController] PlayerInputHandler not found!");
        if (movementComponent == null)
            Debug.LogError("[PlayerController] PlayerMovementComponent not found!");
        if (climbingComponent == null)
            Debug.LogError("[PlayerController] PlayerClimbingComponent not found!");
        if (staminaComponent == null)
            Debug.LogError("[PlayerController] PlayerStaminaComponent not found!");

        // Hook origin setup
        if (hookOrigin == null && hookSystem != null)
        {
            hookOrigin = hookSystem.transform;
        }
    }

    /// <summary>
    /// Initialize the state machine.
    /// </summary>
    private void InitializeStateMachine()
    {
        stateMachine = new StateMachineBase<PlayerControllerRefactored>(this);
    }

    /// <summary>
    /// Check if the hook is currently active.
    /// </summary>
    private bool IsHookActive()
    {
        if (hookSystem == null)
            return false;

        bool hookSystemActive = hookSystem.IsHooking;
        bool isImpulsing = hookSystem.HookMovement != null && hookSystem.HookMovement.IsImpulsing;
        bool hasHookPoint = hookSystem.CurrentHookPoint != null;

        return (hookSystemActive && (hookSystem.IsThrown || hookSystem.IsAttached)) || isImpulsing;
    }

    // IHookUser interface implementation
    public void OnHookLaunched()
    {
        Debug.Log("[Player] Hook launched");
    }

    public void OnHookAttached(IHookable hookPoint)
    {
        Debug.Log($"[Player] Hook attached to {hookPoint}");
    }

    public void OnHookImpulseStarted()
    {
        Debug.Log("[Player] Hook impulse started");
        isBeingImpulsed = true;
        stateMachine.ChangeState(new PlayerHookImpulseState());
    }

    public void OnHookImpulseEnded()
    {
        Debug.Log("[Player] Hook impulse ended");
        isBeingImpulsed = false;
        
        // Transition back to appropriate state
        if (movementComponent.IsGrounded)
        {
            stateMachine.ChangeState(new PlayerIdleState());
        }
        else
        {
            stateMachine.ChangeState(new PlayerJumpState());
        }
    }

    public void OnHookCancelled()
    {
        Debug.Log("[Player] Hook cancelled");
        isBeingImpulsed = false;
    }

    // Debug visualization
    private void OnGUI()
    {
        if (!Application.isPlaying)
            return;

        GUI.Label(new Rect(10, 30, 300, 20), $"State: {stateMachine.GetCurrentStateName()}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Grounded: {movementComponent.IsGrounded}");
        GUI.Label(new Rect(10, 70, 300, 20), $"In Climbable Zone: {climbingComponent.IsInClimbableZone}");
        GUI.Label(new Rect(10, 90, 300, 20), $"Stamina: {staminaComponent.CurrentStamina:F1}/{staminaComponent.MaxStamina:F1}");
        GUI.Label(new Rect(10, 110, 300, 20), $"Hook Active: {IsHookActive()}");
    }
}
