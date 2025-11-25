using UnityEngine;

public class IdleState : IState
{
    public void Enter(PlayerController p)
    {
        // Opcional: animación de idle
    }

    public void Update(PlayerController p)
    {
        // Verificar escalada (prioridad alta)
        if (p.PuedeIniciarEscalada())
        {
            p.CambiarEstado(new ClimbingState());
            return;
        }

        // Verificar movimiento
        if (Mathf.Abs(p.inputH) > 0.1f || Mathf.Abs(p.inputV) > 0.1f)
        {
            p.CambiarEstado(new MovementState());
            return;
        }

        // Verificar salto
        if (p.inputSalto && p.EstaEnSuelo())
        {
            p.CambiarEstado(new JumpState());
            return;
        }
    }

    public void Exit(PlayerController p)
    {
        // Limpieza si es necesaria
    }
}