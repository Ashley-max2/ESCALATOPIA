using UnityEngine;

public class AgarrarLanzarSoltar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform followPoint;

    [Header("Configuraci�n")]
    [SerializeField] private float distanciaMax = 3f;
    [SerializeField] private LayerMask capaAgarrable;
    [SerializeField] private float fuerzaLanzamiento = 5f; // fuerza reducida
    [SerializeField] private float smoothSpeed = 20f; // Velocidad de suavizado del rayo (aumentada para más suavidad)

    private GameObject objetoActual;
    private Rigidbody rbActual;
    private Vector3 smoothedForward;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        smoothedForward = cam.transform.forward;
    }

    void Update()
    {
        // Suavizar la dirección del rayo usando interpolación esférica para direcciones
        smoothedForward = Vector3.Slerp(smoothedForward, cam.transform.forward, Time.deltaTime * smoothSpeed);
        Debug.DrawRay(cam.transform.position, smoothedForward * distanciaMax, Color.red);

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
            objetoActual.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
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

            // rotaci�n fija correcta
            obj.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            // bloquear rotaci�n
            rbActual.constraints = RigidbodyConstraints.FreezeRotation;
        }

        obj.transform.SetParent(followPoint);
    }

    void LanzarObjeto()
    {
        if (rbActual == null) return;

        objetoActual.transform.SetParent(null);

        rbActual.useGravity = true;

        // desbloquear f�sica completa
        rbActual.constraints = RigidbodyConstraints.None;

        rbActual.freezeRotation = false;

        // lanzamiento en arco (45�)
        Vector3 direccion =
            (cam.transform.forward + Vector3.up).normalized;

        rbActual.AddForce(
            direccion * fuerzaLanzamiento * 1.7f,
            ForceMode.Impulse
        );

        // activar l�gica del barril
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
            rbActual.constraints = RigidbodyConstraints.None;
        }

        objetoActual = null;
        rbActual = null;
    }
}