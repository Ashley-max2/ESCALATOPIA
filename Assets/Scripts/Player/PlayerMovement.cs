using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float velocidadRotacion = 5f;

    [Header("Referencias")]
    public CameraManager cameraManager;
    public Camera camaraTerceraPersona;
    public Camera camaraPrimeraPersona;

    // Componentes
    private Rigidbody rb;

    // Input
    private float inputHorizontal, inputVertical;
    private bool inputCorrer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ObtenerInputMovimiento();
    }

    void FixedUpdate()
    {
        Mover();
    }

    void ObtenerInputMovimiento()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
        inputCorrer = Input.GetKey(KeyCode.LeftShift);
    }

    void Mover()
    {
        if (rb == null) return;

        Vector3 direccionMovimiento = Vector3.zero;
        bool enPrimeraPersona = cameraManager != null && cameraManager.EstaEnPrimeraPersona();

        // Obtener la transform de la cámara activa
        Transform camaraTransform = null;
        if (enPrimeraPersona && camaraPrimeraPersona != null)
        {
            camaraTransform = camaraPrimeraPersona.transform;
        }
        else if (!enPrimeraPersona && camaraTerceraPersona != null)
        {
            camaraTransform = camaraTerceraPersona.transform;
        }

        if (camaraTransform == null) return;

        if (enPrimeraPersona)
        {
            // **MOVIMIENTO EN 1RA PERSONA**: Usar la dirección de la CÁMARA
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            // En 1ra persona, el personaje debe rotar para mirar donde mira la cámara
            RotarPersonajeHaciaCamara(camaraTransform);
        }
        else
        {
            // **MOVIMIENTO EN 3RA PERSONA**: Usar dirección relativa a la cámara
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            // Rotación solo en 3ra persona cuando hay movimiento
            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }

        direccionMovimiento.y = 0;

        // Calcular velocidad
        float velocidadActual = inputCorrer ? velocidadCorrer : velocidadCaminar;

        // Aplicar movimiento
        if (direccionMovimiento != Vector3.zero)
        {
            Vector3 movimiento = direccionMovimiento * velocidadActual;
            rb.velocity = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        }
        else
        {
            // Detener movimiento horizontal pero mantener gravedad
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void RotarHaciaDireccion(Vector3 direccion)
    {
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo,
                                            velocidadRotacion * Time.fixedDeltaTime);
    }

    void RotarPersonajeHaciaCamara(Transform camaraTransform)
    {
        // En 1ra persona, el personaje rota para seguir la dirección horizontal de la cámara
        Vector3 direccionCamara = camaraTransform.forward;
        direccionCamara.y = 0;

        if (direccionCamara != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionCamara);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo,
                                                velocidadRotacion * Time.fixedDeltaTime);
        }
    }
}