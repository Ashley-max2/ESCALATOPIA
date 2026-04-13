using System.Collections.Generic;
using UnityEngine;

public class InteractuableHighlighter : MonoBehaviour
{
    [SerializeField] private List<string> interactuableTags;
    [SerializeField] private Color outlineColor = Color.white; 
    [SerializeField] private float outlineWidth = 0.02f; 
    [SerializeField] private float maxRayDistance = 10f; // Distancia máxima del raycast
    [SerializeField] private Camera playerCamera; // Cámara del jugador

    private GameObject lastHighlightedObject;
    private Dictionary<Renderer, List<Material>> originalMaterials = new Dictionary<Renderer, List<Material>>();

    private void Update()
    {
        ApplyHighlightToInteractuables();
    }

    private void ApplyHighlightToInteractuables()
    {
        // Realizar un raycast desde la cámara del jugador
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Verificar si el objeto tiene un tag válido
            if (interactuableTags.Contains(hitObject.tag))
            {
                // Aplicar el resaltado al objeto
                if (lastHighlightedObject != hitObject)
                {
                    RemoveHighlight();
                    HighlightObject(hitObject);
                    lastHighlightedObject = hitObject;
                }
            }
            else
            {
                // Si el objeto no tiene un tag válido, quitar el resaltado del último objeto
                RemoveHighlight();
            }
        }
        else
        {
            // Si no se detecta ningún objeto, quitar el resaltado del último objeto
            RemoveHighlight();
        }
    }

    private void HighlightObject(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Guardar los materiales originales
            Material[] materials = renderer.materials;
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = new List<Material>(materials);
            }

            // Crear nuevos materiales con efecto de outline
            Material[] newMaterials = new Material[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                Material outlineMat = new Material(materials[i]);
                
                // Establecer propiedades para outline
                if (outlineMat.HasProperty("_OutlineWidth"))
                {
                    outlineMat.SetFloat("_OutlineWidth", outlineWidth);
                }
                if (outlineMat.HasProperty("_OutlineColor"))
                {
                    outlineMat.SetColor("_OutlineColor", outlineColor);
                }

                // Usar emisión blanca para el efecto de resaltado
                if (outlineMat.HasProperty("_EmissionColor"))
                {
                    outlineMat.EnableKeyword("_EMISSION");
                    outlineMat.SetColor("_EmissionColor", Color.white * 0.3f);
                }
                
                newMaterials[i] = outlineMat;
            }
            
            renderer.materials = newMaterials;
        }
    }

    private void RemoveHighlight()
    {
        if (lastHighlightedObject != null)
        {
            Renderer[] renderers = lastHighlightedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // Restaurar los materiales originales
                if (originalMaterials.ContainsKey(renderer))
                {
                    Material[] originalMats = originalMaterials[renderer].ToArray();
                    renderer.materials = originalMats;
                    originalMaterials.Remove(renderer);
                }
                else
                {
                    // Si no tenemos los originales, simplemente quitar emisión
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
            lastHighlightedObject = null;
        }
    }
}