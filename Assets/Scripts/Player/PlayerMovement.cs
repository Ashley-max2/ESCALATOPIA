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

    // NUEVO: referencia al HookSystem
    [Header("Gancho")]
    public HookSystem hookSystem;

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

        // Si no lo has asignado en el inspector, intenta encontrarlo en hijos
        if (hookSystem == null)
        {
            hookSystem = GetComponentInChildren<HookSystem>();
        }
    }

    void Update()
    {
        ObtenerInputMovimiento();
    }

    void FixedUpdate()
    {
        // SI EL GANCHO ESTÁ ACTIVO → NO TOCAR VELOCITY
        if (hookSystem != null && hookSystem.IsHooking)
            return;

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
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            RotarPersonajeHaciaCamara(camaraTransform);
        }
        else
        {
            Vector3 direccionCamara = camaraTransform.forward;
            direccionCamara.y = 0;
            direccionCamara.Normalize();

            Vector3 derechaCamara = camaraTransform.right;

            direccionMovimiento = (derechaCamara * inputHorizontal +
                                 direccionCamara * inputVertical).normalized;

            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }

        direccionMovimiento.y = 0;

        float velocidadActual = inputCorrer ? velocidadCorrer : velocidadCaminar;

        if (direccionMovimiento != Vector3.zero)
        {
            Vector3 movimiento = direccionMovimiento * velocidadActual;
            rb.velocity = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);

            Vector3 velocidadObjetivo = direccionMovimiento * velocidadActual;
            Vector3 velocidadActualXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 nuevaVelocidadXZ = Vector3.Lerp(velocidadActualXZ, velocidadObjetivo, 10f * Time.fixedDeltaTime);

            rb.velocity = new Vector3(nuevaVelocidadXZ.x, rb.velocity.y, nuevaVelocidadXZ.z);

            if (direccionMovimiento != Vector3.zero)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
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