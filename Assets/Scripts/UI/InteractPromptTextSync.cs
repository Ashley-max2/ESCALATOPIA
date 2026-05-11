using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Sincroniza un TMP_Text que tenga [E] o {INTERACT}
/// con la tecla actual de interacción rebindeada.
///
/// Uso típico: texto fijo en prefab del panel de subtítulos
/// (por ejemplo "Pulsa [E] para continuar").
/// </summary>
public class InteractPromptTextSync : MonoBehaviour
{
    private static readonly Regex BracketTokenRegex = new Regex(@"\[[^\]]+\]", RegexOptions.Compiled);

    [SerializeField] private TMP_Text targetText;
    [SerializeField] private bool useCurrentTextAsTemplate = true;
    [SerializeField] private string templateText = "Pulsa [E] para continuar";
    [SerializeField] private bool replaceAnyBracketToken = true;

    private string resolvedTemplate;
    private string lastRendered;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        resolvedTemplate = templateText;

        if (useCurrentTextAsTemplate && targetText != null && !string.IsNullOrEmpty(targetText.text))
            resolvedTemplate = targetText.text;
    }

    private void OnEnable()
    {
        RefreshNow();
    }

    private void Update()
    {
        // Actualiza en runtime por si cambia el rebind o el esquema de entrada.
        RefreshNow();
    }

    public void RefreshNow()
    {
        if (targetText == null)
            return;

        string rendered = BuildRenderedText();
        if (rendered != lastRendered)
        {
            targetText.text = rendered;
            lastRendered = rendered;
        }
    }

    private string BuildRenderedText()
    {
        string key = InteractInput.GetBracketedDisplayKey();
        string baseText = resolvedTemplate;

        string replaced = InteractInput.ReplaceInteractPlaceholder(baseText);
        if (replaced != baseText)
            return replaced;

        if (!replaceAnyBracketToken)
            return replaced;

        if (string.IsNullOrEmpty(baseText))
            return baseText;

        // Fallback robusto: si no hay [E] explícito, sustituye cualquier [algo] por la tecla actual.
        return BracketTokenRegex.Replace(baseText, key);
    }
}
