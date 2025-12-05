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

    [SerializeField] GameObject GamepadPanel;

    public AudioSource SFX_TouchButton;

    GameObject panelAbierto;

    private void Update()
    {
        PanelAbierto(ref panelAbierto);
    }

    public void SFX_Button_UI()
    {
        SFX_TouchButton.Play();

        Debug.Log("Subtitulos: Sonido de bot�n presionado" + SFX_TouchButton.volume);
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

        SFX_Button_UI();
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
        if (GamepadPanel.activeSelf)
            panelAbierto = GamepadPanel;
    }

    public void MenuReturn()
    {
        panelAbierto.SetActive(false);

        if(panelAbierto == ConfigPanel)
            MainMenuPanel.SetActive(true);
        else
            ConfigPanel.SetActive(true);

        SFX_Button_UI();
    }

    public void OpenConfig()
    {
        panelAbierto.SetActive(false);
        ConfigPanel.SetActive(true);

        SFX_Button_UI();
    }

    public void OpenSongMenu()
    {
        panelAbierto.SetActive(false);
        SongPanel.SetActive(true);

        SFX_Button_UI();
    }

    public void OpenGamepadMenu()
    {
        panelAbierto.SetActive(false);
        GamepadPanel.SetActive(true);

        SFX_Button_UI();
    }

    public void OpenScreenMenu()
    {
        panelAbierto.SetActive(false);
        ScreenPanel.SetActive(true);

        SFX_Button_UI();
    }
}