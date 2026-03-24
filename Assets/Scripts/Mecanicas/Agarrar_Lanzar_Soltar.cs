using UnityEngine;
using System.Collections;

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

    // Lanza el objeto y programa su destrucción diferida
    void LanzarObjeto()
    {
        if (rbActual == null) return;

        objetoActual.transform.SetParent(null);

        rbActual.useGravity = true;
        rbActual.freezeRotation = false;

        rbActual.AddForce(cam.transform.forward * fuerzaLanzamiento, ForceMode.Impulse);

        // Iniciar proceso de destrucción con delay
        StartCoroutine(DestruirConHijos(objetoActual, 1f));

        objetoActual = null;
        rbActual = null;
    }

    // Suelta el objeto sin lanzar
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

    // Corrutina:
    // Espera X segundos -> desparenta hijos -> destruye el objeto
    IEnumerator DestruirConHijos(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
        {
            DesparentarHijos(obj);
            Destroy(obj);
        }
    }

    // Desparenta todos los hijos para que no se destruyan
    void DesparentarHijos(GameObject padre)
    {
        Transform[] hijos = new Transform[padre.transform.childCount];

        for (int i = 0; i < hijos.Length; i++)
        {
            hijos[i] = padre.transform.GetChild(i);
        }

        foreach (Transform hijo in hijos)
        {
            hijo.SetParent(null);
        }
    }
}