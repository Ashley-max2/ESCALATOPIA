using UnityEngine;

public class JumpState : IState
{
    private bool saltoAplicado = false;
    private float tiempoEnAire = 0f;

    public void Enter(PlayerController p)
    {
        saltoAplicado = false;
        tiempoEnAire = 0f;
        AplicarSalto(p);
        Debug.Log("Entrando en JumpState");
    }

    public void Exit(PlayerController p)
    {
        saltoAplicado = false;
        Debug.Log("Saliendo de JumpState");
    }

    public void Update(PlayerController p)
    {
        tiempoEnAire += Time.deltaTime;
        MoverEnAire(p);

        TransicionarAlSuelo(p);
    }

    void AplicarSalto(PlayerController p)
    {
        if (saltoAplicado) return;

        // Resetear velocidad Y para salto consistente
        Vector3 velocity = p.rb.velocity;
        velocity.y = 0;
        p.rb.velocity = velocity;

        // Aplicar fuerza de salto
        p.rb.AddForce(Vector3.up * p.fuerzaSalto, ForceMode.VelocityChange);
        saltoAplicado = true;

        Debug.Log("Salto aplicado con fuerza: " + p.fuerzaSalto);
    }

    void TransicionarAlSuelo(PlayerController p)
    {
        // Pequeña tolerancia para detectar suelo estable
        if (Mathf.Abs(p.rb.velocity.y) < 2f)
        {
            if (Mathf.Abs(p.inputH) > 0.1f || Mathf.Abs(p.inputV) > 0.1f)
                p.CambiarEstado(new MovementState());
            else
                p.CambiarEstado(new IdleState());
        }

        // Para cambiar al estado escalada y seguir escalando
        if (p.PuedeIniciarEscalada())
        {
            p.CambiarEstado(new ClimbingState());
            return;
        }
    }

    void MoverEnAire(PlayerController p)
    {
        Vector3 dir = (p.cam.right * p.inputH + p.cam.forward * p.inputV).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            float vel = p.inputCorrer ? p.velocidadCorrer : p.velocidadCaminar;
            Vector3 velXZ = new Vector3(p.rb.velocity.x, 0, p.rb.velocity.z);
            Vector3 objetivo = dir * vel;

            Vector3 nuevaVel = Vector3.Lerp(velXZ, objetivo, 8f * Time.deltaTime);
            p.rb.velocity = new Vector3(nuevaVel.x, p.rb.velocity.y, nuevaVel.z);

            Quaternion rot = Quaternion.LookRotation(dir);
            p.transform.rotation =
                Quaternion.Slerp(p.transform.rotation, rot, p.rotacionSuavidad * Time.deltaTime);
        }
    }
}