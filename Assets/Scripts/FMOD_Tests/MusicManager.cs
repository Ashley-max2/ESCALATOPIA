using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private string forestMusicPath = "event:/AmbientMusic/MenuMusic";

    private EventInstance forestInstance;

    void Awake()
    {
        // Singleton: si ya existe uno, destruir el duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        forestInstance = RuntimeManager.CreateInstance(forestMusicPath);
        forestInstance.start();
    }

    void OnDestroy()
    {
        if (forestInstance.isValid())
        {
            forestInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            forestInstance.release();
        }
    }

    public void StopForestMusic()
    {
        forestInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayForestMusic()
    {
        forestInstance.start();
    }

    public static void PlayButton()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Buttons");
    }
}
