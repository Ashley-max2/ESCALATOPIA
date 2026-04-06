using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Singleton persistente que aplica y guarda volúmenes de FMOD entre escenas.
/// </summary>
public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    private const string KEY_GENERAL = "Vol_GeneralMusic";
    private const string KEY_MUSIC = "Vol_Music";
    private const string KEY_SFX = "Vol_SFX";

    private const float DEFAULT_VOLUME = 0.5f;

    private VCA _vcaGeneralMusic;
    private VCA _vcaMusic;
    private VCA _vcaSfx;

    public float GeneralVolume => PlayerPrefs.GetFloat(KEY_GENERAL, DEFAULT_VOLUME);
    public float MusicVolume => PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_VOLUME);
    public float SfxVolume => PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_VOLUME);

    public static AudioSettingsManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        AudioSettingsManager[] managers = Object.FindObjectsByType<AudioSettingsManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        AudioSettingsManager found = (managers != null && managers.Length > 0) ? managers[0] : null;
        if (found != null)
            return found;

        GameObject go = new GameObject("AudioSettingsManager");
        return go.AddComponent<AudioSettingsManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CacheVcas();
        EnsureSavedKeysExist();
        ApplySavedVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reaplica al cargar escena para afectar nueva música/eventos creados en esa escena.
        CacheVcas();
        ApplySavedVolumes();
    }

    private void CacheVcas()
    {
        _vcaGeneralMusic = GetVca("vca:/GeneralMusic");
        _vcaMusic = GetVca("vca:/Music");
        _vcaSfx = GetVca("vca:/SFX");
    }

    private void EnsureSavedKeysExist()
    {
        bool changed = false;

        if (!PlayerPrefs.HasKey(KEY_GENERAL))
        {
            PlayerPrefs.SetFloat(KEY_GENERAL, GetCurrentVcaVolume(_vcaGeneralMusic, 1f));
            changed = true;
        }

        if (!PlayerPrefs.HasKey(KEY_MUSIC))
        {
            PlayerPrefs.SetFloat(KEY_MUSIC, GetCurrentVcaVolume(_vcaMusic, 1f));
            changed = true;
        }

        if (!PlayerPrefs.HasKey(KEY_SFX))
        {
            PlayerPrefs.SetFloat(KEY_SFX, GetCurrentVcaVolume(_vcaSfx, 1f));
            changed = true;
        }

        if (changed)
            PlayerPrefs.Save();
    }

    private float GetCurrentVcaVolume(VCA vca, float fallback)
    {
        if (!vca.isValid())
            return fallback;

        float volume;
        FMOD.RESULT result = vca.getVolume(out volume);
        if (result != FMOD.RESULT.OK)
            return fallback;

        return Mathf.Clamp01(volume);
    }

    private VCA GetVca(string path)
    {
        VCA vca;
        FMOD.RESULT result = RuntimeManager.StudioSystem.getVCA(path, out vca);
        if (result != FMOD.RESULT.OK || !vca.isValid())
        {
            Debug.LogError($"[FMOD] VCA no encontrado: {path} -> {result}. Verifica el path en FMOD.");
        }

        return vca;
    }

    public void ApplySavedVolumes()
    {
        ApplyVolumes(
            PlayerPrefs.GetFloat(KEY_GENERAL, DEFAULT_VOLUME),
            PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_VOLUME),
            PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_VOLUME));
    }

    public void ApplyVolumes(float general, float music, float sfx)
    {
        if (_vcaGeneralMusic.isValid()) _vcaGeneralMusic.setVolume(general);
        if (_vcaMusic.isValid()) _vcaMusic.setVolume(music);
        if (_vcaSfx.isValid()) _vcaSfx.setVolume(sfx);
    }

    public void SetGeneralMusic(float volume, bool save = true)
    {
        if (_vcaGeneralMusic.isValid()) _vcaGeneralMusic.setVolume(volume);
        if (save)
        {
            PlayerPrefs.SetFloat(KEY_GENERAL, volume);
            PlayerPrefs.Save();
        }
    }

    public void SetMusic(float volume, bool save = true)
    {
        if (_vcaMusic.isValid()) _vcaMusic.setVolume(volume);
        if (save)
        {
            PlayerPrefs.SetFloat(KEY_MUSIC, volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSfx(float volume, bool save = true)
    {
        if (_vcaSfx.isValid()) _vcaSfx.setVolume(volume);
        if (save)
        {
            PlayerPrefs.SetFloat(KEY_SFX, volume);
            PlayerPrefs.Save();
        }
    }

    public void SaveAll(float general, float music, float sfx)
    {
        PlayerPrefs.SetFloat(KEY_GENERAL, general);
        PlayerPrefs.SetFloat(KEY_MUSIC, music);
        PlayerPrefs.SetFloat(KEY_SFX, sfx);
        PlayerPrefs.Save();
        ApplyVolumes(general, music, sfx);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        GetOrCreate();
    }
}
