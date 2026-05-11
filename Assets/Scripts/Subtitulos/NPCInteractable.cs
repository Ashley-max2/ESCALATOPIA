using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sistema de interacción con NPCs.
/// - Muestra el prompt "{E}" SOLO cuando el Player entra en el BoxCollider (isTrigger = true)
/// - Presionar E dentro de la zona muestra subtítulos secuencialmente CON MÁQUINA DE ESCRIBIR
/// - Los subtítulos NO se activan solos al entrar (a menos que autoPlay = true explícitamente)
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 5f;

    [Header("Typewriter Effect")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Prompt Billboard")]
    [SerializeField] private bool billboardPrompt = true;
    [SerializeField] private bool billboardOnlyOnY = true;

    [Header("Dialogue")]
    [SerializeField] private List<string> subtitles = new List<string>();

    [Tooltip("Si está activado, los subtítulos empiezan y avanzan solos al entrar (sin pulsar E). Dejar en FALSE para la zona del NPC normal.")]
    [SerializeField] private bool autoPlay = false;
    [Tooltip("Si se pulsa E para empezar, activar modo auto-advance (las líneas avanzan solas).")]
    [SerializeField] private bool startWithAutoAdvanceOnPress = true;
    [SerializeField] private float autoAdvanceDelay = 1.25f;

    // 🔥 AÑADIDO
    [Header("Mission Change (optional)")]
    [SerializeField] private MissionChangeManager missionChangeManager;

    // Estado interno — solo se modifica via OnTriggerEnter/Exit para evitar falsos positivos
    private bool playerInsideTrigger = false;
    private bool entryFrameCooldown = false;   // evita que la E pulsada fuera se registre al entrar
    private int currentSubtitleIndex = 0;
    private float subtitleTimer = 0f;
    private bool showingSubtitle = false;
    private bool dialogueStarted = false;      // true en cuanto el jugador pulsa E por primera vez
    private bool isTyping = false;             // máquina de escribir en progreso
    private Coroutine typewriterCoroutine;     // para detener/saltar la escritura
    private bool autoAdvanceMode = false;
    // Player freeze state (para bloquear movimiento durante diálogos)
    private Rigidbody _playerRb = null;
    private bool _savedPlayerIsKinematic = false;
    private bool _savedPlayerUseGravity = true;
    private Vector3 _savedPlayerVelocity = Vector3.zero;
    private bool _playerFrozen = false;

    // Públicos para que otros scripts puedan consultarlos
    public bool isPlayerNear => playerInsideTrigger;
    public bool hasFinishedDialogue = false;

    [Tooltip("Si está en true, el NPC ignora al player completamente (se activa cuando el boss empieza a correr).")]
    public bool isLocked = false;

    private TMP_Text promptText;
    private string lastPromptLabel = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        promptText = promptE != null ? promptE.GetComponentInChildren<TMP_Text>(true) : null;

        if (promptE != null)
            promptE.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    // ─── TRIGGERS ─────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // Si el componente está desactivado o bloqueado, ignorar por completo
        if (!enabled || isLocked) return;
        if (!other.CompareTag("Player")) return;

        playerInsideTrigger = true;

        // Cooldown de 1 frame: ignora la E que se pudiera haber pulsado ANTES de entrar
        entryFrameCooldown = true;

        // ¿El jugador ya había empezado a leer y salió (ej: menú de pausa)?
        if (dialogueStarted && !hasFinishedDialogue)
        {
            // Reanudar: mostrar el subtítulo en el que se quedó
            if (autoPlay)
            {
                if (promptE != null)
                    promptE.SetActive(false);

                ShowSubtitle();
            }
            else
            {
                // Volver a mostrar el subtítulo actual directamente
                if (promptE != null)
                    promptE.SetActive(false);

                if (subtitlePanel != null)
                    subtitlePanel.SetActive(true);

                if (subtitleText != null)
                    subtitleText.text = InteractInput.ReplaceInteractPlaceholder(subtitles[currentSubtitleIndex]);

                showingSubtitle = true;

                // Mantenemos subtitleTimer tal cual
            }
        }
        else
        {
            // Primera vez (o diálogo terminado): empezar desde el principio
            currentSubtitleIndex = 0;
            subtitleTimer = 0f;
            showingSubtitle = false;
            hasFinishedDialogue = false;
            dialogueStarted = false;

            if (autoPlay)
            {
                if (promptE != null)
                    promptE.SetActive(false);

                ShowSubtitle();
            }
            else
            {
                if (promptE != null)
                    promptE.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled || isLocked) return;
        if (!other.CompareTag("Player")) return;

        playerInsideTrigger = false;
        entryFrameCooldown = false;

        if (promptE != null)
            promptE.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        // NO resetear currentSubtitleIndex ni subtitleTimer si el diálogo estaba en curso
        if (!dialogueStarted || hasFinishedDialogue)
        {
            currentSubtitleIndex = 0;
            subtitleTimer = 0f;
        }

        showingSubtitle = false;
    }

    private void OnDisable()
    {
        playerInsideTrigger = false;
        entryFrameCooldown = false;
        isTyping = false;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (promptE != null)
            promptE.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        showingSubtitle = false;
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────

    void Update()
    {
        // Si el player no está dentro del trigger según Unity Physics → nada que hacer
        if (!playerInsideTrigger) return;

        // Consumir el cooldown del frame de entrada antes de escuchar la E
        if (entryFrameCooldown)
        {
            entryFrameCooldown = false;
            return;
        }

        // ── Tecla E (solo si NO es autoPlay) ──
        if (!autoPlay && InteractInput.PressedThisFrame())
        {
            // 🔥 AÑADIDO
            if (!dialogueStarted && missionChangeManager != null)
            {
                missionChangeManager.ChangeMission();
            }

            if (subtitles == null || subtitles.Count == 0)
            {
                hasFinishedDialogue = true;
                HideSubtitle();
                return;
            }

            // E para iniciar el diálogo (si no ha empezado)
            if (!dialogueStarted && !showingSubtitle)
            {
                // Si queremos que pulsar E ponga el diálogo en modo auto-advance, activarlo
                if (startWithAutoAdvanceOnPress)
                    autoAdvanceMode = true;

                ShowSubtitle();
                return;
            }

            // E mientras está escribiendo = completar línea instantáneamente
            if (isTyping)
            {
                CompleteTyping();
                return;
            }

            // E después de escribir = skip a la siguiente (solo si pasaron 0.5s)
            if (showingSubtitle && !isTyping && subtitleTimer > 0.5f)
            {
                NextSubtitle();
            }
        }

        // ── Auto-avance tras subtitleDuration (sin necesidad de E) ──
        if (showingSubtitle && !isTyping)
        {
            subtitleTimer += Time.deltaTime;

            float delay = autoAdvanceMode ? autoAdvanceDelay : subtitleDuration;
            if (subtitleTimer >= delay)
            {
                NextSubtitle();
            }
        }
    }

    private void LateUpdate()
    {
        UpdatePromptKeyLabel();
        UpdatePromptBillboard();
    }

    private void UpdatePromptKeyLabel()
    {
        if (promptText == null) return;

        string label = InteractInput.GetBracketedDisplayKey();

        if (label != lastPromptLabel)
        {
            promptText.text = label;
            lastPromptLabel = label;
        }
    }

    private void UpdatePromptBillboard()
    {
        if (!billboardPrompt || promptE == null || !promptE.activeSelf)
            return;

        Transform targetTransform = null;

        if (Camera.main != null)
            targetTransform = Camera.main.transform;
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                targetTransform = player.transform;
        }

        if (targetTransform == null)
            return;

        Vector3 direction = targetTransform.position - promptE.transform.position;

        if (billboardOnlyOnY)
            direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        promptE.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────

    private void ShowSubtitle()
    {
        if (currentSubtitleIndex >= subtitles.Count)
            return;

        // marcar que el diálogo se ha iniciado por input o por autoplay
        dialogueStarted = true;

        dialogueStarted = true;

        if (promptE != null)
            promptE.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        subtitleTimer = 0f;
        showingSubtitle = true;

        // Detener corrutina previa si existe
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        // Iniciar máquina de escribir
        string lineToShow = InteractInput.ReplaceInteractPlaceholder(subtitles[currentSubtitleIndex]);
        typewriterCoroutine = StartCoroutine(TypeText(lineToShow));

        // Bloquear movimiento del jugador mientras mostramos subtítulos
        FreezePlayerMovement();
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        subtitleText.text = string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            subtitleText.text += text[i];

            if (typingSpeed > 0f)
                yield return new WaitForSeconds(typingSpeed);
            else
                yield return null;
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    private void CompleteTyping()
    {
        if (!isTyping) return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        subtitleText.text = InteractInput.ReplaceInteractPlaceholder(subtitles[currentSubtitleIndex]);

        isTyping = false;
        subtitleTimer = 0f;
    }

    private void NextSubtitle()
    {
        currentSubtitleIndex++;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;

        if (currentSubtitleIndex < subtitles.Count)
        {
            ShowSubtitle();
        }
        else
        {
            hasFinishedDialogue = true;
            HideSubtitle();
            autoAdvanceMode = false;
        }
    }

    private void HideSubtitle()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        isTyping = false;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        // Solo mostrar promptE al esconder si todavía hay subtítulos sin leer
        if (promptE != null && currentSubtitleIndex < subtitles.Count)
            promptE.SetActive(true);

        showingSubtitle = false;
        subtitleTimer = 0f;
        autoAdvanceMode = false;

        // Restaurar movimiento del jugador
        UnfreezePlayerMovement();
    }

    private void FreezePlayerMovement()
    {
        if (_playerFrozen) return;

        // Intentar obtener player desde GameManager si existe, si no, buscar en escena
        PlayerStateMachine psm = null;
        if (GameManager.Instance != null)
            psm = GameManager.Instance.Player;

        if (psm == null)
            psm = Object.FindObjectOfType<PlayerStateMachine>();

        if (psm == null || psm.Rb == null) return;

        _playerRb = psm.Rb;
        _savedPlayerIsKinematic = _playerRb.isKinematic;
        _savedPlayerUseGravity = _playerRb.useGravity;
        _savedPlayerVelocity = _playerRb.velocity;

        _playerRb.velocity = Vector3.zero;
        _playerRb.useGravity = false;
        _playerRb.isKinematic = true;

        _playerFrozen = true;
    }

    private void UnfreezePlayerMovement()
    {
        if (!_playerFrozen) return;
        if (_playerRb == null) { _playerFrozen = false; return; }

        _playerRb.isKinematic = _savedPlayerIsKinematic;
        _playerRb.useGravity = _savedPlayerUseGravity;
        _playerRb.velocity = _savedPlayerVelocity;

        _playerFrozen = false;
        _playerRb = null;
    }

    /// <summary>
    /// Fuerza el reseteo del estado del NPC (llamar desde BossManager u otros scripts externos).
    /// </summary>
    public void ForceReset()
    {
        playerInsideTrigger = false;
        entryFrameCooldown = false;
        showingSubtitle = false;
        dialogueStarted = false;
        subtitleTimer = 0f;
        currentSubtitleIndex = 0;
        hasFinishedDialogue = false;
        isTyping = false;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (promptE != null)
            promptE.SetActive(false);

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);

        // Asegurar restauración del player si estaba congelado
        UnfreezePlayerMovement();
    }
}
