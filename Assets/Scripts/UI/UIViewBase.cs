using UnityEngine;

/// <summary>
/// Base class for all UI views following MVC pattern.
/// Single Responsibility: UI display and user interaction.
/// </summary>
public abstract class UIViewBase : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] protected GameObject mainPanel;

    [Header("Audio")]
    [SerializeField] protected AudioClip buttonClickSound;

    protected bool isVisible = false;

    /// <summary>
    /// Show the UI view.
    /// </summary>
    public virtual void Show()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            isVisible = true;
            OnShow();
        }
    }

    /// <summary>
    /// Hide the UI view.
    /// </summary>
    public virtual void Hide()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
            isVisible = false;
            OnHide();
        }
    }

    /// <summary>
    /// Toggle visibility.
    /// </summary>
    public void Toggle()
    {
        if (isVisible)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// Play button click sound.
    /// </summary>
    protected void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            AudioService.Instance.PlaySFX(buttonClickSound);
        }
    }

    /// <summary>
    /// Called when view is shown.
    /// Override for custom behavior.
    /// </summary>
    protected virtual void OnShow() { }

    /// <summary>
    /// Called when view is hidden.
    /// Override for custom behavior.
    /// </summary>
    protected virtual void OnHide() { }

    /// <summary>
    /// Check if view is currently visible.
    /// </summary>
    public bool IsVisible => isVisible;
}
