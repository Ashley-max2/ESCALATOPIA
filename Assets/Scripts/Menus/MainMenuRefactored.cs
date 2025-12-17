using UnityEngine;

/// <summary>
/// Refactored Main Menu using MVC pattern.
/// Single Responsibility: Main menu UI coordination.
/// Depends on services (SceneService, AudioService) - Dependency Inversion.
/// </summary>
public class MainMenuRefactored : UIViewBase
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject configPanel;
    [SerializeField] private GameObject songPanel;
    [SerializeField] private GameObject screenPanel;

    private GameObject currentPanel;

    private void Start()
    {
        // Show main panel by default
        Show();
        currentPanel = mainPanel;
    }

    /// <summary>
    /// Start the game (load game scene).
    /// </summary>
    public void OnStartGameClicked()
    {
        PlayButtonSound();
        SceneService.Instance.LoadScene("Game");
    }

    /// <summary>
    /// Open configuration panel.
    /// </summary>
    public void OnConfigClicked()
    {
        PlayButtonSound();
        SwitchPanel(configPanel);
    }

    /// <summary>
    /// Open song/audio settings panel.
    /// </summary>
    public void OnSongMenuClicked()
    {
        PlayButtonSound();
        SwitchPanel(songPanel);
    }

    /// <summary>
    /// Open screen settings panel.
    /// </summary>
    public void OnScreenMenuClicked()
    {
        PlayButtonSound();
        SwitchPanel(screenPanel);
    }

    /// <summary>
    /// Return to previous menu.
    /// </summary>
    public void OnBackClicked()
    {
        PlayButtonSound();

        if (currentPanel == configPanel)
        {
            SwitchPanel(mainPanel);
        }
        else if (currentPanel == songPanel || currentPanel == screenPanel)
        {
            SwitchPanel(configPanel);
        }
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void OnQuitClicked()
    {
        PlayButtonSound();
        SceneService.Instance.QuitApplication();
    }

    /// <summary>
    /// Switch between panels.
    /// </summary>
    private void SwitchPanel(GameObject newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.SetActive(true);
            currentPanel = newPanel;
        }
    }

    protected override void OnShow()
    {
        // Ensure main panel is shown
        if (mainPanel != null)
        {
            SwitchPanel(mainPanel);
        }
    }
}
