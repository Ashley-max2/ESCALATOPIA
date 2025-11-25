using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float velocidadEscalada = 3f;
    public float rotacionSuavidad = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float radioCheckSuelo = 0.3f;
    public LayerMask suelo;
    public Transform puntoCheckSuelo;

    [Header("Escalada")]
    public float distanciaDeteccionPared = 1.5f;
    public Vector3 offsetTriggerEscalada = new Vector3(0, 0.5f, 0);

    [HideInInspector] public float inputH;
    [HideInInspector] public float inputV;
    [HideInInspector] public bool inputCorrer;
    [HideInInspector] public bool inputSalto;
    [HideInInspector] public bool inputEscalar;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform cam;

    private IState estadoActual;
    private bool enZonaEscalada = false;
    private SphereCollider triggerEscalada; // Referencia al collider trigger

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;

        // Configurar punto check suelo (tu código original)
        if (puntoCheckSuelo == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.9f, 0);
            puntoCheckSuelo = gc.transform;
        }

        // Crear y configurar trigger de escalada
        ConfigurarTriggerEscalada();
    }

    private void Start()
    {
        CambiarEstado(new IdleState());
    }

    private void Update()
    {
        LeerInputs();
        estadoActual.Update(this);
    }

    private void ConfigurarTriggerEscalada()
    {
        // Crear GameObject para el trigger
        GameObject triggerObj = new GameObject("ClimbingTrigger");
        triggerObj.transform.SetParent(transform);
        triggerObj.transform.localPosition = offsetTriggerEscalada;
        triggerObj.tag = "Player"; // O el tag que uses para el jugador

        // Añadir collider esférico como trigger
        triggerEscalada = triggerObj.AddComponent<SphereCollider>();
        triggerEscalada.isTrigger = true;
        triggerEscalada.radius = 0.8f; // Radio ajustable

        // Añadir rigidbody (requerido para triggers)
        Rigidbody triggerRb = triggerObj.AddComponent<Rigidbody>();
        triggerRb.isKinematic = true;
        triggerRb.useGravity = false;
    }

    // Métodos para manejar triggers de escalada
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            EntrarZonaEscalada();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("escalable"))
        {
            SalirZonaEscalada();
        }
    }

    void LeerInputs()
    {
        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");
        inputCorrer = Input.GetKey(KeyCode.LeftShift);
        inputSalto = Input.GetButtonDown("Jump");
        inputEscalar = Input.GetKey(KeyCode.E);
    }

    public bool EstaEnSuelo()
    {
        bool enSuelo = Physics.CheckSphere(puntoCheckSuelo.position, radioCheckSuelo, suelo);

        // Debug visual
        Debug.DrawRay(puntoCheckSuelo.position, Vector3.down * radioCheckSuelo,
                     enSuelo ? Color.green : Color.red);

        return enSuelo;
    }

    public void CambiarEstado(IState nuevo)
    {
        if (estadoActual != null)
            estadoActual.Exit(this);

        estadoActual = nuevo;
        estadoActual.Enter(this);
    }

    // MÉTODOS PARA ESCALADA
    public void EntrarZonaEscalada()
    {
        enZonaEscalada = true;
        Debug.Log("Entró en zona escalable");
    }

    public void SalirZonaEscalada()
    {
        enZonaEscalada = false;
        Debug.Log("Salió de zona escalable");

        // Si está escalando, forzar salida
        if (estadoActual is ClimbingState)
        {
            CambiarEstado(new IdleState());
        }
    }

    public bool PuedeIniciarEscalada()
    {
        return enZonaEscalada && inputEscalar &&
               GetComponent<ResistenceController>().TieneResistencia(1f);
    }

    // Para debug visual
    private void OnDrawGizmosSelected()
    {
        // Dibujar trigger de escalada
        if (triggerEscalada != null)
        {
            Gizmos.color = enZonaEscalada ? Color.cyan : Color.blue;
            Gizmos.DrawWireSphere(triggerEscalada.transform.position, triggerEscalada.radius);
        }

        // Dibujar check de suelo (tu código original)
        if (puntoCheckSuelo != null)
        {
            Gizmos.color = EstaEnSuelo() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(puntoCheckSuelo.position, radioCheckSuelo);
        }
    }
}