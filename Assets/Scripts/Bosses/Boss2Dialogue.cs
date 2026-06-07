using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Diálogo del Boss2 (BossLevitante).
/// No depende de NPCInteractable ni de coleccionables.
///
/// Flujo:
///   1. Player entra en el trigger → aparece prompt [E]
///   2. Player pulsa E → diálogo Initial
///   3. Player pulsa E de nuevo → diálogo StartRace → hasFinishedDialogue = true → carrera empieza
///   4. Si el boss gana → ShowLoseDialogue() → al terminar → onLoseDialogueFinished
///   5. Si el player gana → ShowWinDialogue() → al terminar → onWinDialogueFinished
///
/// BossManager lee hasFinishedDialogue igual que con NPCInteractable.
/// </summary>
public class Boss2Dialogue : MonoBehaviour
{
    // ── UI ──────────────────────────────────────────────────────────
    [Header("UI")]
    [SerializeField] private GameObject   promptE;
    [SerializeField] private GameObject   subtitlePanel;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float        typingSpeed      = 0.04f;
    [SerializeField] private float        autoAdvanceDelay = 1.25f;
    [SerializeField] private bool         billboardPrompt  = true;

    // ── Diálogos ────────────────────────────────────────────────────
    [Header("Diálogos")]
    [SerializeField] private List<string> linesInitial    = new List<string>
    {
        "Así que has llegado hasta aquí...",
        "Si quieres pasar tendrás que ganarme en una carrera.",
        "Avísame cuando estés listo."
    };
    [SerializeField] private List<string> linesStartRace  = new List<string>
    {
        "¡Muy bien! Empecemos.",
        "3", "2", "1", "¡YA!"
    };
    [SerializeField] private List<string> linesLose       = new List<string>
    {
        "¿Eso es todo lo que tienes?",
        "Puedes intentarlo de nuevo cuando quieras."
    };
    [SerializeField] private List<string> linesWin        = new List<string>
    {
        "Increíble... te lo has ganado.",
        "Pasa."
    };

    // ── Referencias ─────────────────────────────────────────────────
    [Header("Referencias")]
    [SerializeField] private BossManager bossManager;

    // ── Eventos ─────────────────────────────────────────────────────
    [Header("Eventos")]
    public UnityEvent onLoseDialogueFinished;
    public UnityEvent onWinDialogueFinished;
    public UnityEvent onReadyToStartRace;

    // ── Estado público (BossManager lo lee) ─────────────────────────
    /// <summary>BossManager comprueba esta propiedad para saber cuándo arrancar la carrera.</summary>
    public bool hasFinishedDialogue { get; private set; } = false;

    /// <summary>Permite bloquear la interacción desde BossManager (misma API que NPCInteractable).</summary>
    public bool isLocked { get; set; } = false;

    // ── Estado interno ──────────────────────────────────────────────
    private enum Phase { Initial, StartRace, Lose, Win }

    private bool   _playerInside        = false;
    private bool   _entryCooldown       = false;
    private bool   _interactionEnabled  = true;
    private bool   _showingSubtitle     = false;
    private bool   _isTyping            = false;
    private bool   _hasPlayedInitial    = false;

    private Phase            _currentPhase;
    private List<string>     _currentLines;
    private int              _lineIndex;
    private float            _autoAdvanceTimer;
    private Coroutine        _typewriterCo;
    private TMP_Text         _promptTMP;
    private string           _lastPromptLabel = string.Empty;

    // ────────────────────────────────────────────────────────────────
    private void Start()
    {
        _promptTMP = promptE != null ? promptE.GetComponentInChildren<TMP_Text>(true) : null;
        if (promptE      != null) promptE.SetActive(false);
        if (subtitlePanel!= null) subtitlePanel.SetActive(false);
    }

    private void Update()
    {
        // Ocultar prompt si no hay player o no se puede interactuar
        if (!_showingSubtitle && (!_playerInside || !CanInteract()))
        {
            if (promptE != null) promptE.SetActive(false);
            return;
        }

        if (_entryCooldown) { _entryCooldown = false; return; }

        if (InteractInput.PressedThisFrame())
        {
            if (!_showingSubtitle)
                BeginNextPhase();
            else if (_isTyping)
                CompleteCurrentLine();
            else
                AdvanceLine();
        }

        if (_showingSubtitle && !_isTyping)
        {
            _autoAdvanceTimer += Time.deltaTime;
            if (_autoAdvanceTimer >= autoAdvanceDelay)
                AdvanceLine();
        }
    }

    private void LateUpdate()
    {
        UpdatePromptLabel();
        UpdateBillboard();
    }

    // ── Trigger ─────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside  = true;
        _entryCooldown = true;
        if (promptE != null && CanInteract())
            promptE.SetActive(!_showingSubtitle);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        _entryCooldown = false;
        if (promptE      != null) promptE.SetActive(false);
        if (subtitlePanel!= null) subtitlePanel.SetActive(false);
        _showingSubtitle = false;
        _isTyping        = false;
        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
    }

    // ── Lógica de fases ─────────────────────────────────────────────
    private Phase SelectNextPhase()
    {
        if (!_hasPlayedInitial) return Phase.Initial;
        return Phase.StartRace;
    }

    private void BeginNextPhase() => BeginPhase(SelectNextPhase());

    private void BeginPhase(Phase phase)
    {
        List<string> lines = phase switch
        {
            Phase.Initial   => linesInitial,
            Phase.StartRace => linesStartRace,
            Phase.Lose      => linesLose,
            Phase.Win       => linesWin,
            _               => null
        };

        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning($"[Boss2Dialogue] Sin líneas para la fase {phase}.");
            return;
        }

        _currentPhase     = phase;
        _currentLines     = lines;
        _lineIndex        = 0;
        _showingSubtitle  = true;
        _autoAdvanceTimer = 0f;

        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
        if (promptE       != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (subtitleText == null || _currentLines == null || _lineIndex >= _currentLines.Count) return;
        _autoAdvanceTimer = 0f;
        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
        string line = InteractInput.ReplaceInteractPlaceholder(_currentLines[_lineIndex]);
        _typewriterCo = StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(string line)
    {
        _isTyping = true;
        subtitleText.text = string.Empty;
        foreach (char c in line)
        {
            subtitleText.text += c;
            if (typingSpeed > 0f) yield return new WaitForSeconds(typingSpeed);
            else yield return null;
        }
        _isTyping     = false;
        _typewriterCo = null;
    }

    private void CompleteCurrentLine()
    {
        if (!_isTyping || _currentLines == null || _lineIndex >= _currentLines.Count) return;
        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
        subtitleText.text = InteractInput.ReplaceInteractPlaceholder(_currentLines[_lineIndex]);
        _isTyping         = false;
        _autoAdvanceTimer = 0f;
    }

    private void AdvanceLine()
    {
        if (!_showingSubtitle) return;
        _lineIndex++;
        _autoAdvanceTimer = 0f;
        if (_currentLines == null || _lineIndex >= _currentLines.Count)
            FinishPhase();
        else
            ShowCurrentLine();
    }

    private void FinishPhase()
    {
        _showingSubtitle  = false;
        _isTyping         = false;
        _autoAdvanceTimer = 0f;
        if (_typewriterCo != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (promptE != null && _playerInside && CanInteract()) promptE.SetActive(true);

        switch (_currentPhase)
        {
            case Phase.Initial:
                _hasPlayedInitial = true;
                break;

            case Phase.StartRace:
                hasFinishedDialogue = true;   // BossManager arranca la carrera
                SetInteractionEnabled(false);
                onReadyToStartRace?.Invoke();
                break;

            case Phase.Lose:
                onLoseDialogueFinished?.Invoke();
                break;

            case Phase.Win:
                onWinDialogueFinished?.Invoke();
                break;
        }
        _currentLines = null;
    }

    // ── API pública (misma que CharacterDialogue) ────────────────────
    public void ShowLoseDialogue()
    {
        if (_showingSubtitle) return;
        BeginPhase(Phase.Lose);
    }

    public void ShowWinDialogue()
    {
        if (_showingSubtitle) return;
        BeginPhase(Phase.Win);
    }

    /// <summary>Permite reintentar la carrera tras perder (llamado desde BossRaceManager).</summary>
    public void AllowRaceRetry()
    {
        hasFinishedDialogue = false;
        // NO reseteamos _hasPlayedInitial: el intro no se repite cada vez
        Debug.Log("[Boss2Dialogue] Reintento de carrera disponible.");
    }

    public void SetInteractionEnabled(bool value)
    {
        _interactionEnabled = value;
        if (!value && promptE != null) promptE.SetActive(false);
    }

    public void ForceReset()
    {
        hasFinishedDialogue = false;
        _hasPlayedInitial   = false;
        _showingSubtitle    = false;
        _isTyping           = false;
        _currentLines       = null;
        _lineIndex          = 0;
        _autoAdvanceTimer   = 0f;
        if (_typewriterCo   != null) { StopCoroutine(_typewriterCo); _typewriterCo = null; }
        if (subtitlePanel   != null) subtitlePanel.SetActive(false);
        if (promptE         != null) promptE.SetActive(false);
    }

    // ── Helpers ─────────────────────────────────────────────────────
    private bool CanInteract() => _interactionEnabled && !isLocked && !IsRaceActive();

    private bool IsRaceActive() => bossManager != null && bossManager.raceStarted;

    private void UpdatePromptLabel()
    {
        if (_promptTMP == null) return;
        string label = InteractInput.GetBracketedDisplayKey();
        if (label == _lastPromptLabel) return;
        _promptTMP.text  = label;
        _lastPromptLabel = label;
    }

    private void UpdateBillboard()
    {
        if (!billboardPrompt || promptE == null || !promptE.activeSelf) return;
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam == null) return;
        Vector3 dir = cam.position - promptE.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            promptE.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }
}
