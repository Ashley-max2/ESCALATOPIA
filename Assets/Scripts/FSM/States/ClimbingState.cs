using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/Climbing")]
public class ClimbingState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.escalada.enabled = true;
        fsm.movimiento.enabled = false;
        fsm.salto.enabled = false;
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        if (!fsm.escalada.EstaEscalando())
        {
            fsm.CambiarEstado(fsm.idleState);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.CambiarEstado(fsm.wallJumpingState);
        }
    }

    public override void Exit(PlayerStateMachine fsm)
    {
        fsm.escalada.ForzarFinEscalada();
    }

}
