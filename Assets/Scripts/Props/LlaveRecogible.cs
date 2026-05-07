using UnityEngine;

public class LlaveRecogible : MonoBehaviour
{
    [SerializeField] private string idLlave;

    [SerializeField] private ParticleSystem particulasLuz;

    private Renderer[] renderers;
    private Collider miCollider;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        miCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Guardar en inventario
            if (InventarioLlaves.instancia != null)
            {
                InventarioLlaves.instancia.AgregarLlave(idLlave);
            }

            // Desaparecer visualmente
            foreach (Renderer r in renderers)
                r.enabled = false;

            if (miCollider != null)
                miCollider.enabled = false;

            if (particulasLuz != null)
                Destroy(particulasLuz.gameObject);

            Debug.Log("Llave recogida: " + idLlave);
        }
    }
}