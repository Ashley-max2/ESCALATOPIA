using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los estados del jugador.
/// Proporciona acceso al contexto (PlayerStateMachine) y métodos de transición.
/// </summary>
public abstract class PlayerBaseState : IState
{
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;
    
    public PlayerBaseState(PlayerStateMachine context, PlayerStateFactory stateFactory)
    {
        ctx = context;
        factory = stateFactory;
    }
    
    public abstract void Enter();
    public abstract void Execute();
    public abstract void FixedExecute();
    public abstract void Exit();
    
    /// <summary>
    /// Cambia a un nuevo estado de forma segura
    /// </summary>
    protected void SwitchState(IState newState)
    {
        ctx.TransitionToState(newState);
    }
    
    /// <summary>
    /// Verifica si el buffer de salto está activo
    /// </summary>
    protected bool IsJumpBuffered()
    {
        return Time.time - ctx.LastJumpPressTime < ctx.JumpBufferTime;
    }
    
    /// <summary>
    /// Verifica si el coyote time está activo
    /// </summary>
    protected bool IsCoyoteTimeActive()
    {
        return Time.time - ctx.LastGroundedTime < ctx.CoyoteTime;
    }
}
