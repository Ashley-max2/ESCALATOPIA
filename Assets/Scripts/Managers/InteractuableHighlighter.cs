using System.Collections.Generic;
using UnityEngine;

public class InteractuableHighlighter : MonoBehaviour
{
    [SerializeField] private List<string> interactuableTags = new List<string> { "Barril" };
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private float emissionIntensity = 0.3f;
    [SerializeField] private float maxRayDistance = 10f;
    [SerializeField] private Camera playerCamera;

    private GameObject lastHighlightedObject;

    // Per-object material cache: stores materials + original emission state
    private class ObjectHighlightData
    {
        public Material[] materials;
        public Color[] originalEmissionColors;
        public bool[] originalEmissionEnabled;
    }
    private Dictionary<GameObject, ObjectHighlightData> highlightCache = new Dictionary<GameObject, ObjectHighlightData>();

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactuableTags == null)
            interactuableTags = new List<string>();

        if (!interactuableTags.Contains("Barril"))
            interactuableTags.Add("Barril");
    }

    private void Update()
    {
        Ray ray = new Ray(
            playerCamera != null ? playerCamera.transform.position : transform.position,
            playerCamera != null ? playerCamera.transform.forward : transform.forward
        );

        GameObject newTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            // Busca el tag hacia arriba en la jerarquía por si el collider está en un hijo
            Transform t = hit.collider.transform;
            while (t != null)
            {
                if (interactuableTags.Contains(t.gameObject.tag))
                {
                    newTarget = t.gameObject;
                    break;
                }
                t = t.parent;
            }
        }

        if (lastHighlightedObject != newTarget)
        {
            if (lastHighlightedObject != null)
            {
                SetHighlighted(lastHighlightedObject, false);
                lastHighlightedObject.GetComponentInChildren<IHighlightable>()?.OnHighlightEnd();
            }

            lastHighlightedObject = newTarget;

            if (lastHighlightedObject != null)
            {
                SetHighlighted(lastHighlightedObject, true);
                lastHighlightedObject.GetComponentInChildren<IHighlightable>()?.OnHighlightStart();
            }
        }
    }

    private void SetHighlighted(GameObject obj, bool highlight)
    {
        if (!highlightCache.TryGetValue(obj, out ObjectHighlightData data))
        {
            data = BuildCache(obj);
            highlightCache[obj] = data;
        }

        for (int i = 0; i < data.materials.Length; i++)
        {
            var mat = data.materials[i];
            if (mat == null || !mat.HasProperty("_EmissionColor"))
                continue;

            if (highlight)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", highlightColor * emissionIntensity);
            }
            else
            {
                mat.SetColor("_EmissionColor", data.originalEmissionColors[i]);
                if (data.originalEmissionEnabled[i])
                    mat.EnableKeyword("_EMISSION");
                else
                    mat.DisableKeyword("_EMISSION");
            }
        }
    }

    private ObjectHighlightData BuildCache(GameObject obj)
    {
        var matList = new List<Material>();
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null)
                matList.AddRange(renderer.materials);
        }

        var data = new ObjectHighlightData();
        data.materials = matList.ToArray();
        data.originalEmissionColors = new Color[data.materials.Length];
        data.originalEmissionEnabled = new bool[data.materials.Length];

        for (int i = 0; i < data.materials.Length; i++)
        {
            var mat = data.materials[i];
            if (mat.HasProperty("_EmissionColor"))
                data.originalEmissionColors[i] = mat.GetColor("_EmissionColor");
            else
                data.originalEmissionColors[i] = Color.black;

            data.originalEmissionEnabled[i] = mat.IsKeywordEnabled("_EMISSION");
        }

        return data;
    }
}
