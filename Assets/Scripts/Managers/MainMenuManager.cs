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

    private void Start()
    {
        // Asegurar que solo el panel principal esté activo al inicio
        MainMenuPanel.SetActive(true);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
    }

    public void PlayGame(string sceneName)
    {
        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(string newScene)
    {
        Debug.Log("Cargando escena: " + newScene);
        SceneManager.LoadScene(newScene);
    }

    public void ExitGame()
    {
        Debug.Log("Saliste");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenConfig()
    {
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(true);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
    }

    public void OpenSongMenu()
    {
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(true);
        ScreenPanel.SetActive(false);
    }

    public void OpenScreenMenu()
    {
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        MainMenuPanel.SetActive(true);
        ConfigPanel.SetActive(false);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
    }

    public void ReturnToConfig()
    {
        MainMenuPanel.SetActive(false);
        ConfigPanel.SetActive(true);
        SongPanel.SetActive(false);
        ScreenPanel.SetActive(false);
    }
}