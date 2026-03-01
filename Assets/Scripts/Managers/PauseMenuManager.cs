using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona los paneles del menu de pausa durante el gameplay.
/// Este script SIEMPRE esta activo. No se desactiva nunca.
/// Solo muestra/oculta los paneles hijos segun el estado de pausa.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Paneles")]
    [SerializeField] private GameObject pauseMenuPanel;    // Panel principal (Reanudar, Config, Salir)
    [SerializeField] private GameObject configPanel;       // Submenu configuracion
    [SerializeField] private GameObject songPanel;         // Ajustes de sonido
    [SerializeField] private GameObject screenPanel;       // Ajustes de pantalla
    [SerializeField] private GameObject controlsPanel;     // Panel de rebind de teclas

    [Header("Escena del Menu Principal")]
    [SerializeField] private string mainMenuSceneName = "Main_Menu";

    private void Awake()
    {
        Instance = this;

        // Registrar con GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPausePanel(this);

        // Ocultar todos los paneles al empezar
        HideAll();
    }

    // ==================== MOSTRAR / OCULTAR ====================

    /// <summary>Muestra el menu de pausa (llamado por GameManager.PauseGame)</summary>
    public void Show()
    {
        ShowOnly(pauseMenuPanel);
    }

    /// <summary>Oculta todos los paneles (llamado por GameManager.ResumeGame)</summary>
    public void Hide()
    {
        HideAll();
    }

    // ==================== NAVEGACION ====================

    /// <summary>Reanuda el juego (boton "Reanudar")</summary>
    public void Resume()
    {
        MusicManager.PlayButton();
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    /// <summary>Abre el submenu de configuracion</summary>
    public void OpenConfig()
    {
        MusicManager.PlayButton();
        ShowOnly(configPanel);
    }

    /// <summary>Abre el panel de rebind de controles</summary>
    public void OpenControls()
    {
        MusicManager.PlayButton();
        ShowOnly(controlsPanel);
    }

    /// <summary>Abre el panel de sonido</summary>
    public void OpenSong()
    {
        MusicManager.PlayButton();
        ShowOnly(songPanel);
    }

    /// <summary>Abre el panel de pantalla</summary>
    public void OpenScreen()
    {
        MusicManager.PlayButton();
        ShowOnly(screenPanel);
    }

    /// <summary>Vuelve al panel principal de pausa (desde config/controles/etc)</summary>
    public void ReturnToPauseMenu()
    {
        MusicManager.PlayButton();
        ShowOnly(pauseMenuPanel);
    }

    /// <summary>Vuelve al panel de configuracion (desde controles/sonido/pantalla)</summary>
    public void ReturnToConfig()
    {
        MusicManager.PlayButton();
        ShowOnly(configPanel);
    }

    // ==================== SALIR ====================

    /// <summary>Vuelve al menu principal</summary>
    public void ExitToMainMenu()
    {
        MusicManager.PlayButton();
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>Cierra el juego</summary>
    public void QuitGame()
    {
        MusicManager.PlayButton();
        Time.timeScale = 1f;
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ==================== HELPERS ====================

    private void ShowOnly(GameObject panelToShow)
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(panelToShow == pauseMenuPanel);
        if (configPanel)    configPanel.SetActive(panelToShow == configPanel);
        if (songPanel)      songPanel.SetActive(panelToShow == songPanel);
        if (screenPanel)    screenPanel.SetActive(panelToShow == screenPanel);
        if (controlsPanel)  controlsPanel.SetActive(panelToShow == controlsPanel);
    }

    private void HideAll()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (configPanel)    configPanel.SetActive(false);
        if (songPanel)      songPanel.SetActive(false);
        if (screenPanel)    screenPanel.SetActive(false);
        if (controlsPanel)  controlsPanel.SetActive(false);
    }
}
