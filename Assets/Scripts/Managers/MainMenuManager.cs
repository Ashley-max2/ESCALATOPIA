using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject ConfigPanel;
    [SerializeField] private GameObject SongPanel;
    [SerializeField] private GameObject ScreenPanel;

    private GameObject currentActivePanel;

    private void Start()
    {
        // Asegurar que solo el panel principal está activo al inicio
        if (MainMenuPanel != null)
        {
            MainMenuPanel.SetActive(true);
            currentActivePanel = MainMenuPanel;
        }
        
        DeactivatePanels();
        MainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Abre la escena de juego
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("Cargando escena: PlayerTesting");
        SceneManager.LoadScene("PlayerTesting");
    }

    /// <summary>
    /// Abre el panel de configuración
    /// </summary>
    public void OpenConfigPanel()
    {
        ChangePanel(ConfigPanel);
    }

    /// <summary>
    /// Abre el panel de sonido
    /// </summary>
    public void OpenSongPanel()
    {
        ChangePanel(SongPanel);
    }

    /// <summary>
    /// Abre el panel de pantalla
    /// </summary>
    public void OpenScreenPanel()
    {
        ChangePanel(ScreenPanel);
    }

    /// <summary>
    /// Vuelve al panel anterior según el contexto
    /// </summary>
    public void ReturnToPreviousPanel()
    {
        // Si estamos en el panel de sonido o pantalla, volver a configuración
        if (currentActivePanel == SongPanel || currentActivePanel == ScreenPanel)
        {
            ChangePanel(ConfigPanel);
        }
        // Si estamos en configuración, volver al menú principal
        else if (currentActivePanel == ConfigPanel)
        {
            ChangePanel(MainMenuPanel);
        }
    }

    /// <summary>
    /// Cambia de panel desactivando el actual y activando el nuevo
    /// </summary>
    private void ChangePanel(GameObject newPanel)
    {
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.SetActive(true);
            currentActivePanel = newPanel;
        }
    }

    /// <summary>
    /// Desactiva todos los paneles
    /// </summary>
    private void DeactivatePanels()
    {
        if (MainMenuPanel != null) MainMenuPanel.SetActive(false);
        if (ConfigPanel != null) ConfigPanel.SetActive(false);
        if (SongPanel != null) SongPanel.SetActive(false);
        if (ScreenPanel != null) ScreenPanel.SetActive(false);
    }

    /// <summary>
    /// Sale del juego
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
