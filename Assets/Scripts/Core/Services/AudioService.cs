using UnityEngine;

/// <summary>
/// Service for audio management.
/// Single Responsibility: Audio playback and volume control.
/// </summary>
public class AudioService : MonoBehaviour
{
    private static AudioService instance;
    public static AudioService Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AudioService");
                instance = go.AddComponent<AudioService>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Play a sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Play background music.
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null || musicSource == null)
            return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    /// <summary>
    /// Stop background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Set music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Set SFX volume.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Get music volume.
    /// </summary>
    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 0f;
    }

    /// <summary>
    /// Get SFX volume.
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxSource != null ? sfxSource.volume : 0f;
    }
}
