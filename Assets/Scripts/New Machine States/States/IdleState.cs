using UnityEngine;

public class IdleState : IState
{
    public void Enter(PlayerController p)
    {
        // Debug.Log("IDLE");
    }

    public void Exit(PlayerController p) { }

    public void Update(PlayerController p)
    {
        if (p.inputSalto && p.EstaEnSuelo())
        {
            p.CambiarEstado(new JumpState());
            return;
        }

        if (p.inputH != 0 || p.inputV != 0)
        {
            p.CambiarEstado(new MovementState());
        }
    }
}
