using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component to automatically apply accessibility settings to UI text.
/// Attach to Text components to enable automatic scaling and adjustments.
/// </summary>
[RequireComponent(typeof(Text))]
public class AccessibleText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool applyTextScale = true;
    [SerializeField] private bool applyColorAdjustment = true;

    private Text textComponent;
    private float baseFontSize;
    private Color baseColor;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        baseFontSize = textComponent.fontSize;
        baseColor = textComponent.color;
    }

    private void OnEnable()
    {
        // Subscribe to accessibility events
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.OnTextScaleChanged += OnTextScaleChanged;
            AccessibilityManager.Instance.OnColorblindModeChanged += OnColorblindModeChanged;
        }

        // Apply current settings
        ApplySettings();
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.OnTextScaleChanged -= OnTextScaleChanged;
            AccessibilityManager.Instance.OnColorblindModeChanged -= OnColorblindModeChanged;
        }
    }

    private void OnTextScaleChanged(float scale)
    {
        if (applyTextScale)
        {
            textComponent.fontSize = Mathf.RoundToInt(baseFontSize * scale);
        }
    }

    private void OnColorblindModeChanged(ColorblindMode mode)
    {
        if (applyColorAdjustment)
        {
            textComponent.color = AccessibilityManager.Instance.AdjustColor(baseColor);
        }
    }

    private void ApplySettings()
    {
        if (AccessibilityManager.Instance == null)
            return;

        if (applyTextScale)
        {
            OnTextScaleChanged(AccessibilityManager.Instance.TextScale);
        }

        if (applyColorAdjustment)
        {
            OnColorblindModeChanged(AccessibilityManager.Instance.CurrentColorblindMode);
        }
    }
}
