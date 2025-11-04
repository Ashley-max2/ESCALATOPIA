using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float velocidadRotacion = 10f;

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

        // Calcular dirección de movimiento relativa a la cámara
        Vector3 direccionMovimiento = (camaraTransform.right * inputHorizontal +
                                     camaraTransform.forward * inputVertical).normalized;
        direccionMovimiento.y = 0;

        // Calcular velocidad
        float velocidadActual = inputCorrer ? velocidadCorrer : velocidadCaminar;

        // Aplicar movimiento en XZ
        Vector3 velocidadObjetivo = direccionMovimiento * velocidadActual;
        Vector3 velocidadActualXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Suavizar el movimiento
        Vector3 nuevaVelocidadXZ = Vector3.Lerp(velocidadActualXZ, velocidadObjetivo, 10f * Time.fixedDeltaTime);

        rb.velocity = new Vector3(nuevaVelocidadXZ.x, rb.velocity.y, nuevaVelocidadXZ.z);

        // Rotar el personaje hacia la dirección del movimiento
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

    public bool EstaMoviendose()
    {
        return (inputHorizontal != 0 || inputVertical != 0);
    }

    public float GetVelocidadActual()
    {
        return inputCorrer ? velocidadCorrer : velocidadCaminar;
    }

    public string GetEstadoMovimiento()
    {
        string estado = "Quieto";
        if (EstaMoviendose())
        {
            estado = inputCorrer ? "Corriendo" : "Caminando";
        }
        return estado;
    }

    /* Debug en pantalla
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