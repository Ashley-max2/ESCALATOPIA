using UnityEngine;
using TMPro;

public class AgarrarLanzarSoltar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform followPoint;

    [Header("Configuraci�n")]
    [SerializeField] private float distanciaMax = 3f;
    [SerializeField] private LayerMask capaAgarrable;
    [SerializeField] private float fuerzaLanzamiento = 5f; // fuerza reducida

    [Header("UI (Opcional)")]
    [SerializeField] private TMP_Text interactPromptText;
    [SerializeField] private bool useCurrentTextAsTemplate = true;
    [SerializeField] private string interactPromptTemplate = "Pulsa [E] para agarrar/soltar";

    [Header("Input")]
    [SerializeField] private PlayerInputHandler inputHandler;

    private GameObject objetoActual;
    private Rigidbody rbActual;
    private string promptTemplateResolved;
    private string lastPromptRendered;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();

        promptTemplateResolved = interactPromptTemplate;
        if (useCurrentTextAsTemplate && interactPromptText != null && !string.IsNullOrEmpty(interactPromptText.text))
            promptTemplateResolved = interactPromptText.text;

        RefreshPromptLabel();
    }

    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward * distanciaMax, Color.red);

        RefreshPromptLabel();

        // Interactuar = agarrar / soltar
        if (InteractPressed())
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

    private void RefreshPromptLabel()
    {
        if (interactPromptText == null)
            return;

        string keyLabel = GetInteractKeyLabel();
        string rendered = promptTemplateResolved
            .Replace("[E]", "[" + keyLabel + "]")
            .Replace("{INTERACT}", "[" + keyLabel + "]");

        if (rendered != lastPromptRendered)
        {
            interactPromptText.text = rendered;
            lastPromptRendered = rendered;
        }
    }

    private bool InteractPressed()
    {
        if (inputHandler != null)
        {
            KeyCode interactKey = inputHandler.GetActionKey(InteractInput.ActionName);
            if (interactKey != KeyCode.None)
                return Input.GetKeyDown(interactKey);
        }

        return InteractInput.PressedThisFrame();
    }

    private string GetInteractKeyLabel()
    {
        if (inputHandler != null)
        {
            string label = inputHandler.GetDisplayName(InteractInput.ActionName);
            if (!string.IsNullOrEmpty(label))
                return label;
        }

        return InteractInput.GetDisplayKey();
    }
}
