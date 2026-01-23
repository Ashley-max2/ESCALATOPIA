using UnityEngine;

/// <summary>
/// Estado cuando el jugador está usando el gancho.
/// Gestiona el disparo, movimiento hacia el punto de anclaje, y aterrizaje.
/// </summary>
public class PlayerHookState : PlayerBaseState
{
    private enum HookPhase { Firing, Traveling, Arrived }
    private HookPhase _currentPhase;
    private Vector3 _hookTarget;
    private float _arrivalTimer;
    private const float ARRIVAL_HOLD_TIME = 0.2f;
    
    public PlayerHookState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        ctx.IsHooking = true;
        _currentPhase = HookPhase.Firing;
        _arrivalTimer = 0;
        
        // Fire the hook
        if (ctx.GrapplingHook != null)
        {
            _hookTarget = ctx.GrapplingHook.Fire();
            if (_hookTarget != Vector3.zero)
            {
                _currentPhase = HookPhase.Traveling;
                GameEvents.HookFired(_hookTarget);
                GameEvents.HookConnected();
                
                // Disable gravity during hook travel
                ctx.Rb.useGravity = false;
                
                Debug.Log($"Hook fired to {_hookTarget}");
            }
            else
            {
                // No valid target, return to previous state
                SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            }
        }
        else
        {
            SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
        }
    }
    
    public override void Execute()
    {
        switch (_currentPhase)
        {
            case HookPhase.Traveling:
                CheckArrival();
                break;
            case HookPhase.Arrived:
                HandleArrival();
                break;
        }
        
        // Manual release
        if (ctx.Input.HookReleasePressed)
        {
            ReleaseHook();
            SwitchState(factory.Airborne());
        }
    }
    
    public override void FixedExecute()
    {
        if (_currentPhase == HookPhase.Traveling)
        {
            TravelToTarget();
        }
    }
    
    public override void Exit()
    {
        ctx.IsHooking = false;
        ctx.Rb.useGravity = true;
        
        if (ctx.GrapplingHook != null)
        {
            ctx.GrapplingHook.Release();
        }
        
        GameEvents.HookReleased();
    }
    
    private void TravelToTarget()
    {
        if (ctx.GrapplingHook == null) return;
        
        Vector3 direction = (_hookTarget - ctx.transform.position).normalized;
        float travelSpeed = ctx.GrapplingHook.TravelSpeed;
        
        // Move towards target
        ctx.Rb.velocity = direction * travelSpeed;
        
        // Rotate to face travel direction
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
        if (horizontalDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDir);
            ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }
    
    private void CheckArrival()
    {
        float distanceToTarget = Vector3.Distance(ctx.transform.position, _hookTarget);
        
        if (distanceToTarget < 1.5f)
        {
            _currentPhase = HookPhase.Arrived;
            ctx.Rb.velocity = Vector3.zero;
            
            // Update safe position
            ctx.LastGroundedPosition = ctx.transform.position;
            ctx.FallStartHeight = 0;
        }
    }
    
    private void HandleArrival()
    {
        _arrivalTimer += Time.deltaTime;
        
        // Brief pause at arrival point
        if (_arrivalTimer >= ARRIVAL_HOLD_TIME)
        {
            ReleaseHook();
            
            // Check if there's a climbable surface
            RaycastHit hit;
            if (ctx.CheckClimbableSurface(out hit))
            {
                ctx.WallNormal = hit.normal;
                SwitchState(factory.Climb());
            }
            else if (ctx.IsGrounded)
            {
                SwitchState(factory.Grounded());
            }
            else
            {
                SwitchState(factory.Airborne());
            }
        }
    }
    
    private void ReleaseHook()
    {
        if (ctx.GrapplingHook != null)
        {
            ctx.GrapplingHook.Release();
        }
    }
}
