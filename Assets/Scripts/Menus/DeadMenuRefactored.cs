using UnityEngine;

/// <summary>
/// Refactored Dead/Game Over Menu using MVC pattern.
/// Single Responsibility: Death/game over UI coordination.
/// </summary>
public class DeadMenuRefactored : UIViewBase
{
    [Header("Death Stats")]
    [SerializeField] private UnityEngine.UI.Text deathCountText;
    [SerializeField] private UnityEngine.UI.Text timePlayedText;

    private int deathCount = 0;
    private float timePlayed = 0f;

    /// <summary>
    /// Show the dead menu with stats.
    /// </summary>
    public void ShowWithStats(int deaths, float time)
    {
        deathCount = deaths;
        timePlayed = time;
        UpdateStatsDisplay();
        Show();
    }

    /// <summary>
    /// Retry the level.
    /// </summary>
    public void OnRetryClicked()
    {
        PlayButtonSound();
        SceneService.Instance.ReloadCurrentScene();
    }

    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void OnMainMenuClicked()
    {
        PlayButtonSound();
        SceneService.Instance.LoadScene("Main_Menu");
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
    /// Update the stats display.
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (deathCountText != null)
        {
            deathCountText.text = $"Deaths: {deathCount}";
        }

        if (timePlayedText != null)
        {
            int minutes = Mathf.FloorToInt(timePlayed / 60f);
            int seconds = Mathf.FloorToInt(timePlayed % 60f);
            timePlayedText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    protected override void OnShow()
    {
        // Pause the game when showing death menu
        Time.timeScale = 0f;
    }

    protected override void OnHide()
    {
        // Resume time
        Time.timeScale = 1f;
    }
}
