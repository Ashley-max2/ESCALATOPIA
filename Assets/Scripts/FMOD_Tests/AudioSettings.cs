using UnityEngine;
using UnityEngine.UI;

public class AudioVCASettings : MonoBehaviour
{
    [Header("Sliders (rango 0 a 1)")]
    [SerializeField] private Slider sliderGeneralMusic;    // Música + SFX
    [SerializeField] private Slider sliderMusic;           // Música
    [SerializeField] private Slider sliderSFX;             // SFX

    private AudioSettingsManager _audioSettingsManager;

    private void Awake()
    {
        _audioSettingsManager = AudioSettingsManager.GetOrCreate();
    }

    private void Start()
    {
        // Conectar los sliders.
        if (sliderGeneralMusic) sliderGeneralMusic.onValueChanged.AddListener(SetGeneralMusic);
        if (sliderMusic) sliderMusic.onValueChanged.AddListener(SetMusic);
        if (sliderSFX) sliderSFX.onValueChanged.AddListener(SetSFX);

        // Carga y sincroniza sliders con lo guardado.
        LoadAndApplyVolumes();
    }

    private void OnEnable()
    {
        // Al abrir el panel de sonido, solo sincronizamos UI con valores actuales.
        // No reasignamos listeners aqui para evitar duplicados.
        if (_audioSettingsManager == null)
            _audioSettingsManager = AudioSettingsManager.GetOrCreate();

        LoadAndApplyVolumes();
    }

    // Métodos llamados por los sliders
    public void SetGeneralMusic(float volume)
    {
        _audioSettingsManager?.SetGeneralMusic(volume, true);
    }

    public void SetMusic(float volume)
    {
        _audioSettingsManager?.SetMusic(volume, true);
    }

    public void SetSFX(float volume)
    {
        _audioSettingsManager?.SetSfx(volume, true);
    }

    private void LoadAndApplyVolumes()
    {
        if (_audioSettingsManager == null)
            _audioSettingsManager = AudioSettingsManager.GetOrCreate();

        float generalVol = _audioSettingsManager.GeneralVolume;
        float musicVol = _audioSettingsManager.MusicVolume;
        float sfxVol = _audioSettingsManager.SfxVolume;

        if (sliderGeneralMusic) sliderGeneralMusic.SetValueWithoutNotify(generalVol);
        if (sliderMusic) sliderMusic.SetValueWithoutNotify(musicVol);
        if (sliderSFX) sliderSFX.SetValueWithoutNotify(sfxVol);

        _audioSettingsManager.ApplyVolumes(generalVol, musicVol, sfxVol);
    }

    // Llama esto desde el boton de "Restaurar valores" del panel de sonido
    public void ResetToDefaults()
    {
        MusicManager.PlayButton();
        float defaultVol = 0.5f;

        if (sliderGeneralMusic) sliderGeneralMusic.value = defaultVol;
        if (sliderMusic) sliderMusic.value = defaultVol;
        if (sliderSFX) sliderSFX.value = defaultVol;

        SetGeneralMusic(defaultVol);
        SetMusic(defaultVol);
        SetSFX(defaultVol);

        SaveVolumes();
    }

    // Llama esto cuando quieras guardar (p.ej. al cambiar cualquier slider o al salir)
    public void SaveVolumes()
    {
        if (_audioSettingsManager == null)
            _audioSettingsManager = AudioSettingsManager.GetOrCreate();

        float general = sliderGeneralMusic ? sliderGeneralMusic.value : _audioSettingsManager.GeneralVolume;
        float music = sliderMusic ? sliderMusic.value : _audioSettingsManager.MusicVolume;
        float sfx = sliderSFX ? sliderSFX.value : _audioSettingsManager.SfxVolume;

        _audioSettingsManager.SaveAll(general, music, sfx);
    }

    private void OnDestroy()
    {
        // Limpieza de listeners (buena práctica)
        if (sliderGeneralMusic) sliderGeneralMusic.onValueChanged.RemoveListener(SetGeneralMusic);
        if (sliderMusic) sliderMusic.onValueChanged.RemoveListener(SetMusic);
        if (sliderSFX) sliderSFX.onValueChanged.RemoveListener(SetSFX);
    }
}
