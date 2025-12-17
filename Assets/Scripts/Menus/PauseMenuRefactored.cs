using UnityEngine;

/// <summary>
/// Refactored Pause Menu using MVC pattern.
/// Single Responsibility: Pause menu UI coordination.
/// </summary>
public class PauseMenuRefactored : UIViewBase
{
    [Header("Pause Settings")]
    [SerializeField] private bool pauseTimeOnShow = true;

    private void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    /// <summary>
    /// Resume the game.
    /// </summary>
    public void OnResumeClicked()
    {
        PlayButtonSound();
        Hide();
    }

    /// <summary>
    /// Restart the current level.
    /// </summary>
    public void OnRestartClicked()
    {
        PlayButtonSound();
        Time.timeScale = 1f; // Ensure time is running
        SceneService.Instance.ReloadCurrentScene();
    }

    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void OnMainMenuClicked()
    {
        PlayButtonSound();
        Time.timeScale = 1f; // Ensure time is running
        SceneService.Instance.LoadScene("Main_Menu");
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void OnQuitClicked()
    {
        PlayButtonSound();
        Time.timeScale = 1f; // Ensure time is running
        SceneService.Instance.QuitApplication();
    }

    protected override void OnShow()
    {
        if (pauseTimeOnShow)
        {
            Time.timeScale = 0f;
            Debug.Log("[PauseMenu] Game paused");
        }
    }

    protected override void OnHide()
    {
        if (pauseTimeOnShow)
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseMenu] Game resumed");
        }
    }
}
