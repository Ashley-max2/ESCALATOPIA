using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerStateSO : ScriptableObject
{
    public abstract void Enter(PlayerStateMachine fsm);
    public abstract void Decide(PlayerStateMachine fsm);
    public abstract void Exit(PlayerStateMachine fsm);  
}
