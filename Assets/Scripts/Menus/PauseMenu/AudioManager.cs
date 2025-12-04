using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public Slider musicSlider;
    public Slider sfxSlider;

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private bool initializing = true; // Para evitar llamadas al iniciar

    void Start()
    {
        // --- Evitar ejecuciones durante inicialización ---
        initializing = true;

        // --- Cargar volúmenes ---
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
        musicSource.volume = musicVol;
        sfxSource.volume = sfxVol;

        // --- Conectar eventos de sliders ---
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // --- Resolución ---
        int savedResolution = PlayerPrefs.GetInt("Resolution", 0);
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            savedResolution = Mathf.Clamp(savedResolution, 0, resolutionDropdown.options.Count - 1);
            resolutionDropdown.value = savedResolution;
            resolutionDropdown.RefreshShownValue();
            ApplyResolutionByIndex(savedResolution);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        // --- Pantalla completa ---
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

        // --- Música ---
        if (!musicSource.isPlaying)
            musicSource.Play();

        // --- Fin de inicialización ---
        initializing = false;
    }

    public void SetMusicVolume(float volume)
    {
        if (initializing) return;
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (initializing) return;
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void PlaySFX()
    {
        if (sfxSource.clip != null)
            sfxSource.PlayOneShot(sfxSource.clip);
    }

    public void OnResolutionChanged(int index)
    {
        if (initializing) return;
        ApplyResolutionByIndex(index);
        PlayerPrefs.SetInt("Resolution", index);
        PlayerPrefs.Save(); // Guardar todos los cambios actuales juntos
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

    public void RestartValuesVolume()
    {
        PlayerPrefs.DeleteKey("MusicVol");
        PlayerPrefs.DeleteKey("SFXVol");

        musicSlider.value = 0.5f;
        sfxSlider.value = 0.5f;
        musicSource.volume = 0.5f;
        sfxSource.volume = 0.5f;

        PlayerPrefs.SetFloat("MusicVol", 0.5f);
        PlayerPrefs.SetFloat("SFXVol", 0.5f);
        PlayerPrefs.Save();
    }

    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        if (initializing) return;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save(); // Guarda solo al cambiar modo pantalla
    }
}
