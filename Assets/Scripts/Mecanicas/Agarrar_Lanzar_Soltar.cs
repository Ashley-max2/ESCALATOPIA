using UnityEngine;

public class AgarrarLanzarSoltar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform followPoint;

    [Header("Configuración")]
    [SerializeField] private float distanciaMax = 3f;
    [SerializeField] private LayerMask capaAgarrable;
    [SerializeField] private float fuerzaLanzamiento = 10f;

    private GameObject objetoActual;
    private Rigidbody rbActual;

    // Se ejecuta al iniciar
    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    // Input + mantener objeto en mano
    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward * distanciaMax, Color.red);

        // R = agarrar / soltar
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (objetoActual == null)
                IntentarAgarrar();
            else
                SoltarObjeto();
        }

        // Click izquierdo = lanzar
        if (Input.GetMouseButtonDown(0) && objetoActual != null)
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

    // Detecta objeto con raycast
    void IntentarAgarrar()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMax, capaAgarrable))
        {
            AgarrarObjeto(hit.collider.gameObject);
        }
    }

    // Agarra el objeto
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

    // Lanza el objeto
    void LanzarObjeto()
    {
        if (rbActual == null) return;

        objetoActual.transform.SetParent(null);

        rbActual.useGravity = true;
        rbActual.freezeRotation = false;

        rbActual.AddForce(cam.transform.forward * fuerzaLanzamiento, ForceMode.Impulse);

        // Marcar el objeto como lanzado para que se destruya al tocar el suelo
        DetectarColisionSuelo scriptColision = objetoActual.GetComponent<DetectarColisionSuelo>();
        if (scriptColision != null)
        {
            scriptColision.MarcarComoLanzado();
        }

        objetoActual = null;
        rbActual = null;
    }

    // Suelta el objeto sin lanzarlo
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