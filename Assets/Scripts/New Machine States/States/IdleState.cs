using UnityEngine;

public class IdleState : IState
{
    public void Enter(PlayerController player)
    {
        Debug.Log("Entrando en Idle");
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("Saliendo de Idle");
    }

    public void Update(PlayerController player)
    {
        Debug.Log("Idle...");
    }
}
