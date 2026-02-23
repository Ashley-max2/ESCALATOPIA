using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanelMusic: MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("FMOD Snapshot")]
    [SerializeField] private string snapshotPath = "snapshot:/PauseMenu";

    [Header("Opcional: Parámetro global Pause (si lo usas)")]
    [SerializeField] private bool useGlobalPauseParameter = true;

    private EventInstance snapshotInstance;
    private bool isPaused = false;

    void Awake()
    {
        snapshotInstance = RuntimeManager.CreateInstance(snapshotPath);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Mostrar menú de pausa
            if (pausePanel != null) pausePanel.SetActive(true);

            // Pausar física y scripts del juego
            Time.timeScale = 0f;

            // Activar efecto de graves
            snapshotInstance.start();

            // Opcional: si usas parámetro global "Pause" para ducking/pausa de sonidos
            if (useGlobalPauseParameter)
            {
                RuntimeManager.StudioSystem.setParameterByName("Pause", 1f);
            }
        }
        else
        {
            // Ocultar menú
            if (pausePanel != null) pausePanel.SetActive(false);

            // Reanudar juego
            Time.timeScale = 1f;

            // Quitar efecto muffled (fade out natural si lo tienes automatizado en el snapshot)
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            if (useGlobalPauseParameter)
            {
                RuntimeManager.StudioSystem.setParameterByName("Pause", 0f);
            }
        }

        // Opcional: cursor visible solo en pausa (para menús con ratón)
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Limpieza importante para evitar memory leaks
    void OnDestroy()
    {
        if (snapshotInstance.isValid())
        {
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            snapshotInstance.release();
        }
    }

    // Método para llamar desde botones
    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }
}
