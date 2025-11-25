using UnityEngine;

public class MovementState : IState
{
    public void Enter(PlayerController p)
    {
        // Opcional: sonido de pasos, etc.
    }

    public void Update(PlayerController p)
    {
        // Verificar escalada (prioridad alta)
        if (p.PuedeIniciarEscalada())
        {
            p.CambiarEstado(new ClimbingState());
            return;
        }

        // Verificar salto
        if (p.inputSalto && p.EstaEnSuelo())
        {
            p.CambiarEstado(new JumpState());
            return;
        }

        // Si dejó de moverse, volver a idle
        if (Mathf.Abs(p.inputH) < 0.1f && Mathf.Abs(p.inputV) < 0.1f)
        {
            p.CambiarEstado(new IdleState());
            return;
        }

        // Mover al jugador
        MoverJugador(p);
    }

    private void MoverJugador(PlayerController p)
    {
        Vector3 direccion = (p.cam.right * p.inputH + p.cam.forward * p.inputV).normalized;
        direccion.y = 0;

        float velocidad = p.inputCorrer ? p.velocidadCorrer : p.velocidadCaminar;
        Vector3 movimiento = direccion * velocidad;

        // Aplicar movimiento
        p.rb.velocity = new Vector3(movimiento.x, p.rb.velocity.y, movimiento.z);

        // Rotación suave
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            p.transform.rotation = Quaternion.Slerp(p.transform.rotation, rotacionObjetivo, p.rotacionSuavidad * Time.deltaTime);
        }
    }
}