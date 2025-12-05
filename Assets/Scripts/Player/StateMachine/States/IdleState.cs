using UnityEngine;

public class IdleState : IState
{
    public void Enter(PlayerController p)
    {
        Debug.Log("Entrando en IdleState");
    }

    public void Update(PlayerController p)
    {
        // Verificar escalada (prioridad alta)
        if (p.PuedeIniciarEscalada())
        {
            p.CambiarEstado(new ClimbingState());
            return;
        }

        // Si no está en el suelo, pasar a JumpState (está cayendo)
        if (!p.EstaEnSuelo())
        {
            p.CambiarEstado(new JumpState());
            return;
        }

        // Verificar movimiento (solo si está en el suelo)
        if (Mathf.Abs(p.inputH) > 0.1f || Mathf.Abs(p.inputV) > 0.1f)
        {
            p.CambiarEstado(new MovementState());
            return;
        }

        // Verificar salto
        if (p.inputSalto)
        {
            p.CambiarEstado(new JumpState());
            return;
        }
    }

    public void Exit(PlayerController p)
    {
        Debug.Log("Saliendo de IdleState");
    }
}