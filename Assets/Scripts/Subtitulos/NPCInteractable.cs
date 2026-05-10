using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Sistema de interacción con NPCs.
/// - Muestra el prompt "{E}" SOLO cuando el Player entra en el BoxCollider (isTrigger = true)
/// - Presionar E dentro de la zona muestra subtítulos secuencialmente
/// - Los subtítulos NO se activan solos al entrar (a menos que autoPlay = true explícitamente)
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 5f;

    [Header("Dialogue")]
    [SerializeField] private List<string> subtitles = new List<string>();

    [Tooltip("Si está activado, los subtítulos empiezan y avanzan solos al entrar (sin pulsar E). Dejar en FALSE para la zona del NPC normal.")]
    [SerializeField] private bool autoPlay = false;

    // Estado interno — solo se modifica via OnTriggerEnter/Exit para evitar falsos positivos
    private bool playerInsideTrigger = false;
    private bool entryFrameCooldown = false;   // evita que la E pulsada fuera se registre al entrar
    private int currentSubtitleIndex = 0;
    private float subtitleTimer = 0f;
    private bool showingSubtitle = false;
    private bool dialogueStarted = false;      // true en cuanto el jugador pulsa E por primera vez

    // Públicos para que otros scripts puedan consultarlos
    public bool isPlayerNear => playerInsideTrigger;
    public bool hasFinishedDialogue = false;

    [Tooltip("Si está en true, el NPC ignora al player completamente (se activa cuando el boss empieza a correr).")]
    public bool isLocked = false;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (promptE != null)      promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    // ─── TRIGGERS (sólo el Physics de Unity puede marcar al player como "dentro") ───

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
                if (promptE != null) promptE.SetActive(false);
                ShowSubtitle();
            }
            else
            {
                // Volver a mostrar el subtítulo actual directamente
                if (promptE != null) promptE.SetActive(false);
                if (subtitlePanel != null) subtitlePanel.SetActive(true);
                if (subtitleText != null) subtitleText.text = subtitles[currentSubtitleIndex];
                showingSubtitle = true;
                // Mantenemos subtitleTimer tal cual — el tiempo ya consumido se conserva
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
                if (promptE != null) promptE.SetActive(false);
                ShowSubtitle();
            }
            else
            {
                if (promptE != null) promptE.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled || isLocked) return;
        if (!other.CompareTag("Player")) return;

        playerInsideTrigger = false;
        entryFrameCooldown = false;

        if (promptE != null)       promptE.SetActive(false);
        if (subtitlePanel != null)  subtitlePanel.SetActive(false);

        // NO resetear currentSubtitleIndex ni subtitleTimer si el diálogo estaba en curso
        // Así al re-entrar (ej: después de pausa) se reanuda donde se dejó
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
        if (promptE != null)       promptE.SetActive(false);
        if (subtitlePanel != null)  subtitlePanel.SetActive(false);
        showingSubtitle = false;
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

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
        if (!autoPlay && Input.GetKeyDown(KeyCode.E))
        {
            if (subtitles == null || subtitles.Count == 0)
            {
                hasFinishedDialogue = true;
                HideSubtitle();
                return;
            }

            if (!showingSubtitle && currentSubtitleIndex < subtitles.Count)
            {
                ShowSubtitle();
            }
            else if (showingSubtitle && subtitleTimer > 0.5f)
            {
                // Skip al siguiente subtítulo si ya pasaron 0.5 s
                NextSubtitle();
            }
        }

        // ── Timer de subtítulos ──
        if (showingSubtitle)
        {
            subtitleTimer += Time.deltaTime;
            if (subtitleTimer >= subtitleDuration)
            {
                // Avanza solo al siguiente subtítulo (la E solo hace falta para empezar)
                NextSubtitle();
            }
        }
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private void ShowSubtitle()
    {
        if (currentSubtitleIndex >= subtitles.Count) return;

        dialogueStarted = true;   // marcar que el jugador ya ha empezado a leer
        if (promptE != null)       promptE.SetActive(false);
        if (subtitlePanel != null)  subtitlePanel.SetActive(true);
        subtitleText.text = subtitles[currentSubtitleIndex];
        subtitleTimer = 0f;
        showingSubtitle = true;
    }

    private void NextSubtitle()
    {
        currentSubtitleIndex++;
        if (currentSubtitleIndex < subtitles.Count)
        {
            subtitleText.text = subtitles[currentSubtitleIndex];
            subtitleTimer = 0f;
        }
        else
        {
            hasFinishedDialogue = true;
            HideSubtitle();
        }
    }

    private void HideSubtitle()
    {
        if (subtitlePanel != null)  subtitlePanel.SetActive(false);
        // Solo mostrar promptE al esconder si todavía hay subtítulos sin leer
        if (promptE != null && currentSubtitleIndex < subtitles.Count)
            promptE.SetActive(true);
        showingSubtitle = false;
        subtitleTimer = 0f;
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
        if (promptE != null)       promptE.SetActive(false);
        if (subtitlePanel != null)  subtitlePanel.SetActive(false);
    }
}
