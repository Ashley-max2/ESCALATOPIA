using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Sincroniza un TMP_Text con el nombre visible de una accion rebindeable.
/// Útil para HUDs que muestran una tecla concreta, por ejemplo el gancho.
/// </summary>
public class ActionBindingTextSync : MonoBehaviour
{
    private static readonly Regex BracketTokenRegex = new Regex(@"\[[^\]]+\]", RegexOptions.Compiled);

    [SerializeField] private TMP_Text targetText;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private string actionName = "CambiarGancho";
    [SerializeField] private bool useCurrentTextAsTemplate = true;
    [SerializeField] private string templateText = "Pulsa [R]";
    [SerializeField] private bool replaceAnyBracketToken = true;

    private string lastRendered;
    private string resolvedTemplate;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();

        resolvedTemplate = templateText;

        if (useCurrentTextAsTemplate && targetText != null && !string.IsNullOrEmpty(targetText.text))
            resolvedTemplate = targetText.text;

        if (inputHandler == null)
            inputHandler = FindObjectOfType<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        if (inputHandler == null)
            inputHandler = FindObjectOfType<PlayerInputHandler>();

        if (inputHandler != null)
            inputHandler.OnBindingsChanged.AddListener(RefreshNow);

        RefreshNow();
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnBindingsChanged.RemoveListener(RefreshNow);
    }

    private void Update()
    {
        RefreshNow();
    }

    public void RefreshNow()
    {
        if (targetText == null)
            return;

        if (inputHandler == null)
            inputHandler = FindObjectOfType<PlayerInputHandler>();

        string rendered = BuildRenderedText();

        if (rendered != lastRendered)
        {
            targetText.text = rendered;
            lastRendered = rendered;
        }
    }

    private string BuildRenderedText()
    {
        if (inputHandler == null)
            return string.IsNullOrEmpty(resolvedTemplate) ? string.Empty : resolvedTemplate;

        string keyLabel = inputHandler.GetDisplayName(actionName);
        string baseText = resolvedTemplate;
        string trimmedText = baseText.Trim();

        if (string.IsNullOrEmpty(baseText))
            return keyLabel;

        string exactToken = $"[{actionName}]";
        if (baseText.Contains(exactToken))
            return baseText.Replace(exactToken, keyLabel);

        if (trimmedText.Length == 1 && !char.IsWhiteSpace(trimmedText[0]))
            return keyLabel;

        if (!replaceAnyBracketToken)
            return baseText;

        // Fallback: si el texto tiene [R] u otro token, lo sustituimos por la tecla actual.
        return BracketTokenRegex.Replace(baseText, keyLabel);
    }
}
