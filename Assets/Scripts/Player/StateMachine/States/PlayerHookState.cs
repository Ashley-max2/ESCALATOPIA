using UnityEngine;
using FMODUnity;

/// <summary>
/// Estado cuando el jugador está usando el gancho.
/// Gestiona el disparo, movimiento hacia el punto de anclaje, atracción de objetos, y aterrizaje.
/// El modo (hookpoint / pull) se determina por GrapplingHook.ModoPull (se cambia con R).
/// Incluye aceleración inicial configurable para sensación de momentum.
/// 
/// Flujo hookpoint:
///   1. RopeThrowing  – la cuerda vuela parabólicamente hacia el hookpoint (el jugador espera)
///   2. Traveling      – la cuerda se tensa y el jugador viaja hacia el hookpoint
///   3. Arrived        – el jugador se detiene 1 metro más allá del hookpoint
/// </summary>
public class PlayerHookState : PlayerBaseState
{
    private enum HookPhase { RopeThrowing, Traveling, Arrived, PullingObject }
    private HookPhase _currentPhase;
    private Vector3 _hookTarget;
    private Vector3 _overshootTarget;   // hookTarget + 1m extra en la dirección del viaje
    private float _arrivalTimer;
    private float _travelTime;
    private Rigidbody _pulledObject;
    private const float ARRIVAL_HOLD_TIME = 0.2f;
    private const float OVERSHOOT_DISTANCE = 0.2f;  // 1 metro extra al llegar
    
    public PlayerHookState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        ctx.IsHooking = true;
        _currentPhase = HookPhase.RopeThrowing;
        _arrivalTimer = 0;
        _travelTime = 0;
        _pulledObject = null;
        
        if (ctx.GrapplingHook == null)
        {
            SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            return;
        }
        
        // Comprobar si tiene un objeto agarrado (no se puede usar el gancho)
        if (ctx.GrapplingHook.TieneObjetoAgarrado())
        {
            Debug.Log("No se puede usar el gancho con un objeto agarrado");
            SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            return;
        }
        
        // Decidir modo según ModoPull
        if (ctx.GrapplingHook.ModoPull)
        {
            // Modo atraer objeto
            Rigidbody pullTarget = ctx.GrapplingHook.FirePull();
            if (pullTarget != null)
            {
                _pulledObject = pullTarget;
                _hookTarget = pullTarget.position;
                _currentPhase = HookPhase.PullingObject;
                
                RuntimeManager.PlayOneShot("event:/SFX/Gancho/UsarGancho", ctx.transform.position);
                GameEvents.HookFired(_hookTarget);
                GameEvents.HookConnected();
                
                Debug.Log($"Hook pulling object {pullTarget.name}");
            }
            else
            {
                Debug.Log("No pullable object found");
                SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            }
        }
        else
        {
            // Modo hookpoint
            _hookTarget = ctx.GrapplingHook.Fire();
            if (_hookTarget != Vector3.zero)
            {
                // Fase 1: La cuerda vuela hacia el hookpoint (el jugador espera)
                _currentPhase = HookPhase.RopeThrowing;
                
                RuntimeManager.PlayOneShot("event:/SFX/Gancho/UsarGancho", ctx.transform.position);
                GameEvents.HookFired(_hookTarget);
                
                // Calcular el target con 1m extra en la dirección del viaje
                Vector3 dirToTarget = (_hookTarget - ctx.transform.position).normalized;
                _overshootTarget = _hookTarget + dirToTarget * OVERSHOOT_DISTANCE;
                
                // Congelar al jugador mientras la cuerda viaja
                ctx.Rb.velocity = Vector3.zero;
                ctx.Rb.useGravity = false;
                
                Debug.Log($"Hook rope throwing to {_hookTarget}");
            }
            else
            {
                SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            }
        }
    }
    
    public override void Execute()
    {
        switch (_currentPhase)
        {
            case HookPhase.RopeThrowing:
                HandleRopeThrowing();
                break;
            case HookPhase.Traveling:
                CheckArrival();
                break;
            case HookPhase.Arrived:
                HandleArrival();
                break;
            case HookPhase.PullingObject:
                HandlePullObject();
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
        switch (_currentPhase)
        {
            case HookPhase.RopeThrowing:
                // Mantener al jugador quieto mientras la cuerda viaja
                ctx.Rb.velocity = Vector3.zero;
                break;
            case HookPhase.Traveling:
                TravelToTarget();
                break;
            case HookPhase.PullingObject:
                PullObjectToPlayer();
                break;
        }
    }
    
    public override void Exit()
    {
        ctx.IsHooking = false;
        ctx.Rb.useGravity = true;
        
        ReleaseHook();
        _pulledObject = null;
        
        GameEvents.HookReleased();
    }
    
    /// <summary>
    /// Espera a que la cuerda (CuerdaRenderer) llegue al hookpoint.
    /// Cuando CuerdaLlegó es true, la cuerda se ha tensado y el jugador empieza a moverse.
    /// </summary>
    private void HandleRopeThrowing()
    {
        if (ctx.GrapplingHook == null) return;
        
        // Comprobar si la cuerda ha llegado al destino
        CuerdaRenderer cuerda = ctx.GrapplingHook.Cuerda;
        if (cuerda != null && cuerda.CuerdaLlegó)
        {
            // ¡La cuerda se ha tensado! Empezar a mover al jugador
            _currentPhase = HookPhase.Traveling;
            _travelTime = 0f;
            
            GameEvents.HookConnected();
            Debug.Log("Rope arrived! Player starts traveling.");
        }
    }
    
    /// <summary>
    /// Mueve al jugador hacia el punto de gancho con aceleración inicial.
    /// El destino es _overshootTarget (hookpoint + 1m extra).
    /// </summary>
    private void TravelToTarget()
    {
        if (ctx.GrapplingHook == null) return;
        
        _travelTime += Time.fixedDeltaTime;
        
        // Viajar hacia el overshoot target (1m más allá del hookpoint)
        Vector3 direction = (_overshootTarget - ctx.transform.position).normalized;
        float travelSpeed = ctx.GrapplingHook.TravelSpeed;
        
        // Aceleración progresiva (ease-out para sensación de momentum)
        float accelFactor = Mathf.Clamp01(_travelTime / ctx.HookAccelerationTime);
        float currentSpeed = travelSpeed * EaseOutQuad(accelFactor);
        
        // Mover hacia el objetivo
        ctx.Rb.velocity = direction * currentSpeed;
        
        // Rotar hacia la dirección del movimiento
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
        if (horizontalDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDir);
            ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }
    
    /// <summary>
    /// Atrae el objeto hacia el jugador
    /// </summary>
    private void PullObjectToPlayer()
    {
        if (_pulledObject == null || ctx.GrapplingHook == null) return;
        
        Vector3 direction = (ctx.transform.position - _pulledObject.position).normalized;
        _pulledObject.MovePosition(
            _pulledObject.position + direction * ctx.GrapplingHook.PullSpeed * Time.fixedDeltaTime
        );
        
        // Actualizar el target para el renderer de la cuerda
        ctx.GrapplingHook.CurrentTarget = _pulledObject.position;
    }
    
    /// <summary>
    /// Gestiona la finalización de la atracción de objeto
    /// </summary>
    private void HandlePullObject()
    {
        if (_pulledObject == null || ctx.GrapplingHook == null)
        {
            ReleaseHook();
            SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
            return;
        }
        
        float dist = Vector3.Distance(_pulledObject.position, ctx.transform.position);
        if (dist <= ctx.GrapplingHook.PullStopDistance)
        {
            // Objeto llegó al jugador
            _pulledObject.velocity = Vector3.zero;
            _pulledObject.angularVelocity = Vector3.zero;
            
            ReleaseHook();
            SwitchState(ctx.IsGrounded ? factory.Grounded() : factory.Airborne());
        }
    }
    
    private void CheckArrival()
    {
        // Comprobar distancia al overshoot target (hookpoint + 1m)
        float distanceToTarget = Vector3.Distance(ctx.transform.position, _overshootTarget);
        
        if (distanceToTarget < 1.5f)
        {
            _currentPhase = HookPhase.Arrived;
            ctx.Rb.velocity = Vector3.zero;
            
            // Colocar al jugador exactamente en el overshoot position
            ctx.Rb.MovePosition(_overshootTarget);
            
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
            
            // Check if there's a climbable surface and not exhausted
            RaycastHit hit;
            if (ctx.CheckClimbableSurface(out hit))
            {
                if (ctx.Stamina == null || !ctx.Stamina.IsExhausted)
                {
                    ctx.WallNormal = hit.normal;
                    SwitchState(factory.Climb());
                }
                else
                {
                    SwitchState(factory.Airborne());
                }
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
            if (ctx.GrapplingHook.IsPulling)
                ctx.GrapplingHook.ReleasePull();
            else
                ctx.GrapplingHook.Release();
        }
    }
    
    /// <summary>
    /// Curva ease-out cuadrática para aceleración natural
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}
