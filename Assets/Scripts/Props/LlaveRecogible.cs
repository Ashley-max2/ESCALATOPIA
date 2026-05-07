using UnityEngine;

public class LlaveRecogible : MonoBehaviour
{
    public static bool llaveRecogida = false;

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
            llaveRecogida = true;

            foreach (Renderer r in renderers)
                r.enabled = false;

            if (miCollider != null)
                miCollider.enabled = false;

            if (particulasLuz != null)
                Destroy(particulasLuz.gameObject);

            Debug.Log("Llave recogida");
        }
    }
}