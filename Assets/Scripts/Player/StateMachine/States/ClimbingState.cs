using UnityEngine;

public class ClimbingState : IState
{
    private ResistenceController resistenceController;
    private ClimbingHangingAnimController animController;
    private float consumoResistencia = 8f;
    private Vector3 ultimaPosicionPared;

    public void Enter(PlayerController p)
    {
        resistenceController = p.GetComponent<ResistenceController>();
        animController = p.GetComponent<ClimbingHangingAnimController>();

        // Verificar si tiene resistencia al entrar
        if (!resistenceController.TieneResistencia(1f))
        {
            Debug.Log("No tiene suficiente resistencia para escalar");
            p.CambiarEstado(new IdleState());
            return;
        }

        // Guardar posición inicial para referencia
        ultimaPosicionPared = p.transform.position;

        IniciarEscalada(p);
        
        // Activar animación de escalada
        if (animController != null)
        {
            animController.StartClimbing();
        }
        
        Debug.Log("Entrando en ClimbingState - Resistencia: " + resistenceController.GetResistenciaActual());
    }

    public void Exit(PlayerController p)
    {
        PararEscalada(p);
        
        // Detener animación de escalada
        if (animController != null)
        {
            animController.StopClimbing();
        }
        
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

        // Verificar resistencia continuamente
        if (!resistenceController.TieneResistenciaSuficiente())
        {
            Debug.Log("Se agot� la resistencia durante la escalada");
            p.CambiarEstado(new IdleState());
            return;
        }

        // Si suelta la tecla E, salir
        if (!p.inputEscalar)
        {
            Debug.Log("Solt� la tecla E, saliendo de escalada");
            p.CambiarEstado(new IdleState());
            return;
        }

        // Verificar que sigue en zona escalable - �AHORA FUNCIONA!
        if (!p.enZonaEscalada)
        {
            Debug.Log("Perdi� contacto con la pared escalable");
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

        // Movimiento horizontal limitado
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        Vector3 movHorizontal = p.transform.right * inputHorizontal * p.velocidadEscalada * 0.5f * Time.deltaTime;

        // Aplicar movimiento combinado
        p.transform.Translate(movimiento + movHorizontal);

        // Actualizar velocidad de escalada en el animator
        if (animController != null)
        {
            float climbSpeed = inputVertical + (inputHorizontal * 0.5f);
            animController.SetClimbingSpeed(climbSpeed);
        }

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