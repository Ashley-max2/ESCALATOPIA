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

    GameObject panelAbierto;

    private void Update()
    {
        PanelAbierto(ref panelAbierto);
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
        UnityEditor.EditorApplication.isPlaying = false;
    }

    void PanelAbierto(ref GameObject panelAbierto)
    {
        if(MainMenuPanel.activeSelf)
            panelAbierto = MainMenuPanel;
        if (ConfigPanel.activeSelf)
            panelAbierto = ConfigPanel;
        if (SongPanel.activeSelf)
            panelAbierto = SongPanel;
        if (ScreenPanel.activeSelf)
            panelAbierto = ScreenPanel;
    }

    public void MenuReturn()
    {
        panelAbierto.SetActive(false);

        if(panelAbierto == ConfigPanel)
            MainMenuPanel.SetActive(true);
        else
            ConfigPanel.SetActive(true);
    }

    public void OpenConfig()
    {
        panelAbierto.SetActive(false);
        ConfigPanel.SetActive(true);
    }

    public void OpenSongMenu()
    {
        panelAbierto.SetActive(false);
        SongPanel.SetActive(true);
    }

    public void OpenScreenMenu()
    {
        panelAbierto.SetActive(false);
        ScreenPanel.SetActive(true);
    }
}
