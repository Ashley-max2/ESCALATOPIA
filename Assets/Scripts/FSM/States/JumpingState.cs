using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player States/Jumping")]
public class JumpingState : PlayerStateSO
{
    public override void Enter(PlayerStateMachine fsm)
    {
        fsm.salto.enabled = true;
    }

    public override void Decide(PlayerStateMachine fsm)
    {
        if (fsm.salto.EstaCayendo())
        {
            fsm.CambiarEstado(fsm.fallingState);
        }
    }

    public override void Exit(PlayerStateMachine fsm)
    {

    }

    
}
