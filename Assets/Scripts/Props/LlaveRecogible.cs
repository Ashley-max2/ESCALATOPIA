using UnityEngine;

public class LlaveRecogible : MonoBehaviour
{
    [SerializeField] private ParticleSystem particulasLuz;

    private Renderer[] renderers;
    private Collider miCollider;

    private void Awake()
    {
        // Guardamos todos los renderers de la llave
        renderers = GetComponentsInChildren<Renderer>();

        // Collider trigger de la llave
        miCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si entra el jugador
        if (other.CompareTag("Player"))
        {
            // Hacer invisible la llave
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }

            // Desactivar el trigger para no repetir
            if (miCollider != null)
            {
                miCollider.enabled = false;
            }

            // Destruir partículas
            if (particulasLuz != null)
            {
                Destroy(particulasLuz.gameObject);
            }

            Debug.Log("Llave recogida");
        }
    }
}