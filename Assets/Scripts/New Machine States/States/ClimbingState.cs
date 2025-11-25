using UnityEngine;

public class ClimbingState : IState
{
    private ResistenceController resistenceController;
    private float consumoResistencia = 8f;
    private Vector3 puntoInicioEscalada;

    public void Enter(PlayerController p)
    {
        resistenceController = p.GetComponent<ResistenceController>();
        puntoInicioEscalada = p.transform.position;
        IniciarEscalada(p);
    }

    public void Exit(PlayerController p)
    {
        PararEscalada(p);
    }

    public void Update(PlayerController p)
    {
        // Si suelta la tecla E o se queda sin resistencia, salir
        if (!p.inputEscalar || !resistenceController.TieneResistencia(1f))
        {
            p.CambiarEstado(new IdleState());
            return;
        }

        // Ejecutar escalada
        Escalar(p);
    }

    private void IniciarEscalada(PlayerController p)
    {
        p.rb.useGravity = false;
        p.rb.velocity = Vector3.zero;
        p.rb.angularVelocity = Vector3.zero;

        Debug.Log("Iniciando escalada");
    }

    private void Escalar(PlayerController p)
    {
        // Movimiento vertical con inputs
        float inputVertical = Input.GetAxisRaw("Vertical");
        Vector3 movimiento = Vector3.up * inputVertical * p.velocidadEscalada * Time.deltaTime;

        // Aplicar movimiento
        p.transform.Translate(movimiento);

        // Movimiento horizontal limitado
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputHorizontal) > 0.1f)
        {
            Vector3 movHorizontal = p.transform.right * inputHorizontal * p.velocidadEscalada * 0.5f * Time.deltaTime;
            p.transform.Translate(movHorizontal);
        }

        // Consumir resistencia (más si se mueve)
        float consumo = consumoResistencia * Time.deltaTime *
                       (Mathf.Abs(inputVertical) + Mathf.Abs(inputHorizontal) * 0.5f);
        resistenceController.ConsumirResistencia(consumo);
    }

    private void PararEscalada(PlayerController p)
    {
        p.rb.useGravity = true;
        Debug.Log("Parando escalada");
    }
}