using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private string forestMusicPath = "event:/AmbientMusic/MenuMusic";

    private EventInstance forestInstance;

    void Awake()
    {
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

    // Opcional: métodos para controlar desde otros scripts
    public void StopForestMusic()
    {
        forestInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayForestMusic()
    {
        forestInstance.start();
    }
}
