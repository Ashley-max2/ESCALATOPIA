using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioVCASettings : MonoBehaviour
{
    [Header("Sliders (rango 0 a 1)")]
    [SerializeField] private Slider sliderGeneralMusic;    // Música + SFX
    [SerializeField] private Slider sliderMusic;           // Música
    [SerializeField] private Slider sliderSFX;             // SFX

    // Referencias a VCAs y Bus Master
    private VCA vcaGeneralMusic;
    private VCA vcaMusic;
    private VCA vcaSFX;

    void Awake()
    {
        vcaGeneralMusic = GetVCA("vca:/GeneralMusic");
        vcaMusic        = GetVCA("vca:/Music");
        vcaSFX          = GetVCA("vca:/SFX");
    }

    private VCA GetVCA(string path)
    {
        VCA vca;
        FMOD.RESULT result = RuntimeManager.StudioSystem.getVCA(path, out vca);
        if (result != FMOD.RESULT.OK || !vca.isValid())
        {
            Debug.LogError($"[FMOD] VCA no encontrado: {path} → {result}. Verifica el path copiado desde FMOD.");
        }
        return vca;
    }

    void Start()
    {
        // Conectar los sliders (en el Inspector: OnValueChanged → Dynamic float → este método)
        if (sliderGeneralMusic)   sliderGeneralMusic.onValueChanged.AddListener(SetGeneralMusic);
        if (sliderMusic)          sliderMusic.onValueChanged.AddListener(SetMusic);
        if (sliderSFX)            sliderSFX.onValueChanged.AddListener(SetSFX);

        // Valores iniciales (carga de PlayerPrefs o usa 1f por defecto)
        LoadAndApplyVolumes();
    }

    // Métodos llamados por los sliders
    public void SetGeneralMusic(float volume)
    {
        if (vcaGeneralMusic.isValid()) vcaGeneralMusic.setVolume(volume);
    }

    public void SetMusic(float volume)
    {
        if (vcaMusic.isValid()) vcaMusic.setVolume(volume);
    }

    public void SetSFX(float volume)
    {
        if (vcaSFX.isValid()) vcaSFX.setVolume(volume);
    }

    private void LoadAndApplyVolumes()
    {
        float generalVol      = PlayerPrefs.GetFloat("Vol_GeneralMusic", 0.5f);
        float musicVol        = PlayerPrefs.GetFloat("Vol_Music",        0.5f);
        float sfxVol          = PlayerPrefs.GetFloat("Vol_SFX",          0.5f);

        if (sliderGeneralMusic)   sliderGeneralMusic.value   = generalVol;
        if (sliderMusic)          sliderMusic.value          = musicVol;
        if (sliderSFX)            sliderSFX.value            = sfxVol;

        SetGeneralMusic(generalVol);
        SetMusic(musicVol);
        SetSFX(sfxVol);
    }

    // Llama esto cuando quieras guardar (p.ej. al cambiar cualquier slider o al salir)
    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat("Vol_GeneralMusic", sliderGeneralMusic ? sliderGeneralMusic.value : 1f);
        PlayerPrefs.SetFloat("Vol_Music",        sliderMusic ? sliderMusic.value : 1f);
        PlayerPrefs.SetFloat("Vol_SFX",          sliderSFX ? sliderSFX.value : 1f);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        // Limpieza de listeners (buena práctica)
        if (sliderGeneralMusic)   sliderGeneralMusic.onValueChanged.RemoveListener(SetGeneralMusic);
        if (sliderMusic)          sliderMusic.onValueChanged.RemoveListener(SetMusic);
        if (sliderSFX)            sliderSFX.onValueChanged.RemoveListener(SetSFX);
    }
}