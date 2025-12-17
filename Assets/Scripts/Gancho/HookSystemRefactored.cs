using UnityEngine;

/// <summary>
/// Refactored HookSystem using SOLID principles.
/// Single Responsibility: Coordinate hook services and manage hook state.
/// Depends on abstractions (IHookUser, IHookTargetingService, IHookPhysicsService).
/// </summary>
public class HookSystemRefactored : MonoBehaviour
{
    [Header("Service Dependencies")]
    [SerializeField] private HookTargetFinderRefactored targetingService;
    [SerializeField] private HookPhysicsService physicsService;
    [SerializeField] private HookVisualController visualController;
    [SerializeField] private HookInputController inputController;

    [Header("Hook User")]
    private IHookUser hookUser;

    [Header("State")]
    private HookState currentState;
    private IHookable currentHookPoint;

    // Events (Observer pattern)
    public event HookEventHandler HookLaunched;
    public event HookEventHandler HookAttached;
    public event HookEventHandler HookImpulseStarted;
    public event HookEventHandler HookImpulseEnded;
    public event HookEventHandler HookCancelled;

    // Public properties
    public IHookable CurrentHookPoint => currentHookPoint;
    public bool IsHooking => !(currentState is HookIdleState);
    public bool IsAiming => currentState is HookAimingState;
    public bool IsThrown => currentState is HookThrownState;
    public bool IsAttached => currentState is HookAttachedState;

    // Service accessors
    public IHookTargetingService TargetingService => targetingService;
    public IHookPhysicsService PhysicsService => physicsService;

    private void Awake()
    {
        InitializeServices();
        InitializeHookUser();
    }

    private void Start()
    {
        ChangeState(new HookIdleState(this));
    }

    private void Update()
    {
        currentState?.Update();
        inputController?.ProcessInput(this);
    }

    /// <summary>
    /// Initialize all service dependencies.
    /// </summary>
    private void InitializeServices()
    {
        if (targetingService == null)
            targetingService = GetComponent<HookTargetFinderRefactored>();

        if (physicsService == null)
            physicsService = GetComponent<HookPhysicsService>();

        if (visualController == null)
            visualController = GetComponent<HookVisualController>();

        if (inputController == null)
            inputController = GetComponent<HookInputController>();

        // Validate services
        if (targetingService == null)
            Debug.LogError("[HookSystem] HookTargetFinderRefactored not found!");

        if (physicsService == null)
            Debug.LogError("[HookSystem] HookPhysicsService not found!");
    }

    /// <summary>
    /// Initialize the hook user (player).
    /// </summary>
    private void InitializeHookUser()
    {
        // Find IHookUser in parent (should be PlayerControllerRefactored)
        hookUser = GetComponentInParent<IHookUser>();

        if (hookUser == null)
        {
            Debug.LogError("[HookSystem] IHookUser not found in parent! Make sure PlayerControllerRefactored implements IHookUser.");
        }
    }

    /// <summary>
    /// Change the hook state.
    /// </summary>
    public void ChangeState(HookState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    /// <summary>
    /// Launch the hook.
    /// </summary>
    public void LaunchHook()
    {
        if (targetingService.CurrentTarget == null)
        {
            Debug.LogWarning("[HookSystem] No valid target to launch hook");
            return;
        }

        ChangeState(new HookThrownState(this));
        
        // Notify hook user
        hookUser?.OnHookLaunched();

        // Raise event
        OnHookLaunched(new HookEventArgs
        {
            HookPoint = targetingService.CurrentTarget,
            HookPosition = targetingService.CurrentTarget.HookPoint,
            EventType = HookEventType.Launched
        });
    }

    /// <summary>
    /// Attach the hook to the current target.
    /// </summary>
    public void AttachHook()
    {
        currentHookPoint = targetingService.CurrentTarget;
        
        if (currentHookPoint == null)
        {
            Debug.LogWarning("[HookSystem] Cannot attach - no current target");
            return;
        }

        ChangeState(new HookAttachedState(this));

        // Notify hook user
        hookUser?.OnHookAttached(currentHookPoint);

        // Raise event
        OnHookAttached(new HookEventArgs
        {
            HookPoint = currentHookPoint,
            HookPosition = currentHookPoint.HookPoint,
            EventType = HookEventType.Attached
        });

        // Start impulse
        StartImpulse();
    }

    /// <summary>
    /// Start the hook impulse.
    /// </summary>
    private void StartImpulse()
    {
        if (hookUser == null || currentHookPoint == null)
            return;

        physicsService.ApplyImpulse(hookUser.Rigidbody, currentHookPoint.HookPoint);

        // Notify hook user
        hookUser?.OnHookImpulseStarted();

        // Raise event
        OnHookImpulseStarted(new HookEventArgs
        {
            HookPoint = currentHookPoint,
            HookPosition = currentHookPoint.HookPoint,
            EventType = HookEventType.ImpulseStarted
        });
    }

    /// <summary>
    /// Update the hook impulse.
    /// </summary>
    public void UpdateImpulse()
    {
        if (hookUser == null || currentHookPoint == null)
            return;

        physicsService.UpdateImpulse(
            hookUser.Transform.position,
            currentHookPoint.HookPoint,
            hookUser.Rigidbody
        );

        // Check if impulse finished
        if (!physicsService.IsImpulsing)
        {
            EndImpulse();
        }
    }

    /// <summary>
    /// End the hook impulse.
    /// </summary>
    private void EndImpulse()
    {
        // Notify hook user
        hookUser?.OnHookImpulseEnded();

        // Raise event
        OnHookImpulseEnded(new HookEventArgs
        {
            HookPoint = currentHookPoint,
            HookPosition = currentHookPoint?.HookPoint ?? Vector3.zero,
            EventType = HookEventType.ImpulseEnded
        });

        // Cancel hook
        CancelHook();
    }

    /// <summary>
    /// Cancel the hook.
    /// </summary>
    public void CancelHook()
    {
        physicsService.StopImpulse();
        currentHookPoint = null;
        
        ChangeState(new HookIdleState(this));

        // Restore gravity
        if (hookUser?.Rigidbody != null)
        {
            hookUser.Rigidbody.useGravity = true;
        }

        // Notify hook user
        hookUser?.OnHookCancelled();

        // Raise event
        OnHookCancelled(new HookEventArgs
        {
            EventType = HookEventType.Cancelled
        });
    }

    // Event raising methods
    protected virtual void OnHookLaunched(HookEventArgs e) => HookLaunched?.Invoke(this, e);
    protected virtual void OnHookAttached(HookEventArgs e) => HookAttached?.Invoke(this, e);
    protected virtual void OnHookImpulseStarted(HookEventArgs e) => HookImpulseStarted?.Invoke(this, e);
    protected virtual void OnHookImpulseEnded(HookEventArgs e) => HookImpulseEnded?.Invoke(this, e);
    protected virtual void OnHookCancelled(HookEventArgs e) => HookCancelled?.Invoke(this, e);
}
