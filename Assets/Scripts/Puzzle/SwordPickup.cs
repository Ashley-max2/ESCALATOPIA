using UnityEngine;
using FMODUnity;

public class SwordPickup : MonoBehaviour
{
    [Header("Símbolo")] 
    public string swordSymbol;

    [Header("Resaltar al mirar")]
    public Renderer[] highlightRenderers;
    public Color highlightColor = new Color(1f, 0.88f, 0.15f);
    public float emissionIntensity = 2f;

    [Header("Audio")]
    [EventRef] public string pickupSound = "event:/SFX/PickUp";

    private SwordPuzzleManager puzzleManager;
    private bool isPicked;
    private bool isHighlighted;

    private Material[] highlightMaterials;
    private Color[] originalEmissionColors;
    private bool[] originalEmissionEnabled;

    private void Start()
    {
        if (puzzleManager == null)
            puzzleManager = SwordPuzzleManager.Instance;

        if (highlightRenderers == null || highlightRenderers.Length == 0)
            highlightRenderers = GetComponentsInChildren<Renderer>();

        CacheHighlightMaterials();
        SetHighlighted(false);
    }

    private void CacheHighlightMaterials()
    {
        var materials = new System.Collections.Generic.List<Material>();

        foreach (var renderer in highlightRenderers)
        {
            if (renderer == null)
                continue;

            materials.AddRange(renderer.materials);
        }

        highlightMaterials = materials.ToArray();
        originalEmissionColors = new Color[highlightMaterials.Length];
        originalEmissionEnabled = new bool[highlightMaterials.Length];

        for (int i = 0; i < highlightMaterials.Length; i++)
        {
            var mat = highlightMaterials[i];
            if (mat.HasProperty("_EmissionColor"))
                originalEmissionColors[i] = mat.GetColor("_EmissionColor");
            else
                originalEmissionColors[i] = Color.black;

            originalEmissionEnabled[i] = mat.IsKeywordEnabled("_EMISSION");
        }
    }

    private void Update()
    {
        if (isHighlighted && Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    private void TryPickup()
    {
        if (!isHighlighted)
            return;

        if (puzzleManager == null)
        {
            Debug.LogWarning("SwordPickup: no se encontró SwordPuzzleManager.");
            return;
        }

        if (puzzleManager.PickupSword(this))
        {
            Debug.Log($"Espada recogida: {swordSymbol}");
        }
        else
        {
            Debug.Log("Ya tienes una espada en la mano.");
        }
    }

    public bool IsPickable => !isPicked;

    public void SetHighlighted(bool highlight)
    {
        if (isPicked)
            highlight = false;

        if (isHighlighted == highlight)
            return;

        isHighlighted = highlight;

        for (int i = 0; i < highlightMaterials.Length; i++)
        {
            var mat = highlightMaterials[i];
            if (mat == null || !mat.HasProperty("_EmissionColor"))
                continue;

            if (highlight)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", highlightColor * emissionIntensity);
            }
            else
            {
                mat.SetColor("_EmissionColor", originalEmissionColors[i]);
                if (originalEmissionEnabled[i])
                    mat.EnableKeyword("_EMISSION");
                else
                    mat.DisableKeyword("_EMISSION");
            }
        }
    }

    public void PickUp(Transform holdPoint)
    {
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        RuntimeManager.PlayOneShot(pickupSound, transform.position);
        isPicked = true;
        SetHighlighted(false);
    }

    public void DisablePickup()
    {
        enabled = false;
    }

    public string Symbol => swordSymbol;
}

