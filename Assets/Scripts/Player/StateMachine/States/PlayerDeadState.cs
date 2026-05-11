using UnityEngine;

/// <summary>
/// Estado de muerte del jugador.
/// Muestra la pantalla de muerte vía PlayerDeathHandler y espera input para respawnear.
/// </summary>
public class PlayerDeadState : PlayerBaseState
{
    private float _deathTimer;
    private const float INPUT_DELAY = 1.5f; // Segundos antes de aceptar input (evita skips)
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
        
        // Mostrar pantalla de muerte (pausa el juego con timeScale = 0)
        if (PlayerDeathHandler.Instance != null)
        {
            PlayerDeathHandler.Instance.ShowDeathScreen();
        }
        
        Debug.Log("[PlayerDeadState] Player died!");
    }
    
    public override void Execute()
    {
        // Usar unscaledDeltaTime porque el juego está pausado (timeScale = 0)
        _deathTimer += Time.unscaledDeltaTime;
        
        // Esperar un poco antes de aceptar input
        if (_deathTimer < INPUT_DELAY) return;
        
        // Cualquier tecla → respawn
        if (!_respawnTriggered && Input.anyKeyDown)
        {
            _respawnTriggered = true;
            
            if (PlayerDeathHandler.Instance != null)
            {
                PlayerDeathHandler.Instance.HideAndRespawn();
            }
            else
            {
                // Fallback si no hay handler
                Time.timeScale = 1f;
                ctx.Respawn();
            }
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
