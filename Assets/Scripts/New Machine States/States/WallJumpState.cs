using UnityEngine;

public class WallJumpState : IState
{
    private float temporizadorBloqueo = 0f;
    private Vector3 direccionSalto;
    private ResistenceController resistenceController;
    private bool saltoAplicado = false;

    public void Enter(PlayerController p)
    {
        resistenceController = p.GetComponent<ResistenceController>();
        temporizadorBloqueo = p.tiempoBloqueoWallJump;
        saltoAplicado = false;

        // Verificar si tiene resistencia para el salto de pared
        if (!resistenceController.TieneResistencia(10f))
        {
            Debug.Log("No hay resistencia para salto de pared");
            p.CambiarEstado(new IdleState());
            return;
        }

        // Consumir resistencia
        resistenceController.ConsumirResistencia(10f);

        // Calcular dirección del salto
        CalcularDireccionSalto(p);
        AplicarSaltoDePared(p);

        Debug.Log("Entrando en WallJumpState");
    }

    public void Update(PlayerController p)
    {
        temporizadorBloqueo -= Time.deltaTime;

        // Durante el bloqueo, permitir rotación pero no movimiento
        if (temporizadorBloqueo > 0)
        {
            RotarDuranteSalto(p);
        }
        else
        {
            // Después del bloqueo, permitir control de aire
            MoverEnAire(p);
        }

        // Verificar transiciones
        if (p.EstaEnSuelo() && p.rb.velocity.y <= 0.1f)
        {
            TransicionarAlSuelo(p);
        }

        // Si el temporizador termina y estamos cayendo
        if (temporizadorBloqueo <= 0 && p.rb.velocity.y < 0)
        {
            p.CambiarEstado(new JumpState());
        }
    }

    public void Exit(PlayerController p)
    {
        // Reactivar el control completo al salir del estado
        saltoAplicado = false;
        Debug.Log("Saliendo de WallJumpState");
    }

    private void CalcularDireccionSalto(PlayerController p)
    {
        // Calcular dirección del salto de pared (opuesta a la pared)
        Vector3 normalPared = -p.transform.forward;

        direccionSalto = (normalPared * p.fuerzaWallJumpLateral + Vector3.up * p.fuerzaWallJump).normalized;
    }

    private void AplicarSaltoDePared(PlayerController p)
    {
        if (saltoAplicado) return;

        // Resetear velocidad
        Vector3 velocity = p.rb.velocity;
        velocity.y = 0;
        p.rb.velocity = velocity;

        // Aplicar fuerza del salto de pared
        Vector3 fuerzaTotal = direccionSalto * (p.fuerzaWallJump + p.fuerzaWallJumpLateral);
        p.rb.AddForce(fuerzaTotal, ForceMode.VelocityChange);

        saltoAplicado = true;
        Debug.Log("Wall Jump aplicado! Dirección: " + direccionSalto);
    }

    private void RotarDuranteSalto(PlayerController p)
    {
        // Rotar hacia la dirección del salto durante el bloqueo
        if (direccionSalto != Vector3.zero)
        {
            Vector3 direccionHorizontal = new Vector3(direccionSalto.x, 0, direccionSalto.z);
            if (direccionHorizontal != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direccionHorizontal);
                p.transform.rotation = Quaternion.Slerp(p.transform.rotation, targetRotation, p.rotacionSuavidad * Time.deltaTime);
            }
        }
    }

    private void MoverEnAire(PlayerController p)
    {
        // Control de aire después del bloqueo
        Vector3 dir = (p.cam.right * p.inputH + p.cam.forward * p.inputV).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            float vel = p.velocidadCaminar * 0.7f; // Reducida en aire
            Vector3 velXZ = new Vector3(p.rb.velocity.x, 0, p.rb.velocity.z);
            Vector3 objetivo = dir * vel;

            Vector3 nuevaVel = Vector3.Lerp(velXZ, objetivo, 6f * Time.deltaTime);
            p.rb.velocity = new Vector3(nuevaVel.x, p.rb.velocity.y, nuevaVel.z);

            // Rotación suave hacia la dirección de movimiento
            Quaternion rot = Quaternion.LookRotation(dir);
            p.transform.rotation = Quaternion.Slerp(p.transform.rotation, rot, p.rotacionSuavidad * Time.deltaTime);
        }
    }

    private void TransicionarAlSuelo(PlayerController p)
    {
        if (Mathf.Abs(p.rb.velocity.y) < 0.5f)
        {
            if (Mathf.Abs(p.inputH) > 0.1f || Mathf.Abs(p.inputV) > 0.1f)
                p.CambiarEstado(new MovementState());
            else
                p.CambiarEstado(new IdleState());
        }
    }
}