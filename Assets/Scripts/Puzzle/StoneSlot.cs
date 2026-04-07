using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class StoneSlot : MonoBehaviour
{
    [Header("Símbolo requerido")]
    public string requiredSymbol;
    [Header("Posición de colocación")]
    public Transform placePoint;
    [Header("Resaltar al mirar")]
    public Renderer[] highlightRenderers;
    public Color highlightColor = new Color(1f, 0.88f, 0.15f);
    public float emissionIntensity = 2f;
    [Header("Animación")]
    public float placeDuration = 0.4f;

    [Header("Audio")]
    [EventRef] public string placeSound = "event:/SFX/Buttons";

    private bool isPlaced = false;
    private Material[] highlightMaterials;
    private Color[] originalEmissionColors;
    private bool[] originalEmissionEnabled;

    private void Reset()
    {
        if (placePoint == null)
            placePoint = transform;
    }

    private void Awake()
    {
        if (placePoint == null)
            placePoint = transform;

        if (highlightRenderers == null || highlightRenderers.Length == 0)
            highlightRenderers = GetComponentsInChildren<Renderer>();

        CacheHighlightMaterials();
        SetHighlighted(false);
    }

    private void CacheHighlightMaterials()
    {
        var materialsList = new System.Collections.Generic.List<Material>();

        foreach (var renderer in highlightRenderers)
        {
            if (renderer == null)
                continue;

            materialsList.AddRange(renderer.materials);
        }

        highlightMaterials = materialsList.ToArray();
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

    public bool CanPlaceSword => !isPlaced;

    public void SetHighlighted(bool highlight)
    {
        if (isPlaced)
            highlight = false;

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

    public bool PlaceSword(SwordPickup sword)
    {
        if (isPlaced || sword == null)
            return false;

        if (sword.Symbol != requiredSymbol)
        {
            Debug.Log($"Espada incorrecta: se requiere '{requiredSymbol}' pero se intentó usar '{sword.Symbol}'.");
            return false;
        }

        GameObject swordObject = sword.gameObject;
        swordObject.transform.SetParent(null);

        Collider collider = swordObject.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody rigidbody = swordObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        swordObject.transform.DOMove(placePoint.position, placeDuration);
        swordObject.transform.DORotateQuaternion(placePoint.rotation, placeDuration).OnComplete(() =>
        {
            swordObject.transform.SetParent(placePoint);
            swordObject.transform.localPosition = Vector3.zero;
            swordObject.transform.localRotation = Quaternion.identity;
        });

        RuntimeManager.PlayOneShot(placeSound, transform.position);
        sword.DisablePickup();
        isPlaced = true;
        SetHighlighted(false);
        Debug.Log($"Espada colocada correctamente en piedra '{requiredSymbol}'.");
        return true;
    }
}
