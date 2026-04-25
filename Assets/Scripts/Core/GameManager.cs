using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager básico para gestionar el estado del juego.
/// Singleton que persiste entre escenas.
/// Gestiona la pausa con panel UI, ESC, y auto-pausa al perder foco.
/// </summary>
public class GameManager : MonoBehaviour
{
    private const string AutoPauseCanvasName = "AutoPauseCanvas";

    public static GameManager Instance { get; private set; }

    [Header("=== GAME STATE ===")]
    [SerializeField] private bool isPaused;
    [SerializeField] private bool gameOver;

    [Header("=== REFERENCES ===")]
    [SerializeField] private PlayerStateMachine player;

    [Header("=== PAUSE PANEL ===")]
    [Tooltip("Si lo dejas vacio se crea uno automaticamente")]
    [SerializeField] private GameObject pausePanel;

    [Header("=== PAUSE SETTINGS ===")]
    [Tooltip("Escenas donde la pausa (ESC/Start/Options) queda deshabilitada")]
    [SerializeField] private string[] pauseBlockedScenes = { "Creditos" };

    [Header("=== HUD PANELS (se ocultan al pausar) ===")]
    [Tooltip("Arrastra aqui los paneles de HUD que quieres ocultar al pausar (stamina, punteria, etc.)")]
    [SerializeField] private GameObject[] hudPanelsToHideOnPause;

    // Referencia al PauseMenuManager (se registra automaticamente)
    private PauseMenuManager _pauseMenuManager;

    public bool IsPaused => isPaused;
    public bool IsGameOver => gameOver;
    public PlayerStateMachine Player => player;

    /// <summary>
    /// Llamado por PauseMenuManager al inicializarse para registrar el panel.
    /// </summary>
    public void RegisterPausePanel(PauseMenuManager manager)
    {
        _pauseMenuManager = manager;
    }

    // Guardamos estado del player al pausar
    private Vector3 _savedVelocity;
    private bool _savedGravity;

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
            player = FindFirstSceneObject<PlayerStateMachine>();
        }
    }

    private void Start()
    {
        // Subscribe to events
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnPlayerRespawn += HandlePlayerRespawn;
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Crear panel de pausa si no hay uno asignado
        if (pausePanel == null)
        {
            CreatePausePanel();
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Se llama cada vez que se carga una escena nueva.
    /// Re-busca el player porque las referencias
    /// del DontDestroyOnLoad apuntan a objetos destruidos.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Resetear estado de pausa
        isPaused = false;
        Time.timeScale = 1f;

        // Re-buscar player
        player = FindFirstSceneObject<PlayerStateMachine>();
        _inputHandler = null; // se re-busca en Update

        // PauseMenuManager se registrara automaticamente en su Awake()
        // No necesitamos buscarlo aqui
    }

    // Referencia al input handler para saber tipo de mando
    private PlayerInputHandler _inputHandler;

    // Referencia al primer boton del panel de pausa para auto-seleccion con mando
    private GameObject _resumeButtonObj;

    private void Update()
    {
        if (IsPauseBlockedInCurrentScene())
            return;

        // Buscar input handler si no lo tenemos
        if (_inputHandler == null && player != null)
            _inputHandler = player.GetComponent<PlayerInputHandler>();

        // Detectar boton de pausa segun tipo de mando
        // ESC siempre funciona
        bool pausePressed = Input.GetKeyDown(KeyCode.Escape);

        if (_inputHandler != null)
        {
            var gp = _inputHandler.DetectedGamepad;
            // Xbox: Start = Button7 | PS: Options = Button9
            // IMPORTANTE: En PS4, Button7 = R2 (gancho), NO es Start!
            if (gp == PlayerInputHandler.GamepadType.Xbox)
                pausePressed |= Input.GetKeyDown(KeyCode.JoystickButton7);
            else if (gp == PlayerInputHandler.GamepadType.PlayStation)
                pausePressed |= Input.GetKeyDown(KeyCode.JoystickButton9);
        }

        // Solo pausar si NO estamos pausados ya
        // Para reanudar, el jugador DEBE usar el boton "Reanudar" del menu
        if (pausePressed && !isPaused)
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Se llama automaticamente cuando la ventana pierde/gana foco
    /// Alt-tab, minimizar, cambiar ventana = pausa automatica
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (IsPauseBlockedInCurrentScene())
            return;

        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pausa el juego: congela todo, muestra panel, activa cursor
    /// </summary>
    public void PauseGame()
    {
        if (IsPauseBlockedInCurrentScene()) return;
        if (isPaused) return;
        isPaused = true;

        // Congelar player si existe
        if (player != null && player.Rb != null)
        {
            _savedVelocity = player.Rb.velocity;
            _savedGravity = player.Rb.useGravity;

            player.Rb.velocity = Vector3.zero;
            player.Rb.useGravity = false;
            player.Rb.isKinematic = true;
        }

        // Pausar tiempo
        Time.timeScale = 0f;

        // Cursor libre
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ocultar paneles de HUD (stamina, punteria, etc.)
        SetHudPanelsActive(false);

        // Mostrar panel via PauseMenuManager
        if (_pauseMenuManager != null)
        {
            _pauseMenuManager.Show();
        }
        else if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    private bool IsPauseBlockedInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(currentScene) || pauseBlockedScenes == null)
            return false;

        for (int i = 0; i < pauseBlockedScenes.Length; i++)
        {
            if (string.Equals(currentScene, pauseBlockedScenes[i], System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reanuda el juego: restaura todo, oculta panel, bloquea cursor
    /// Se llama desde el boton "Reanudar" del panel
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;

        // Restaurar tiempo
        Time.timeScale = 1f;

        // Restaurar player
        if (player != null && player.Rb != null)
        {
            player.Rb.isKinematic = false;
            player.Rb.useGravity = _savedGravity;
            player.Rb.velocity = _savedVelocity;
        }

        isPaused = false;

        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ocultar panel via PauseMenuManager
        if (_pauseMenuManager != null)
            _pauseMenuManager.Hide();
        else if (pausePanel != null)
            pausePanel.SetActive(false);

        // Restaurar paneles de HUD
        SetHudPanelsActive(true);
    }

    /// <summary>
    /// Crea el panel de pausa por codigo con estilo visual
    /// </summary>
    private void CreatePausePanel()
    {
        // Usamos un canvas dedicado para pausa y evitar conflictos si hay multiples canvas en escena.
        Canvas canvas = GetOrCreatePauseCanvas();
        EnsureEventSystemExists();

        // Panel oscuro de fondo (overlay)
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = pausePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = pausePanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.7f); // fondo negro semitransparente

        // Contenedor central
        GameObject container = new GameObject("Container");
        container.transform.SetParent(pausePanel.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 300);
        containerRect.anchoredPosition = Vector2.zero;

        Image containerBg = container.AddComponent<Image>();
        containerBg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f); // panel oscuro

        // Titulo "PAUSA"
        GameObject titleObj = new GameObject("PauseTitle");
        titleObj.transform.SetParent(container.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(300, 60);
        titleRect.anchoredPosition = new Vector2(0, -50);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSA";
        titleText.font = GetDefaultFont();
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;

        // Boton "Reanudar"
        _resumeButtonObj = CreatePauseButton(container.transform, "ReanumeButton", "REANUDAR", new Vector2(0, -20), () => ResumeGame());

        // Boton "Salir"
        CreatePauseButton(container.transform, "ExitButton", "SALIR", new Vector2(0, -90), () => QuitGame());

        pausePanel.SetActive(false);
    }

    private Canvas GetOrCreatePauseCanvas()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].name == AutoPauseCanvasName)
                return canvases[i];
        }

        GameObject canvasObj = new GameObject(AutoPauseCanvasName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        return canvas;
    }

    private void EnsureEventSystemExists()
    {
        if (EventSystem.current != null)
            return;

        EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (systems != null && systems.Length > 0)
            return;

        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(eventSystemObj);
    }

    private T FindFirstSceneObject<T>() where T : Object
    {
        T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (found != null && found.Length > 0)
            return found[0];

        return null;
    }

    /// <summary>
    /// Helper para crear botones del panel de pausa
    /// </summary>
    private GameObject CreatePauseButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(250, 50);
        btnRect.anchoredPosition = position;

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.3f, 0.3f, 0.4f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;

        // Colores del boton
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.6f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);

        // Texto del boton
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = label;
        btnText.font = GetDefaultFont();
        btnText.fontSize = 24;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        return btnObj;
    }

    private void QuitGame()
    {
        Time.timeScale = 1f; // restaurar antes de salir

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Devuelve la fuente por defecto de Unity (compatible con todas las versiones)
    /// </summary>
    private Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    /// <summary>
    /// Activa o desactiva todos los paneles de HUD configurados.
    /// Se usa para ocultar stamina, punteria, etc. durante la pausa.
    /// </summary>
    private void SetHudPanelsActive(bool active)
    {
        if (hudPanelsToHideOnPause == null) return;

        for (int i = 0; i < hudPanelsToHideOnPause.Length; i++)
        {
            if (hudPanelsToHideOnPause[i] != null)
                hudPanelsToHideOnPause[i].SetActive(active);
        }
    }

    private void HandlePlayerDeath(Vector3 position)
    {
        Debug.Log($"Player died at {position}");
    }

    private void HandlePlayerRespawn(Vector3 position)
    {
        Debug.Log($"Player respawned at {position}");
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnPlayerRespawn -= HandlePlayerRespawn;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
