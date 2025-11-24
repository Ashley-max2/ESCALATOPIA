using UnityEngine;

public class MovementState : IState
{
    public void Enter(PlayerController player)
    {
        Debug.Log("Entrando en Movement");
    }

    public void Exit(PlayerController player)
    {
        Debug.Log("Saliendo de Movement");
    }

    public void Update(PlayerController player)
    {
        ObtenerInput(player);
        Mover(player);

        // Si no hay input vuelve a Idle
        if (player.inputHorizontal == 0 && player.inputVertical == 0)
        {
            player.SetState(new IdleState());
        }
    }

    void ObtenerInput(PlayerController player)
    {
        player.inputHorizontal = Input.GetAxisRaw("Horizontal");
        player.inputVertical = Input.GetAxisRaw("Vertical");
        player.inputCorrer = Input.GetKey(KeyCode.LeftShift);
    }

    void Mover(PlayerController player)
    {
        if (player.rb == null) return;

        // Direccion relativa a la camara
        Vector3 direccionMovimiento =
            (player.camaraTransform.right * player.inputHorizontal +
             player.camaraTransform.forward * player.inputVertical).normalized;

        direccionMovimiento.y = 0;

        // Velocidad
        float velocidadActual = player.inputCorrer
            ? player.velocidadCorrer
            : player.velocidadCaminar;

        // Movimiento XZ
        Vector3 velocidadObjetivo = direccionMovimiento * velocidadActual;
        Vector3 velocidadActualXZ = new Vector3(player.rb.velocity.x, 0, player.rb.velocity.z);

        Vector3 nuevaVelocidadXZ = Vector3.Lerp(
            velocidadActualXZ,
            velocidadObjetivo,
            10f * Time.deltaTime
        );

        player.rb.velocity = new Vector3(
            nuevaVelocidadXZ.x,
            player.rb.velocity.y,
            nuevaVelocidadXZ.z
        );

        // Rotación
        if (direccionMovimiento != Vector3.zero)
        {
            RotarHaciaDireccion(player, direccionMovimiento);
        }
    }

    void RotarHaciaDireccion(PlayerController player, Vector3 direccion)
    {
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            rotacionObjetivo,
            player.velocidadRotacion * Time.deltaTime
        );
    }
}
