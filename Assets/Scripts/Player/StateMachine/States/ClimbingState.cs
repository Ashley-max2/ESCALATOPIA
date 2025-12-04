using UnityEngine;

public class ClimbingState : IState
{
    private ResistenceController resistenceController;
    private float consumoResistencia = 8f;
    private Vector3 ultimaPosicionPared;

    public void Enter(PlayerController p)
    {
        resistenceController = p.GetComponent<ResistenceController>();

        // Verificar si tiene resistencia al entrar
        if (!resistenceController.TieneResistencia(1f))
        {
            Debug.Log("No tiene suficiente resistencia para escalar");
            p.CambiarEstado(new IdleState());
            return;
        }

        // Guardar posicion inicial para referencia
        ultimaPosicionPared = p.transform.position;

        IniciarEscalada(p);
        Debug.Log("Entrando en ClimbingState - Resistencia: " + resistenceController.GetResistenciaActual());
    }

    public void Exit(PlayerController p)
    {
        PararEscalada(p);
        Debug.Log("Saliendo de ClimbingState - Resistencia restante: " + resistenceController.GetResistenciaActual());
    }

    public void Update(PlayerController p)
    {
        // Verificar salto de pared
        if (p.inputSalto && resistenceController.TieneResistencia(10f))
        {
            p.CambiarEstado(new WallJumpState());
            return;
        }

        // Verificar si salio de la zona escalable PRIMERO
        if (!p.enZonaEscalada)
        {
            Debug.Log("Salio de zona escalable");
            p.CambiarEstado(new JumpState());
            return;
        }

        // Verificar resistencia continuamente
        if (!resistenceController.TieneResistenciaSuficiente())
        {
            Debug.Log("Resistencia agotada durante escalada");
            p.CambiarEstado(new JumpState());
            return;
        }

        // Si suelta la tecla E, salir
        if (!p.inputEscalar)
        {
            Debug.Log("Dejo de presionar E");
            p.CambiarEstado(new JumpState());
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

        // Movimiento horizontal limitado
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        Vector3 movHorizontal = p.transform.right * inputHorizontal * p.velocidadEscalada * 0.5f * Time.deltaTime;

        // Aplicar movimiento combinado
        p.transform.Translate(movimiento + movHorizontal);

        // Consumir resistencia proporcional al movimiento
        float consumo = consumoResistencia * Time.deltaTime *
                       (Mathf.Abs(inputVertical) + Mathf.Abs(inputHorizontal) * 0.5f);
        resistenceController.ConsumirResistencia(consumo);

        // Debug para ver consumo de resistencia
        if (inputVertical != 0 || inputHorizontal != 0)
        {
            Debug.Log($"Escalando - Resistencia: {resistenceController.GetResistenciaActual():F1}, Consumo: {consumo:F2}");
        }
    }

    private void PararEscalada(PlayerController p)
    {
        p.rb.useGravity = true;
    }
}
