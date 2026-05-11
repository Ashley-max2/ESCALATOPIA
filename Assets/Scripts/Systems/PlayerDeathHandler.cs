using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton que gestiona la muerte del jugador.
/// Detecta: caída >15m (vía GroundedState), tocar Water, quedarse sin stamina.
/// Muestra una pantalla de muerte con la causa y respawnea al pulsar cualquier tecla.
/// Colocar en el GameObject del Player.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    public static PlayerDeathHandler Instance { get; private set; }

    // ── Estado interno ──
    private PlayerStateMachine _psm;
    private StaminaSystem _stamina;
    private bool _isDead = false;
    private float _deathTimer;

    // ── UI procedural ──
    private GameObject _deathCanvasObj;
    private Text _causeText;

    // ───────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _psm = GetComponent<PlayerStateMachine>();
        _stamina = GetComponent<StaminaSystem>();
    }

    private void OnEnable()
    {
        if (_stamina != null)
            _stamina.OnStaminaDepleted += OnStaminaDepleted;
    }

    private void OnDisable()
    {
        if (_stamina != null)
            _stamina.OnStaminaDepleted -= OnStaminaDepleted;
    }

    private void Start()
    {
        CreateDeathScreenUI();
    }

    // ───────────────────────────────────────────────────────────────────────────
    //  DETECCIONES
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>Muerte por tocar agua.</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;
        if (other.CompareTag("Water"))
        {
            Die(DeathCause.Water);
        }
    }

    /// <summary>Muerte por quedarse sin stamina.</summary>
    private void OnStaminaDepleted()
    {
        if (_isDead) return;
        Die(DeathCause.Stamina);
    }

    // ───────────────────────────────────────────────────────────────────────────
    //  API PÚBLICA
    // ───────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mata al jugador con la causa indicada.
    /// Llamar desde PlayerGroundedState para caídas o desde aquí para agua/stamina.
    /// </summary>
    public void Die(DeathCause cause)
    {
        if (_isDead) return;
        _isDead = true;
        DeathManager.LastDeathCause = cause;

        // Transicionar a estado muerto (que llamará a ShowDeathScreen)
        if (_psm != null) _psm.Die();
    }

    /// <summary>Muestra la pantalla de muerte (llamado desde PlayerDeadState.Enter).</summary>
    public void ShowDeathScreen()
    {
        _deathTimer = 0f;

        if (_deathCanvasObj != null)
        {
            _deathCanvasObj.SetActive(true);
            UpdateCauseText();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    /// <summary>Oculta la pantalla y respawnea (llamado desde PlayerDeadState.Execute).</summary>
    public void HideAndRespawn()
    {
        if (_deathCanvasObj != null)
            _deathCanvasObj.SetActive(false);

        _isDead = false;
        DeathManager.LastDeathCause = DeathCause.None;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_psm != null) _psm.Respawn();
    }

    public bool IsDead => _isDead;

    // ───────────────────────────────────────────────────────────────────────────
    //  UI PROCEDURAL
    // ───────────────────────────────────────────────────────────────────────────

    private void UpdateCauseText()
    {
        if (_causeText == null) return;
        switch (DeathManager.LastDeathCause)
        {
            case DeathCause.Stamina: _causeText.text = "Yo también necesito descansar"; break;
            case DeathCause.Fall:    _causeText.text = "¿Tu saltarías de tan alto?";    break;
            case DeathCause.Water:   _causeText.text = "Glu glu glu gl...";             break;
            default:                 _causeText.text = "Has muerto.";                   break;
        }
    }

    private void CreateDeathScreenUI()
    {
        // ── Canvas ──
        _deathCanvasObj = new GameObject("DeathScreenCanvas_Auto");
        Canvas canvas = _deathCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = _deathCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _deathCanvasObj.AddComponent<GraphicRaycaster>();

        // ── Fondo negro semitransparente ──
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(_deathCanvasObj.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        bg.raycastTarget = false;
        RectTransform bgRt = bg.rectTransform;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // ── Título "HAS MUERTO" ──
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(_deathCanvasObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "HAS MUERTO";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 72;
        titleText.color = new Color(0.9f, 0.15f, 0.15f, 1f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.raycastTarget = false;
        RectTransform titleRt = titleText.rectTransform;
        titleRt.anchorMin = new Vector2(0, 0.55f);
        titleRt.anchorMax = new Vector2(1, 0.75f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        // ── Texto de causa ──
        GameObject causeObj = new GameObject("CauseText");
        causeObj.transform.SetParent(_deathCanvasObj.transform, false);
        _causeText = causeObj.AddComponent<Text>();
        _causeText.text = "";
        _causeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _causeText.fontSize = 36;
        _causeText.color = Color.white;
        _causeText.alignment = TextAnchor.MiddleCenter;
        _causeText.raycastTarget = false;
        RectTransform causeRt = _causeText.rectTransform;
        causeRt.anchorMin = new Vector2(0, 0.40f);
        causeRt.anchorMax = new Vector2(1, 0.55f);
        causeRt.offsetMin = Vector2.zero;
        causeRt.offsetMax = Vector2.zero;

        // ── Texto "Pulsa cualquier tecla" ──
        GameObject continueObj = new GameObject("ContinueText");
        continueObj.transform.SetParent(_deathCanvasObj.transform, false);
        Text continueText = continueObj.AddComponent<Text>();
        continueText.text = "Pulsa cualquier tecla para continuar";
        continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        continueText.fontSize = 24;
        continueText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        continueText.alignment = TextAnchor.MiddleCenter;
        continueText.raycastTarget = false;
        RectTransform contRt = continueText.rectTransform;
        contRt.anchorMin = new Vector2(0, 0.2f);
        contRt.anchorMax = new Vector2(1, 0.3f);
        contRt.offsetMin = Vector2.zero;
        contRt.offsetMax = Vector2.zero;

        _deathCanvasObj.SetActive(false);
    }
}
