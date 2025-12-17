using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subtitle system for accessibility.
/// Displays subtitles for audio cues and dialogue.
/// </summary>
public class SubtitleSystem : MonoBehaviour
{
    private static SubtitleSystem instance;
    public static SubtitleSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SubtitleSystem");
                instance = go.AddComponent<SubtitleSystem>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("UI References")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private Text subtitleText;
    [SerializeField] private float defaultDisplayDuration = 3f;

    private float currentDisplayTime = 0f;
    private bool isDisplaying = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isDisplaying)
            return;

        currentDisplayTime -= Time.deltaTime;
        if (currentDisplayTime <= 0f)
        {
            HideSubtitle();
        }
    }

    /// <summary>
    /// Show a subtitle.
    /// </summary>
    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (!AccessibilityManager.Instance.SubtitlesEnabled)
            return;

        if (subtitlePanel == null || subtitleText == null)
        {
            Debug.LogWarning("[SubtitleSystem] UI references not set!");
            return;
        }

        subtitleText.text = text;
        subtitlePanel.SetActive(true);
        isDisplaying = true;
        currentDisplayTime = duration > 0 ? duration : defaultDisplayDuration;

        Debug.Log($"[Subtitles] {text}");
    }

    /// <summary>
    /// Hide the subtitle.
    /// </summary>
    public void HideSubtitle()
    {
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
        isDisplaying = false;
    }

    /// <summary>
    /// Show subtitle for an audio clip.
    /// </summary>
    public void ShowSubtitleForAudio(string audioDescription, AudioClip clip)
    {
        if (clip == null)
            return;

        float duration = clip.length;
        ShowSubtitle(audioDescription, duration);
    }
}
