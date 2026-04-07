using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject MainMenuPanel;
    [SerializeField] GameObject ConfigPanel;
    [SerializeField] GameObject SongPanel;
    [SerializeField] GameObject ScreenPanel;
    [SerializeField] GameObject ControlsPanel;

    [Header("Botones del menú principal")]
    [Tooltip("Botón Continuar: se oculta automáticamente si no hay partida guardada")]
    [SerializeField] private Button botonContinuar;

    [Tooltip("Nombre de la escena inicial del juego (Level_0, etc.)")]
    [SerializeField] private string escenaInicial = "Level_0";

    private void Start()
    {
        // Desbloquear cursor siempre al entrar al menú principal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // Asegurar que solo el panel principal esté activo al inicio
        MainMenuPanel.SetActive(true);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);

        // Mostrar u ocultar el botón Continuar según si existe partida guardada
        ActualizarBotonContinuar();
    }

    /// <summary>
    /// Muestra el botón Continuar solo si hay un save con escena guardada.
    /// </summary>
    private void ActualizarBotonContinuar()
    {
        if (botonContinuar == null) return;

        bool haySave = GameProgressDatabase.HasSave();
        if (haySave)
        {
            GameProgressData save = GameProgressDatabase.Load();
            haySave = save != null && !string.IsNullOrWhiteSpace(save.Scene);
        }

        // Si hay save → botón visible; si no → botón oculto
        botonContinuar.gameObject.SetActive(haySave);
    }

    // ------------------------------------------------------------------ //
    // BOTÓN CONTINUAR
    // Carga la escena y posición guardadas en el JSON.
    // Si por algún motivo no hay save, carga la escena inicial.
    // ------------------------------------------------------------------ //
    public void ContinuarPartida()
    {
        MusicManager.PlayButton();
        string targetScene = escenaInicial;

        if (GameProgressDatabase.HasSave())
        {
            GameProgressData save = GameProgressDatabase.Load();
            if (save != null && !string.IsNullOrWhiteSpace(save.Scene))
                targetScene = save.Scene;
        }

        Debug.Log("[MainMenu] Continuando partida en escena: " + targetScene);
        SceneManager.LoadScene(targetScene);
    }

    // ------------------------------------------------------------------ //
    // BOTÓN NUEVA PARTIDA
    // Borra el save, crea uno vacío con la escena inicial y carga el juego.
    // La próxima vez que el jugador vuelva al menú, Continuar cargará
    // la escena/checkpoint que haya guardado en esa nueva partida.
    // ------------------------------------------------------------------ //
    public void NuevaPartida()
    {
        MusicManager.PlayButton();

        // 1. Borrar save anterior
        GameProgressDatabase.DeleteSave();

        // 2. Crear un save nuevo con la escena inicial (sin posición de spawn)
        //    Así el botón Continuar sabe a qué escena volver aunque no haya checkpoint
        GameProgressData saveNuevo = GameProgressData.CreateDefault();
        saveNuevo.Scene = escenaInicial;
        GameProgressDatabase.Save(saveNuevo);

        Debug.Log("[MainMenu] Nueva partida iniciada. Cargando: " + escenaInicial);
        SceneManager.LoadScene(escenaInicial);
    }

    // Mantenemos PlayGame por si hay botones del Inspector que ya lo usan
    /// <summary>Obsoleto: usa ContinuarPartida() o NuevaPartida().</summary>
    public void PlayGame(string sceneName)
    {
        ContinuarPartida();
    }

    public void Creditos(string sceneName)
    {
        MusicManager.PlayButton();
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(string newScene)
    {
        MusicManager.PlayButton();
        Debug.Log("Cargando escena: " + newScene);
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        MusicManager.PlayButton();
        Debug.Log("Saliste");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenConfig()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(true);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);
    }

    public void OpenSongMenu()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(true);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);
    }

    public void OpenScreenMenu()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(true);
        ControlsPanel.SetActive(false);
    }

    public void OpenControlsMenu()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(true);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);
    }

    public void ReturnToConfig()
    {
        MusicManager.PlayButton();
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(true);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);
    }
}
