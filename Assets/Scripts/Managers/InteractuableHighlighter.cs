using System.Collections.Generic;
using UnityEngine;

public class InteractuableHighlighter : MonoBehaviour
{
    [SerializeField] private List<string> interactuableTags;
    [SerializeField] private Color highlightColor; 
    [SerializeField] private float emissionIntensity = 1.5f; 
    [SerializeField] private float maxRayDistance = 10f; // Distancia máxima del raycast
    [SerializeField] private Camera playerCamera; // Cámara del jugador

    private GameObject lastHighlightedObject;

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
                HighlightObject(hitObject);
                lastHighlightedObject = hitObject;
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
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * emissionIntensity);
                }
            }
        }
    }

    private void RemoveHighlight()
    {
        if (lastHighlightedObject != null)
        {
            Renderer[] renderers = lastHighlightedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor("_EmissionColor", Color.black); // Restablecer el color de emisión
                    }
                }
            }
            lastHighlightedObject = null;
        }
    }
}