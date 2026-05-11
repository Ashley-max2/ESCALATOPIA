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
        // Si ya existe uno de otra escena, destruir el ANTIGUO
        // y quedarnos con el de ESTA escena (musica diferente por escena)
        if (Instance != null && Instance != this)
        {
            Destroy(Instance); // destruir solo el componente antiguo
        }

        Instance = this;
        // NO usamos DontDestroyOnLoad: cada escena tiene su propia musica

        try
        {
            forestInstance = RuntimeManager.CreateInstance(forestMusicPath);
            forestInstance.start();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MusicManager] No se pudo crear evento FMOD '{forestMusicPath}': {e.Message}. ¿Están los banks buildeados?");
        }
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
