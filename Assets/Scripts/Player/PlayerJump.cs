using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Salto")]
    public float fuerzaSalto = 12f; // Aumentada para mejor respuesta
    public float checkDistanciaSuelo = 0.15f;
    public float radioCheckSuelo = 0.4f;
    public LayerMask capaSuelo;
    public Transform puntoCheckSuelo;

    // Componentes
    private Rigidbody rb;

    // Estado
    private bool estaEnSuelo;
    private bool inputSalto;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("No se encontró Rigidbody en el Player!");
        }

        // Crear punto de check automáticamente si no existe
        if (puntoCheckSuelo == null)
        {
            CrearPuntoCheckSuelo();
        }
    }

    void CrearPuntoCheckSuelo()
    {
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(transform);
        groundCheck.transform.localPosition = new Vector3(0, -0.9f, 0);
        puntoCheckSuelo = groundCheck.transform;
    }

    void Update()
    {
        ObtenerInputSalto();
        VerificarSuelo();

        if (inputSalto && estaEnSuelo && rb != null)
        {
            Saltar();
        }
    }

    void ObtenerInputSalto()
    {
        inputSalto = Input.GetButtonDown("Jump");
    }

    void VerificarSuelo()
    {
        if (puntoCheckSuelo == null) return;

        // Múltiples métodos de detección para mayor precisión
        Vector3 origen = puntoCheckSuelo.position;

        // Método 1: SphereCast
        RaycastHit hit;
        bool sphereCastHit = Physics.SphereCast(origen, radioCheckSuelo, Vector3.down, out hit,
                                               checkDistanciaSuelo, capaSuelo);

        // Método 2: CheckSphere como respaldo
        bool checkSphereHit = Physics.CheckSphere(origen, radioCheckSuelo, capaSuelo);

        estaEnSuelo = sphereCastHit || checkSphereHit;

        if (estaEnSuelo)
        {
            Debug.Log("tocando el suelo");
        }

        // Debug visual en tiempo de juego
        Debug.DrawRay(origen, Vector3.down * (checkDistanciaSuelo + radioCheckSuelo),
                     estaEnSuelo ? Color.green : Color.red);
    }

    void Saltar()
    {
        // Consumir resistencia al saltar
        ResistenceController resistencia = GetComponent<ResistenceController>();
        if (resistencia != null)
        {
            //Last Jump
            if (!resistencia.TieneResistencia(1f))
            {
                Debug.Log("No hay suficiente resistencia para saltar.");
                return;
            }

            // Consumir resistencia antes de saltar
            resistencia.ConsumirResistencia(5f);
        }

        // Resetear velocidad Y para salto consistente
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Aplicar fuerza de salto (VelocityChange para respuesta inmediata)
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.VelocityChange);

        

        Debug.Log("¡SALTANDO! Fuerza: " + fuerzaSalto);
    }

    // Métodos públicos para que otros scripts consulten el estado
    public bool EstaEnSuelo()
    {
        return estaEnSuelo;
    }

    public bool EstaSaltando()
    {
        return !estaEnSuelo && rb.velocity.y > 0.1f;
    }

    public bool EstaCayendo()
    {
        return !estaEnSuelo && rb.velocity.y < -0.1f;
    }

    // Método para obtener información de estado para UI
    public string GetEstadoSalto()
    {
        if (EstaSaltando()) return "Saltando ↑";
        if (EstaCayendo()) return "Cayendo ↓";
        if (estaEnSuelo) return "En suelo";
        return "En aire";
    }

    // Método para debug visual en el Editor
    void OnDrawGizmosSelected()
    {
        if (puntoCheckSuelo == null) return;

        Gizmos.color = estaEnSuelo ? Color.green : Color.red;
        Vector3 origen = puntoCheckSuelo.position;
        Vector3 destino = origen + Vector3.down * (checkDistanciaSuelo + radioCheckSuelo);

        Gizmos.DrawWireSphere(origen, radioCheckSuelo);
        Gizmos.DrawLine(origen, destino);
        Gizmos.DrawWireSphere(destino, 0.1f);
    }

    // Debug en pantalla
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string estado = $"SALTO: {GetEstadoSalto()}\n";
        estado += $"Velocidad Y: {rb.velocity.y:F2}";

        GUI.Label(new Rect(10, 60, 300, 100), estado, style);
    }
}