using UnityEngine;

/// <summary>
/// Estado de muerte del jugador.
/// Sin pantalla de muerte: espera a que un sistema externo (HazardTeleporter)
/// haga fade + teletransporte al checkpoint.
/// </summary>
public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(PlayerStateMachine context, PlayerStateFactory factory)
        : base(context, factory) { }

    public override void Enter()
    {
        // Stop all movement
        ctx.Rb.velocity = Vector3.zero;
        ctx.Rb.isKinematic = true;

        // Notify death
        GameEvents.PlayerDeath(ctx.transform.position);

        // Asegurar juego activo para que el HazardTeleporter pueda hacer fade correctamente.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PlayerDeadState] Player died!");
    }

    public override void Execute()
    {
        // Sin input/manual UI: espera a que HazardTeleporter detecte PlayerDeadState
        // y haga el teletransporte al checkpoint con fade.
    }

    public override void FixedExecute()
    {
    }

    public override void Exit()
    {
        ctx.Rb.isKinematic = false;
    }
}
