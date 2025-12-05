using UnityEngine;

public class MovementState : IState
{
    public void Enter(PlayerController p)
    {
        Debug.Log("Entrando en MovementState");
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
        // Solo mover en el suelo
        if (!p.EstaEnSuelo())
        {
            return;
        }

        Vector3 direccion = (p.cam.right * p.inputH + p.cam.forward * p.inputV).normalized;
        direccion.y = 0;

        float velocidad = p.inputCorrer ? p.velocidadCorrer : p.velocidadCaminar;
        Vector3 movimientoObjetivo = direccion * velocidad;

        // Usar velocidad directa solo en XZ, mantener Y de la física
        Vector3 velocidadActual = new Vector3(p.rb.velocity.x, 0, p.rb.velocity.z);
        Vector3 cambioVelocidad = movimientoObjetivo - velocidadActual;
        
        // Aplicar cambio de velocidad suavemente
        p.rb.AddForce(cambioVelocidad * 10f, ForceMode.Acceleration);

        // Rotación suave
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            p.transform.rotation = Quaternion.Slerp(p.transform.rotation, rotacionObjetivo, p.rotacionSuavidad * Time.deltaTime);
        }
    }

    public void Exit(PlayerController p)
    {
        Debug.Log("Saliendo de MovementState");
    }
}