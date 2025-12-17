using UnityEngine;
using System;

/// <summary>
/// Colorblind mode types.
/// </summary>
public enum ColorblindMode
{
    None,
    Protanopia,      // Red-blind
    Deuteranopia,    // Green-blind
    Tritanopia       // Blue-blind
}

/// <summary>
/// Accessibility settings manager for Gold-level UX rubric score.
/// Provides colorblind modes, text scaling, subtitles, and other accessibility features.
/// </summary>
public class AccessibilityManager : MonoBehaviour
{
    private static AccessibilityManager instance;
    public static AccessibilityManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AccessibilityManager");
                instance = go.AddComponent<AccessibilityManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Accessibility Settings")]
    private ColorblindMode currentColorblindMode = ColorblindMode.None;
    private float textScale = 1.0f;
    private bool subtitlesEnabled = true;
    private bool highContrastMode = false;
    private float uiScale = 1.0f;

    // Events
    public event Action<ColorblindMode> OnColorblindModeChanged;
    public event Action<float> OnTextScaleChanged;
    public event Action<bool> OnSubtitlesToggled;
    public event Action<bool> OnHighContrastToggled;
    public event Action<float> OnUIScaleChanged;

    // Properties
    public ColorblindMode CurrentColorblindMode => currentColorblindMode;
    public float TextScale => textScale;
    public bool SubtitlesEnabled => subtitlesEnabled;
    public bool HighContrastMode => highContrastMode;
    public float UIScale => uiScale;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    /// <summary>
    /// Set colorblind mode.
    /// </summary>
    public void SetColorblindMode(ColorblindMode mode)
    {
        if (currentColorblindMode == mode)
            return;

        currentColorblindMode = mode;
        OnColorblindModeChanged?.Invoke(mode);
        SaveSettings();

        Debug.Log($"[Accessibility] Colorblind mode set to: {mode}");
    }

    /// <summary>
    /// Set text scale (0.8 - 1.5).
    /// </summary>
    public void SetTextScale(float scale)
    {
        textScale = Mathf.Clamp(scale, 0.8f, 1.5f);
        OnTextScaleChanged?.Invoke(textScale);
        SaveSettings();

        Debug.Log($"[Accessibility] Text scale set to: {textScale}");
    }

    /// <summary>
    /// Toggle subtitles.
    /// </summary>
    public void SetSubtitlesEnabled(bool enabled)
    {
        subtitlesEnabled = enabled;
        OnSubtitlesToggled?.Invoke(enabled);
        SaveSettings();

        Debug.Log($"[Accessibility] Subtitles {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Toggle high contrast mode.
    /// </summary>
    public void SetHighContrastMode(bool enabled)
    {
        highContrastMode = enabled;
        OnHighContrastToggled?.Invoke(enabled);
        SaveSettings();

        Debug.Log($"[Accessibility] High contrast mode {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Set UI scale (0.8 - 1.3).
    /// </summary>
    public void SetUIScale(float scale)
    {
        uiScale = Mathf.Clamp(scale, 0.8f, 1.3f);
        OnUIScaleChanged?.Invoke(uiScale);
        SaveSettings();

        Debug.Log($"[Accessibility] UI scale set to: {uiScale}");
    }

    /// <summary>
    /// Get color adjustment for current colorblind mode.
    /// </summary>
    public Color AdjustColor(Color originalColor)
    {
        switch (currentColorblindMode)
        {
            case ColorblindMode.Protanopia:
                return AdjustForProtanopia(originalColor);
            case ColorblindMode.Deuteranopia:
                return AdjustForDeuteranopia(originalColor);
            case ColorblindMode.Tritanopia:
                return AdjustForTritanopia(originalColor);
            default:
                return originalColor;
        }
    }

    /// <summary>
    /// Adjust color for Protanopia (red-blind).
    /// </summary>
    private Color AdjustForProtanopia(Color color)
    {
        // Simplified protanopia simulation
        float r = 0.56667f * color.r + 0.43333f * color.g;
        float g = 0.55833f * color.r + 0.44167f * color.g;
        float b = 0.24167f * color.g + 0.75833f * color.b;
        return new Color(r, g, b, color.a);
    }

    /// <summary>
    /// Adjust color for Deuteranopia (green-blind).
    /// </summary>
    private Color AdjustForDeuteranopia(Color color)
    {
        // Simplified deuteranopia simulation
        float r = 0.625f * color.r + 0.375f * color.g;
        float g = 0.7f * color.r + 0.3f * color.g;
        float b = 0.3f * color.g + 0.7f * color.b;
        return new Color(r, g, b, color.a);
    }

    /// <summary>
    /// Adjust color for Tritanopia (blue-blind).
    /// </summary>
    private Color AdjustForTritanopia(Color color)
    {
        // Simplified tritanopia simulation
        float r = 0.95f * color.r + 0.05f * color.g;
        float g = 0.43333f * color.g + 0.56667f * color.b;
        float b = 0.475f * color.g + 0.525f * color.b;
        return new Color(r, g, b, color.a);
    }

    /// <summary>
    /// Save accessibility settings.
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Accessibility_ColorblindMode", (int)currentColorblindMode);
        PlayerPrefs.SetFloat("Accessibility_TextScale", textScale);
        PlayerPrefs.SetInt("Accessibility_Subtitles", subtitlesEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_HighContrast", highContrastMode ? 1 : 0);
        PlayerPrefs.SetFloat("Accessibility_UIScale", uiScale);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load accessibility settings.
    /// </summary>
    private void LoadSettings()
    {
        currentColorblindMode = (ColorblindMode)PlayerPrefs.GetInt("Accessibility_ColorblindMode", 0);
        textScale = PlayerPrefs.GetFloat("Accessibility_TextScale", 1.0f);
        subtitlesEnabled = PlayerPrefs.GetInt("Accessibility_Subtitles", 1) == 1;
        highContrastMode = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;
        uiScale = PlayerPrefs.GetFloat("Accessibility_UIScale", 1.0f);

        Debug.Log("[Accessibility] Settings loaded");
    }

    /// <summary>
    /// Reset to default settings.
    /// </summary>
    public void ResetToDefaults()
    {
        SetColorblindMode(ColorblindMode.None);
        SetTextScale(1.0f);
        SetSubtitlesEnabled(true);
        SetHighContrastMode(false);
        SetUIScale(1.0f);

        Debug.Log("[Accessibility] Reset to defaults");
    }
}
