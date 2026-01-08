using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Cinematic
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    public int currentChapterIndex = 0;
    public string[] chapters = { "Tutorial", "Boss1", "Boss2", "Boss3", "Boss4", "Final" };

    [Header("Events")]
    public UnityEvent<GameState> OnStateChanged;
    public UnityEvent<int> OnChapterChanged;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        
        // Handle time scale
        Time.timeScale = (newState == GameState.Paused || newState == GameState.MainMenu) ? 0f : 1f;

        Debug.Log($"[GameManager] State changed to {newState}");
    }

    public void StartGame()
    {
        currentChapterIndex = 0;
        LoadChapter(currentChapterIndex);
        SetState(GameState.Playing);
    }

    public void NextChapter()
    {
        currentChapterIndex++;
        if (currentChapterIndex < chapters.Length)
        {
            LoadChapter(currentChapterIndex);
        }
        else
        {
            // Win game
            Debug.Log("Game Completed!");
            SetState(GameState.GameOver); // Or Win State
        }
    }

    private void LoadChapter(int index)
    {
        string sceneName = chapters[index];
        Debug.Log($"[GameManager] Loading Chapter: {sceneName}");
        OnChapterChanged?.Invoke(index);
        // SceneManager.LoadScene(sceneName); // Uncomment when scenes are set up
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            SetState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            SetState(GameState.Playing);
    }
}
