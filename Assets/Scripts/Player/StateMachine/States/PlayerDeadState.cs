using UnityEngine;

/// <summary>
/// Estado de muerte del jugador.
/// Gestiona la muerte por caída y el respawn.
/// </summary>
public class PlayerDeadState : PlayerBaseState
{
    private float _deathTimer;
    private const float RESPAWN_DELAY = 2f;
    private bool _respawnTriggered;
    
    public PlayerDeadState(PlayerStateMachine context, PlayerStateFactory factory) 
        : base(context, factory) { }
    
    public override void Enter()
    {
        _deathTimer = 0;
        _respawnTriggered = false;
        
        // Stop all movement
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.isKinematic = true;
        
        // Notify death
        GameEvents.PlayerDeath(ctx.transform.position);
        
        Debug.Log("Player died!");
    }
    
    public override void Execute()
    {
        _deathTimer += Time.deltaTime;
        
        // Auto respawn after delay, or on input
        if (!_respawnTriggered && (_deathTimer >= RESPAWN_DELAY || ctx.Input.JumpPressed))
        {
            _respawnTriggered = true;
            ctx.Respawn();
        }
    }
    
    public override void FixedExecute()
    {
    }
    
    public override void Exit()
    {
        ctx.Rb.isKinematic = false;
    }
}
