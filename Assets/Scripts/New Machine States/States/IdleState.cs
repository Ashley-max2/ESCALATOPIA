using UnityEngine;

public class IdleState : IState
{
    public void Enter(PlayerController player)
    {
        Debug.Log("Entrando en Idle");
    }

    public void Exit(PlayerController player) { }

    public void Update(PlayerController player)
    {
        // Leer el input básico para detectar si hay movimiento
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            player.SetState(new MovementState());
        }
    }
}
