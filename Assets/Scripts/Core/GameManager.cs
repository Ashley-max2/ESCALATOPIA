using UnityEngine;

/// <summary>
/// GameManager básico para gestionar el estado del juego.
/// Singleton que persiste entre escenas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("=== GAME STATE ===")]
    [SerializeField] private bool isPaused;
    [SerializeField] private bool gameOver;
    
    [Header("=== REFERENCES ===")]
    [SerializeField] private PlayerStateMachine player;
    
    public bool IsPaused => isPaused;
    public bool IsGameOver => gameOver;
    public PlayerStateMachine Player => player;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Find player if not assigned
        if (player == null)
        {
            player = FindObjectOfType<PlayerStateMachine>();
        }
    }
    
    private void Start()
    {
        // Subscribe to events
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnPlayerRespawn += HandlePlayerRespawn;
    }
    
    private void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void TogglePause()
    {
        SetPaused(!isPaused);
    }
    
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = isPaused ? 0 : 1;
        
        // Show/hide cursor
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
    
    private void HandlePlayerDeath(Vector3 position)
    {
        Debug.Log($"Player died at {position}");
        // Could trigger death screen here
    }
    
    private void HandlePlayerRespawn(Vector3 position)
    {
        Debug.Log($"Player respawned at {position}");
        // Could hide death screen here
    }
    
    private void OnDestroy()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnPlayerRespawn -= HandlePlayerRespawn;
    }
}
