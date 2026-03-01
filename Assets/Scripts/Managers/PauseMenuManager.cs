using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona los paneles del menu de pausa durante el gameplay.
/// Funciona igual que MainMenu.cs pero dentro del contexto de pausa.
/// GameManager se encarga de activar/desactivar el panel raiz con ESC.
/// Este script solo gestiona la navegacion entre subpaneles.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pauseMenuPanel;    // Panel principal (Reanudar, Config, Salir)
    [SerializeField] private GameObject configPanel;       // Submenu configuracion
    [SerializeField] private GameObject songPanel;         // Ajustes de sonido
    [SerializeField] private GameObject screenPanel;       // Ajustes de pantalla
    [SerializeField] private GameObject controlsPanel;     // Panel de rebind de teclas

    [Header("Escena del Menu Principal")]
    [SerializeField] private string mainMenuSceneName = "Main_Menu";

    private void OnEnable()
    {
        // Siempre mostrar el panel principal al abrir la pausa
        ShowOnly(pauseMenuPanel);
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
        // Restaurar tiempo y limpiar GameManager primero
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        // Desbloquear cursor DESPUES de ResumeGame (que lo bloquea)
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

    // ==================== HELPER ====================

    /// <summary>
    /// Muestra solo el panel indicado y oculta todos los demas.
    /// </summary>
    private void ShowOnly(GameObject panelToShow)
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(panelToShow == pauseMenuPanel);
        if (configPanel)    configPanel.SetActive(panelToShow == configPanel);
        if (songPanel)      songPanel.SetActive(panelToShow == songPanel);
        if (screenPanel)    screenPanel.SetActive(panelToShow == screenPanel);
        if (controlsPanel)  controlsPanel.SetActive(panelToShow == controlsPanel);
    }
}
