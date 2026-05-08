using UnityEngine;




public class AgarrarLanzarSoltar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform followPoint;

    [Header("Configuracion")]
    [SerializeField] private float distanciaMax = 3f;
    [SerializeField] private LayerMask capaAgarrable;
    [SerializeField] private float fuerzaLanzamiento = 10f;
    
    public void SetDistanciaMax(float val) => distanciaMax = val;
    public void SetFuerzaLanzamiento(float val) => fuerzaLanzamiento = val;

    private GameObject objetoActual;
    private Rigidbody rbActual;
    private PlayerInputHandler _input;

    /// <summary>
    /// True si el player tiene un objeto agarrado
    /// </summary>
    public bool TieneObjeto => objetoActual != null;

    /// <summary>
    /// Comprueba si un GameObject especifico es el que esta agarrado
    /// </summary>
    public bool EsObjetoAgarrado(GameObject obj)
    {
        return objetoActual != null && objetoActual == obj;
    }

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        _input = GetComponentInParent<PlayerInputHandler>();
    }

    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward * distanciaMax, Color.red);

        // InteractPressed = agarrar / soltar
        if (_input != null && _input.InteractPressed)
        {
            if (objetoActual == null)
                IntentarAgarrar();
            else
                SoltarObjeto();
        }

        // HookReleasePressed = lanzar (as an alternative to mouse)
        if (_input != null && _input.HookReleasePressed && objetoActual != null)
        {
            LanzarObjeto();
        }

        // Mantener objeto en la mano
        if (objetoActual != null)
        {
            objetoActual.transform.position = followPoint.position;
            objetoActual.transform.rotation = Quaternion.identity;
        }
    }

    void IntentarAgarrar()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMax, capaAgarrable))
        {
            AgarrarObjeto(hit.collider.gameObject);
        }
    }

    void AgarrarObjeto(GameObject obj)
    {
        objetoActual = obj;
        rbActual = obj.GetComponent<Rigidbody>();

        if (rbActual != null)
        {
            rbActual.useGravity = false;
            rbActual.velocity = Vector3.zero;
            rbActual.angularVelocity = Vector3.zero;
            rbActual.freezeRotation = true;
        }

        obj.transform.SetParent(followPoint);
    }

    void LanzarObjeto()
    {
        if (rbActual == null) return;

        objetoActual.transform.SetParent(null);

        rbActual.useGravity = true;
        rbActual.freezeRotation = false;

        rbActual.AddForce(cam.transform.forward * fuerzaLanzamiento, ForceMode.Impulse);

        // BUSCAR ThrownBox (en objeto, hijo o padre)
        ThrownBox thrown = objetoActual.GetComponent<ThrownBox>();

        if (thrown == null)
            thrown = objetoActual.GetComponentInChildren<ThrownBox>();

        if (thrown == null)
            thrown = objetoActual.GetComponentInParent<ThrownBox>();

        if (thrown != null)
        {
            thrown.MarcarComoLanzado();
        }

        objetoActual = null;
        rbActual = null;
    }

    void SoltarObjeto()
    {
        if (objetoActual == null) return;

        objetoActual.transform.SetParent(null);

        if (rbActual != null)
        {
            rbActual.useGravity = true;
            rbActual.freezeRotation = false;
        }

        objetoActual = null;
        rbActual = null;
    }
}