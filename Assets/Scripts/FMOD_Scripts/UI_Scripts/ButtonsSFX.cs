using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsSFX : MonoBehaviour
{
    [SerializeField] private string fmodEventButtons = "event:/SFX/Buttons";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            button.onClick.AddListener(playButtonSound);
        }
        else
        {
            Debug.Log("Fallos en la ruta");
        }
    }

    public void playButtonSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(fmodEventButtons);
    }

    private void OnDestroy()
    {
        if(button != null)
        {
            button.onClick.RemoveListener(playButtonSound);
        }
    }
}
