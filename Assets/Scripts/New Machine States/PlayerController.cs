using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float rotacionSuavidad = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float radioCheckSuelo = 0.3f;
    public LayerMask suelo;
    public Transform puntoCheckSuelo;

    [HideInInspector] public float inputH;
    [HideInInspector] public float inputV;
    [HideInInspector] public bool inputCorrer;
    [HideInInspector] public bool inputSalto;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform cam;

    private IState estadoActual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;

        if (puntoCheckSuelo == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.9f, 0);
            puntoCheckSuelo = gc.transform;
        }
    }

    private void Start()
    {
        CambiarEstado(new IdleState());
    }

    private void Update()
    {
        LeerInputs();
        estadoActual.Update(this);

        // Debug en pantalla
        Debug.Log($"En suelo: {EstaEnSuelo()} | Velocidad Y: {rb.velocity.y}");
    }

    public void CambiarEstado(IState nuevo)
    {
        if (estadoActual != null)
            estadoActual.Exit(this);

        estadoActual = nuevo;
        estadoActual.Enter(this);
    }

    void LeerInputs()
    {
        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");
        inputCorrer = Input.GetKey(KeyCode.LeftShift);
        inputSalto = Input.GetButtonDown("Jump");
    }

    public bool EstaEnSuelo()
    {
        return Physics.CheckSphere(puntoCheckSuelo.position, radioCheckSuelo, suelo);
    }

    private void OnDrawGizmos()
    {
        if (puntoCheckSuelo != null)
        {
            Gizmos.color = EstaEnSuelo() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(puntoCheckSuelo.position, radioCheckSuelo);
        }
    }
}
