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
    public float tiempoReentradaEscalada = 0.5f;

    [Header("Wall Jump")]
    public float fuerzaWallJump = 12f;
    public float fuerzaWallJumpLateral = 8f;
    public float tiempoBloqueoWallJump = 0.15f;

    [Header("Sonido salto")]
    public AudioClip sonidoSalto;
    private AudioSource audioSource;

    [HideInInspector] public float inputH;
    [HideInInspector] public float inputV;
    [HideInInspector] public bool inputCorrer;
    [HideInInspector] public bool inputSalto;
    [HideInInspector] public bool inputEscalar;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform cam;

    // CAMBIO: Cambiar de private a public o añadir propiedad pública
    [HideInInspector] public bool enZonaEscalada = false;

    // Referencia al sistema de gancho
    private HookSystem hookSystem;

    private IState estadoActual;
    private SphereCollider triggerEscalada;
    private float temporizadorReentradaEscalada = 0f;
    private bool permitirReentradaEscalada = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        hookSystem = GetComponentInChildren<HookSystem>();

        // Configurar punto check suelo
        if (puntoCheckSuelo == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.9f, 0);
            puntoCheckSuelo = gc.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
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

        // Detectar si el gancho empieza a impulsar y cambiar al estado del gancho
        if (EstaGanchoActivo() && !(estadoActual is HookImpulseState))
        {
            Debug.Log("[PLAYER] Gancho activo detectado - Cambiando a HookImpulseState");
            CambiarEstado(new HookImpulseState());
        }

        // Actualizar temporizador de reentrada de escalada
        if (temporizadorReentradaEscalada > 0f)
        {
            temporizadorReentradaEscalada -= Time.deltaTime;
            permitirReentradaEscalada = false;
        }
        else
        {
            permitirReentradaEscalada = true;
        }

        if (estadoActual != null)
            estadoActual.Update(this);

        // DEBUG TEMPORAL
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResistenceController rc = GetComponent<ResistenceController>();
            if (rc != null)
            {
                rc.DebugEstadoResistencia();
            }
        }
    }

    private void ConfigurarTriggerEscalada()
    {
        // Crear GameObject para el trigger
        GameObject triggerObj = new GameObject("ClimbingTrigger");
        triggerObj.transform.SetParent(transform);
        triggerObj.transform.localPosition = offsetTriggerEscalada;
        triggerObj.tag = "Player";

        // Añadir collider esférico como trigger
        triggerEscalada = triggerObj.AddComponent<SphereCollider>();
        triggerEscalada.isTrigger = true;
        triggerEscalada.radius = 0.8f;

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

    private void LeerInputs()
    {
        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");
        inputCorrer = Input.GetKey(KeyCode.LeftShift);
        inputSalto = Input.GetButtonDown("Jump");
        inputEscalar = Input.GetKey(KeyCode.E);

        // Bloquear TODO movimiento del player mientras el gancho impulse
        if (EstaGanchoActivo())
        {
            inputH = 0f;
            inputV = 0f;
            inputCorrer = false;
            inputSalto = false;   // desactivar salto
            inputEscalar = false; // desactivar escalada
        }
    }

    public bool EstaEnSuelo()
    {
        bool enSuelo = Physics.CheckSphere(puntoCheckSuelo.position, radioCheckSuelo, suelo);

        // Debug visual
        Debug.DrawRay(puntoCheckSuelo.position, Vector3.down * radioCheckSuelo,
                     enSuelo ? Color.green : Color.red);

        return enSuelo;
    }

    public void CambiarEstado(IState nuevoEstado)
    {
        // Bloquear cambios de estado durante impulso, EXCEPTO a HookImpulseState
        if (EstaGanchoActivo() && !(nuevoEstado is HookImpulseState))
        {
            Debug.Log($"[PLAYER] Cambio de estado bloqueado: {nuevoEstado.GetType().Name} durante impulso");
            return;
        }
        
        Debug.Log($"[PLAYER] Cambiando de {GetNombreEstadoActual()} a {nuevoEstado.GetType().Name}");

        if (estadoActual != null)
            estadoActual.Exit(this);

        estadoActual = nuevoEstado;

        if (estadoActual != null)
            estadoActual.Enter(this);
    }

    // Método público para que HookSystem cambie al estado del gancho
    public void EntrarEnEstadoGancho()
    {
        Debug.Log("[PLAYER] Forzando cambio a HookImpulseState");
        CambiarEstado(new HookImpulseState());
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
        if (EstaEnEstado<ClimbingState>())
        {
            CambiarEstado(new IdleState());
        }
    }

    public bool PuedeIniciarEscalada()
    {
        ResistenceController rc = GetComponent<ResistenceController>();
        return enZonaEscalada && inputEscalar && rc != null && rc.TieneResistencia(1f) && permitirReentradaEscalada;
    }

    // NUEVO MÉTODO: Llamado desde WallJumpState para prevenir reentrada inmediata
    public void IniciarTemporizadorReentradaEscalada()
    {
        temporizadorReentradaEscalada = tiempoReentradaEscalada;
        permitirReentradaEscalada = false;
    }

    // MÉTODOS PARA ACCEDER AL ESTADO ACTUAL
    public IState GetEstadoActual()
    {
        return estadoActual;
    }

    public bool EstaEnEstado<T>() where T : IState
    {
        return estadoActual is T;
    }

    public string GetNombreEstadoActual()
    {
        return estadoActual?.GetType().Name ?? "Null";
    }

    // Método para verificar si el gancho está impulsando al player
    // Método para verificar si el gancho está impulsando al player
    public bool EstaGanchoActivo()
    {
        if (hookSystem == null) return false;
        
        // Verificar múltiples condiciones
        bool hookSystemActive = hookSystem.IsHooking;
        bool isImpulsing = hookSystem.HookMovement != null && hookSystem.HookMovement.IsImpulsing;
        bool hasHookPoint = hookSystem.CurrentHookPoint != null;
        
        // Está activo si el sistema de gancho está en cualquier estado excepto Idle
        // Y específicamente si está impulsando
        bool activo = (hookSystemActive && (hookSystem.IsThrown || hookSystem.IsAttached)) || isImpulsing;
        
        if (activo)
        {
            Debug.Log($"[PLAYER] EstaGanchoActivo = true (HookSystem: {hookSystemActive}, Impulsing: {isImpulsing})");
        }
        
        return activo;
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

        // Dibujar check de suelo
        if (puntoCheckSuelo != null)
        {
            Gizmos.color = EstaEnSuelo() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(puntoCheckSuelo.position, radioCheckSuelo);
        }
    }

    // Método para debug en pantalla
    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 30, 300, 20), $"Estado: {GetNombreEstadoActual()}");
            GUI.Label(new Rect(10, 50, 300, 20), $"En suelo: {EstaEnSuelo()}");
            GUI.Label(new Rect(10, 70, 300, 20), $"En zona escalada: {enZonaEscalada}");
            GUI.Label(new Rect(10, 90, 300, 20), $"Puede re-escalar: {permitirReentradaEscalada}");

            ResistenceController rc = GetComponent<ResistenceController>();
            if (rc != null)
            {
                GUI.Label(new Rect(10, 110, 300, 20), $"Resistencia: {rc.GetResistenciaActual():F1}");
            }
        }
    }
}