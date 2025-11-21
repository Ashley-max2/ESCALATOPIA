using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Player States/Falling")]
public class FallingState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        if (fsm.salto.EstaEnSuelo())
        {
            fsm.CambiarEstado(fsm.idleState);
        }
        else if (!fsm.fallDetector.playerLive)
        {
            fsm.CambiarEstado(fsm.deadState);
        }
    }

    public override void Exit(PlayerStateMachine fsm)
    {

    }

}
