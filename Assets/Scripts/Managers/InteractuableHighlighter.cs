using UnityEngine;

public class InteractuableHighlighter : MonoBehaviour
{
    [Header("Resalte de Interactuables")]
    public Color highlightColor = new Color(1f, 0.88f, 0.15f);
    public float emissionIntensity = 2f;

    private void Start()
    {
        ApplyHighlightToInteractuables();
    }

    private void ApplyHighlightToInteractuables()
    {
        // Buscar todos los GameObjects con el tag "Interactuable"
        GameObject[] interactuables = GameObject.FindGameObjectsWithTag("Interactuable");

        foreach (GameObject obj in interactuables)
        {
            // Obtener todos los Renderers del objeto y sus hijos
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                // Aplicar el color a todos los materiales del renderer
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", highlightColor * emissionIntensity);
                    }
                }
            }

            Debug.Log($"Resalte aplicado a: {obj.name}");
        }

        Debug.Log($"Se aplicó resalte a {interactuables.Length} objetos Interactuables");
    }
}
