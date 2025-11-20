using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/Walking")]
public class WalkingState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.movimiento.enabled = true;
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        if (!fsm.movimiento.EstaMoviendose()) 
        {
            fsm.CambiarEstado(fsm.idleState);
        }
        else if (fsm.movimiento.GetVelocidadActual() > 5) 
        {
            fsm.CambiarEstado(fsm.runningState);
        }
        else if (fsm.salto.EstaSaltando())
        {
            fsm.CambiarEstado(fsm.jumpingState);
        }
    }

    public override void Exit(PlayerStateMachine fsm)
    {

    }
}
