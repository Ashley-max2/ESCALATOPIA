using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenSettingsManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private bool initializing = true;

    void Start()
    {
        initializing = true;

        // Resolución
        int savedResolution = PlayerPrefs.GetInt("Resolution", 0);
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            savedResolution = Mathf.Clamp(savedResolution, 0, resolutionDropdown.options.Count - 1);
            resolutionDropdown.value = savedResolution;
            resolutionDropdown.RefreshShownValue();
            ApplyResolutionByIndex(savedResolution);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        // Pantalla completa
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

        initializing = false;
    }

    public void OnResolutionChanged(int index)
    {
        if (initializing) return;
        ApplyResolutionByIndex(index);
        PlayerPrefs.SetInt("Resolution", index);
        PlayerPrefs.Save();
    }

    private void ApplyResolutionByIndex(int index)
    {
        switch (index)
        {
            case 0: SetResolution(1920, 1080); break;
            case 1: SetResolution(1280, 720); break;
            case 2: SetResolution(1024, 768); break;
            default: SetResolution(1920, 1080); break;
        }
    }

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        if (initializing) return;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
