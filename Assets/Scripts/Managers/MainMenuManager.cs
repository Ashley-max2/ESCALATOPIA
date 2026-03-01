using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject MainMenuPanel;
    [SerializeField] GameObject ConfigPanel;
    [SerializeField] GameObject SongPanel;
    [SerializeField] GameObject ScreenPanel;
    [SerializeField] GameObject ControlsPanel;

    private void Start()
    {
        // Desbloquear cursor siempre al entrar al menu principal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // Asegurar que solo el panel principal esté activo al inicio
        MainMenuPanel.SetActive(true);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
        ControlsPanel.SetActive(false);
    }

    public void PlayGame(string sceneName)
    {
        MusicManager.PlayButton();
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
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
