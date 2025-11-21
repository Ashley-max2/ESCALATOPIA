using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/Dead")]
public class DeadState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.movimiento.enabled = false;
        fsm.salto.enabled = false;
        fsm.escalada.enabled = false;
        fsm.saltoEscalado.enabled = false;
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        // Esperar respawn
        if (fsm.fallDetector.playerLive)
        {
            fsm.CambiarEstado(fsm.idleState);
        }
    }

    public override void Exit(PlayerStateMachine fsm) 
    { 

    }

}
