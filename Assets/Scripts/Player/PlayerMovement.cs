using System.Collections;
using System.Collections.Generic;
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
    private Transform camaraTransform;
    private Rigidbody rb;

    // Input
    private float inputHorizontal, inputVertical;
    private bool inputCorrer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraTransform = Camera.main.transform;
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

        // Obtener la transform de la c�mara activa
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
            // **MOVIMIENTO EN 1RA PERSONA**: Usar la direcci�n de la C�MARA
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            // En 1ra persona, el personaje debe rotar para mirar donde mira la c�mara
            RotarPersonajeHaciaCamara(camaraTransform);
        }
        else
        {
            // **MOVIMIENTO EN 3RA PERSONA**: Usar direcci�n relativa a la c�mara
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            // Rotaci�n solo en 3ra persona cuando hay movimiento
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
            // Aplicar movimiento en XZ
            Vector3 velocidadObjetivo = direccionMovimiento * velocidadActual;
            Vector3 velocidadActualXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            // Suavizar el movimiento
            Vector3 nuevaVelocidadXZ = Vector3.Lerp(velocidadActualXZ, velocidadObjetivo, 10f * Time.fixedDeltaTime);

            rb.velocity = new Vector3(nuevaVelocidadXZ.x, rb.velocity.y, nuevaVelocidadXZ.z);

            // Rotar el personaje hacia la direcci�n del movimiento
            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
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
            // En 1ra persona, el personaje rota para seguir la direcci�n horizontal de la c�mara
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;

            if (direccionCamara != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionCamara);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo,
                                                    velocidadRotacion * Time.fixedDeltaTime);
            }
        }

        /* Debug en pantalla
        bool EstaMoviendose()
        {
            return (inputHorizontal != 0 || inputVertical != 0);
        }

        
        float GetVelocidadActual()
        {
            return inputCorrer ? velocidadCorrer : velocidadCaminar;
        }

        string GetEstadoMovimiento()
        {
            string estado = "Quieto";
            if (EstaMoviendose())
            {
                estado = inputCorrer ? "Corriendo" : "Caminando";
            }
            return estado;
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            string estado = $"MOVIMIENTO: {GetEstadoMovimiento()}\n";
            estado += $"Velocidad: {GetVelocidadActual()}\n";
            estado += $"Input: ({inputHorizontal:F1}, {inputVertical:F1})";

            GUI.Label(new Rect(10, 10, 300, 100), estado, style);
        }
        */
    }
}