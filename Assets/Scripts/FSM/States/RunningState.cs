using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/Running")]
public class RunningState : PlayerStateSO
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
        else if (fsm.movimiento.GetVelocidadActual() <= 5f)
        {
            fsm.CambiarEstado(fsm.walkingState);
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
