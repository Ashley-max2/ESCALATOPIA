using UnityEngine;

public class ClimbingState : IState
{
    private ResistenceController resistenceController;
    private float consumoResistencia = 8f;

    public void Enter(PlayerController p)
    {
        resistenceController = p.GetComponent<ResistenceController>();

        // Verificar si tiene resistencia al entrar
        if (!resistenceController.TieneResistencia(1f))
        {
            p.CambiarEstado(new IdleState());
            return;
        }

        IniciarEscalada(p);
    }

    public void Exit(PlayerController p)
    {
        PararEscalada(p);
    }

    public void Update(PlayerController p)
    {
        // Verificar resistencia continuamente
        if (!resistenceController.TieneResistenciaSuficiente())
        {
            p.CambiarEstado(new IdleState());
            return;
        }

        // Si suelta la tecla E, salir
        if (!p.inputEscalar)
        {
            p.CambiarEstado(new IdleState());
            return;
        }

        // Ejecutar escalada y consumir resistencia
        Escalar(p);
    }

    private void IniciarEscalada(PlayerController p)
    {
        p.rb.useGravity = false;
        p.rb.velocity = Vector3.zero;
        p.rb.angularVelocity = Vector3.zero;
    }

    private void Escalar(PlayerController p)
    {
        // Movimiento vertical
        float inputVertical = Input.GetAxisRaw("Vertical");
        Vector3 movimiento = Vector3.up * inputVertical * p.velocidadEscalada * Time.deltaTime;
        p.transform.Translate(movimiento);

        // Consumir resistencia proporcional al movimiento
        float consumo = consumoResistencia * Time.deltaTime * Mathf.Abs(inputVertical);
        resistenceController.ConsumirResistencia(consumo);
    }

    private void PararEscalada(PlayerController p)
    {
        p.rb.useGravity = true;
    }
}