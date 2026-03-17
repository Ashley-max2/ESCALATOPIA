using UnityEngine;

public class Agarrar_Lanzar_Soltar : MonoBehaviour
{
    [Header("Referencias")]
    public Camera cam;              // Cámara del jugador (arrastrar en inspector)
    public Transform followPoint;  // Punto donde se coloca el objeto

    [Header("Configuración")]
    public float distanciaMax = 3f;
    public string tagAgarrable = "Objeto";

    private GameObject objetoActual;

    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.forward * distanciaMax, Color.red);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (objetoActual == null)
                IntentarAgarrar();
            else
                SoltarObjeto();
        }
    }

    void IntentarAgarrar()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distanciaMax))
        {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag(tagAgarrable))
            {
                objetoActual = obj;
                AgarrarObjeto(obj);
            }
        }
    }

    void AgarrarObjeto(GameObject obj)
    {
        obj.transform.SetParent(followPoint);
        obj.transform.localPosition = Vector3.zero;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void SoltarObjeto()
    {
        objetoActual.transform.SetParent(null);

        Rigidbody rb = objetoActual.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        objetoActual = null;
    }
}