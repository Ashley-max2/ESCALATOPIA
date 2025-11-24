using UnityEngine;

public class JumpState : IState
{
    private bool saltoAplicado = false;

    public void Enter(PlayerController p)
    {
        AplicarSalto(p);
    }

    public void Exit(PlayerController p) { }

    public void Update(PlayerController p)
    {
        MoverEnAire(p);

        // Si estamos de nuevo en el suelo
        if (p.EstaEnSuelo() && p.rb.velocity.y <= 0.1f)
        {
            if (p.inputH != 0 || p.inputV != 0)
                p.CambiarEstado(new MovementState());
            else
                p.CambiarEstado(new IdleState());
        }
    }

    void AplicarSalto(PlayerController p)
    {
        if (saltoAplicado) return;

        p.rb.velocity = new Vector3(p.rb.velocity.x, 0, p.rb.velocity.z);
        p.rb.AddForce(Vector3.up * p.fuerzaSalto, ForceMode.VelocityChange);

        saltoAplicado = true;
    }

    void MoverEnAire(PlayerController p)
    {
        Vector3 dir = (p.cam.right * p.inputH + p.cam.forward * p.inputV).normalized;
        dir.y = 0;

        float vel = p.inputCorrer ? p.velocidadCorrer : p.velocidadCaminar;
        Vector3 velXZ = new Vector3(p.rb.velocity.x, 0, p.rb.velocity.z);
        Vector3 objetivo = dir * vel;

        Vector3 nuevaVel = Vector3.Lerp(velXZ, objetivo, 8f * Time.deltaTime);
        p.rb.velocity = new Vector3(nuevaVel.x, p.rb.velocity.y, nuevaVel.z);

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            p.transform.rotation =
                Quaternion.Slerp(p.transform.rotation, rot, p.rotacionSuavidad * Time.deltaTime);
        }
    }
}
