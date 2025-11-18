using UnityEngine;

public class Escalada : MonoBehaviour
{
    public float velocidadEscalada = 3f;
    private bool puedeEscalar = false;
    private bool escalando = false;

    private ResistenceController resistenceController;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Obtener referencia al controlador de resistencia
        resistenceController = GetComponent<ResistenceController>();
    }

    void Update()
    {
        // Si puede escalar y el jugador pulsa E y tiene resistencia
        if (puedeEscalar && Input.GetKey(KeyCode.E) && resistenceController.TieneResistencia(1f))
        {
            IniciarEscalada();
        }

        // Mientras escala
        if (escalando)
        {
            // Si se queda sin resistencia se para
            if (!resistenceController.TieneResistencia(1f))
            {
                PararEscalada();
                return;
            }

            Escalar();

            // Dejar de escalar si suelta la tecla
            if (!Input.GetKey(KeyCode.E))
            {
                PararEscalada();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            puedeEscalar = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            puedeEscalar = false;
            PararEscalada();
        }
    }

    void IniciarEscalada()
    {
        escalando = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    void Escalar()
    {
        // Subir
        transform.Translate(Vector3.up * velocidadEscalada * Time.deltaTime);

        // Consumir resistencia cada frame
        resistenceController.ConsumirResistencia(8f * Time.deltaTime);
    }

    void PararEscalada()
    {
        escalando = false;
        rb.useGravity = true;
    }

public bool EstaEscalando()
{
    return escalando;
}

public void ForzarFinEscalada()
{
    escalando = false;
    rb.useGravity = true;
}
}