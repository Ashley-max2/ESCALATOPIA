using UnityEngine;
using TMPro;

/// <summary>
/// Muestra un texto 3D con la tecla de interacción cuando el jugador apunta al barril.
/// El texto siempre mira a la cámara del jugador (billboard).
/// </summary>
public class BarrilInteractuable : MonoBehaviour, IHighlightable
{
    [Header("Prompt 3D")]
    [Tooltip("Offset respecto al pivote del barril donde aparece el texto.")]
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float fontSize = 3f;
    [SerializeField] private Color textColor = Color.white;

    [Header("Barrel Settings")]
    [Tooltip("Si está asignado, al interactuar llamará ActivarFragmentos.")]
    [SerializeField] private BarrilFragmentado barrilFragmentado;

    private bool isHighlighted = false;
    private TextMeshPro promptText;
    private string lastPromptLabel = string.Empty;

    private void Awake()
    {
        CreatePromptText();
    }

    private void CreatePromptText()
    {
        GameObject go = new GameObject("BarrilPrompt");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = promptOffset;

        promptText = go.AddComponent<TextMeshPro>();
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = fontSize;
        promptText.color = textColor;
        promptText.text = InteractInput.GetBracketedDisplayKey();
        lastPromptLabel = promptText.text;

        go.SetActive(false);
    }

    public void OnHighlightStart()
    {
        isHighlighted = true;
        if (promptText != null) promptText.gameObject.SetActive(true);
    }

    public void OnHighlightEnd()
    {
        isHighlighted = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isHighlighted) return;

        UpdatePromptKeyLabel();

        if (InteractInput.PressedThisFrame())
            Interactuar();
    }

    private void LateUpdate()
    {
        if (promptText == null || !promptText.gameObject.activeSelf) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        promptText.transform.rotation = Quaternion.LookRotation(
            promptText.transform.position - cam.transform.position
        );
    }

    private void Interactuar()
    {
        if (barrilFragmentado != null)
        {
            barrilFragmentado.ActivarFragmentos();
            OnHighlightEnd();
            enabled = false;
        }
    }

    private void UpdatePromptKeyLabel()
    {
        if (promptText == null) return;

        string label = InteractInput.GetBracketedDisplayKey();
        if (label == lastPromptLabel) return;

        promptText.text = label;
        lastPromptLabel = label;
    }
}
