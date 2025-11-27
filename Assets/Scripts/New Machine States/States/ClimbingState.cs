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
        Debug.Log("Entrando en ClimbingState");
    }

    public void Exit(PlayerController p)
    {
        PararEscalada(p);
        Debug.Log("Saliendo de ClimbingState");
    }

    public void Update(PlayerController p)
    {
        // Verificar salto de pared
        if (p.inputSalto)
        {
            p.CambiarEstado(new WallJumpState());
            return;
        }

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

        // Movimiento horizontal limitado
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputHorizontal) > 0.1f)
        {
            Vector3 movHorizontal = p.transform.right * inputHorizontal * p.velocidadEscalada * 0.5f * Time.deltaTime;
            p.transform.Translate(movHorizontal);
        }

        // Consumir resistencia proporcional al movimiento
        float consumo = consumoResistencia * Time.deltaTime *
                       (Mathf.Abs(inputVertical) + Mathf.Abs(inputHorizontal) * 0.5f);
        resistenceController.ConsumirResistencia(consumo);
    }

    private void PararEscalada(PlayerController p)
    {
        p.rb.useGravity = true;
    }
}