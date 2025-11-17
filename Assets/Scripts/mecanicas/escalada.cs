using UnityEngine;

public class Escalada : MonoBehaviour
{
    public float velocidadEscalada = 3f;
    private bool puedeEscalar = false;
    private bool escalando = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Si puede escalar y el jugador pulsa una tecla
        if (puedeEscalar && Input.GetKey(KeyCode.E))
        {
            IniciarEscalada();
        }

        // Mientras escala
        if (escalando)
        {
            Escalar();

            // Si suelta la tecla, deja de escalar
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
        rb.useGravity = false;   // desactivamos gravedad
        rb.velocity = Vector3.zero;
    }

    void Escalar()
    {
        // Movimiento vertical hacia arriba
        transform.Translate(Vector3.up * velocidadEscalada * Time.deltaTime);
    }

    void PararEscalada()
    {
        escalando = false;
        rb.useGravity = true;    // recuperar gravedad
    }
}
