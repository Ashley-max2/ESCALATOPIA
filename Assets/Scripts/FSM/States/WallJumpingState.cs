using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/WallJump")]
public class WallJumpingState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.saltoEscalado.enabled = true;
        fsm.movimiento.enabled = false;
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        // Cuando termina el wall jump, pasa a Falling
        if (!fsm.saltoEscalado.enabled)
        {
            fsm.CambiarEstado(fsm.fallingState);
        }
    }

    public override void Exit(PlayerStateMachine fsm)
    {
        fsm.movimiento.enabled = true;
    }

    

}
