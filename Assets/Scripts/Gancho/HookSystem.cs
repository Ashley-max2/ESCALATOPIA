using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookSystem : MonoBehaviour
{
    [Header("Dependencies")]
    public HookTargetFinder targetFinder;
    public Camera playerCamera;

    public HookMovementController hookMovement;
    public HookVisualController hookVisual;
    public HookInputController hookInput;

    // Referencia al player
    public Transform playerTransform;
    public Rigidbody playerRigidbody;

    private HookState currentState;
    public IHookable CurrentHookPoint { get; set; }
    public Vector3 HookOrigin => hookVisual.hookOrigin.position;
    public Vector3 PlayerPosition => playerTransform.position;

    // Propiedades públicas
    public HookTargetFinder TargetFinder => targetFinder;
    public HookMovementController HookMovement => hookMovement;
    public HookVisualController HookVisual => hookVisual;
    public Rigidbody PlayerRigidbody => playerRigidbody;

    void Start()
    {
        InitializeComponents();
        ChangeState(new HookIdleState(this));
    }

    void Update()
    {
        currentState?.Update();
        hookInput.ProcessInput(this);
    }

    private void InitializeComponents()
    {
        if (playerTransform == null)
            playerTransform = transform.parent;

        if (playerRigidbody == null && playerTransform != null)
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();

        targetFinder ??= GetComponent<HookTargetFinder>();
        hookMovement ??= GetComponent<HookMovementController>();
        hookVisual ??= GetComponent<HookVisualController>();
        hookInput ??= GetComponent<HookInputController>();
    }

    public void ChangeState(HookState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void LaunchHook() => ChangeState(new HookThrownState(this));
    public void CancelHook() => ChangeState(new HookIdleState(this));

    // Antes:
    // public bool IsHooking => !(currentState is HookIdleState);

    public bool IsHooking => !(currentState is HookIdleState);
    public bool IsAiming => currentState is HookAimingState;
    public bool IsThrown => currentState is HookThrownState;
    public bool IsAttached => currentState is HookAttachedState;
}