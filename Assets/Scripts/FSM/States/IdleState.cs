using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Player States/Idle")]

public class IdleState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.movimiento.enabled = true;
        fsm.salto.enabled = true;
    }
    public override void Update(PlayerStateMachine fsm)
    {
        if (fsm.movimiento.EstaMoviendose())
        {
            if (fsm.movimiento.GetVelocidadActual() > 5f)
                fsm.CambiarEstado(fsm.runningState);
            else
                fsm.CambiarEstado(fsm.walkingState);
        }
        else if (fsm.salto.EstaSaltando())
        {
            fsm.CambiarEstado(fsm.jumpingState);
        }
        else if (fsm.escalada.EstaEscalando())
        {
            fsm.CambiarEstado(fsm.climbingState);
        }
        else if (!fsm.fallDetector.playerLive)
        {

        }
    }
    public override void Exit(PlayerStateMachine fsm)
    {

    }
}
