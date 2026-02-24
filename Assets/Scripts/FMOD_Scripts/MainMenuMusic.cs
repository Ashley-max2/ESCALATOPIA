using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private string menuMusicPath = "event:/Ambient/MenuMusic";

    private EventInstance menuMusicInstance;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        menuMusicInstance = RuntimeManager.CreateInstance(menuMusicPath);
        menuMusicInstance.start();
    }

    void OnDestroy()
    {
        if (menuMusicInstance.isValid())
        {
            menuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            menuMusicInstance.release();
        }
    }

}
