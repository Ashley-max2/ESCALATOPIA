using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public enum DialoguePhaseType
{
    Initial,
    RepeatBeforeCollect,
    AfterCollect,
    StartRace,
    Lose,
    Win
}

[System.Serializable]
public class DialoguePhase
{
    public DialoguePhaseType phaseType;
    public string phaseName = "Fase de diálogo";
    public List<string> lines = new List<string>();
}

/// <summary>
/// Controla el diálogo secuencial de un NPC con varias fases de conversación.
///
/// - Muestra un prompt "E" cuando el jugador está cerca.
/// - Avanza cada frase solamente al pulsar E o clic izquierdo.
/// - Reproduce un efecto de máquina de escribir en cada línea.
/// - El diálogo cambia dependiendo de si el objeto ha sido recogido.
/// - Permite mostrar diálogos de victoria/derrota desde código.
/// </summary>
public class CharacterDialogue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Dialogue Phases")]
    [SerializeField] private List<DialoguePhase> dialoguePhases = new List<DialoguePhase>
    {
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.Initial,
            phaseName = "Inicio",
            lines = new List<string>
            {
                "Hola joven, soy Anas, el niño de la montaña",
                "Para poder seguir a la siguiente zona debes demostrarme que eres digno de escalar la montaña",
                "He perdido un objeto en lo alto de este lugar (Foto mostrando el objeto)",
                "Si lo consigues y luego me ganas en una carrera te daré el objeto."
            }
        },
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.RepeatBeforeCollect,
            phaseName = "Repetición antes de recoger",
            lines = new List<string>
            {
                "¿Qué esperas? El objeto sigue ahí arriba",
                "No molestes si no vas a por el objeto"
            }
        },
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.AfterCollect,
            phaseName = "Después de conseguir el objeto",
            lines = new List<string>
            {
                "Eres digno de poder escalar la montaña, pero primero debes ganar en esta carrera",
                "Te aseguro que no podrás avanzar más en la montaña sin antes conseguir este objeto",
                "Así que más te vale ganarme",
                "Avísame cuando estés listo"
            }
        },
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.StartRace,
            phaseName = "Preparar salida de carrera",
            lines = new List<string>
            {
                "Perfecto, empecemos entonces",
                "3",
                "2",
                "1",
                "VAMOS!"
            }
        },
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.Lose,
            phaseName = "Derrota",
            lines = new List<string>
            {
                "Has perdido jaja",
                "Puedes seguir intentándolo hasta que consigas ganar…",
                "Así no me aburro"
            }
        },
        new DialoguePhase
        {
            phaseType = DialoguePhaseType.Win,
            phaseName = "Victoria",
            lines = new List<string>
            {
                "Oh, bien hecho",
                "Te lo has ganado",
                "Aquí tienes el objeto"
            }
        }
    };

    [Header("References")]
    [Tooltip("Objeto del que depende el diálogo. Si no se asigna, el NPC usará solo las fases de conversación inicial/repetir.")]
    [SerializeField] private CollectibleItem collectibleItem;

    [Tooltip("Controlador de IA del Boss para iniciar la carrera cuando se termine el diálogo de preparación.")]
    [SerializeField] private BossAIController bossAIController;

    [Tooltip("Manager de carrera opcional para bloquear interacción mientras la carrera está activa.")]
    [SerializeField] private BossManager bossManager;

    [Tooltip("Si está activo, se bloquea la opción de hablar mientras la carrera está en progreso.")]
    [SerializeField] private bool disableDuringRace = true;

    [Tooltip("Evento que se dispara cuando termina el diálogo de inicio de carrera.")]
    public UnityEvent onReadyToStartRace;

    private bool playerInsideTrigger = false;
    private bool entryFrameCooldown = false;
    private bool showingSubtitle = false;
    private bool isTyping = false;
    private bool interactionEnabled = true;
    private Coroutine typewriterCoroutine;
    private int currentLineIndex = 0;
    private DialoguePhaseType currentPhase;
    private List<string> currentLines;

    private bool hasInitialIntroPlayed = false;
    private bool hasAfterCollectPlayed = false;
    private bool hasRaceStartPlayed = false;

    private void Start()
    {
        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerInsideTrigger || !CanInteract())
        {
            if (promptE != null)
                promptE.SetActive(false);
            return;
        }

        if (entryFrameCooldown)
        {
            entryFrameCooldown = false;
            return;
        }

        if (AdvancePressed())
        {
            if (!showingSubtitle)
            {
                BeginPhaseDialogue(SelectActivePhase());
            }
            else if (isTyping)
            {
                CompleteCurrentLine();
            }
            else
            {
                AdvanceSubtitle();
            }
        }
    }

    private bool CanInteract()
    {
        return interactionEnabled && !IsRaceActive();
    }

    private bool IsRaceActive()
    {
        return disableDuringRace && bossManager != null && bossManager.raceStarted;
    }

    private bool AdvancePressed()
    {
        return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
        if (!interactionEnabled && promptE != null)
        {
            promptE.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInsideTrigger = true;
        entryFrameCooldown = true;

        if (!CanInteract())
        {
            if (promptE != null)
                promptE.SetActive(false);
            return;
        }

        if (promptE != null)
            promptE.SetActive(!showingSubtitle);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInsideTrigger = false;
        entryFrameCooldown = false;
        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        showingSubtitle = false;
        isTyping = false;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    private DialoguePhaseType SelectActivePhase()
    {
        bool collected = collectibleItem != null && collectibleItem.IsCollected;

        if (collected)
        {
            if (!hasAfterCollectPlayed)
                return DialoguePhaseType.AfterCollect;

            if (!hasRaceStartPlayed)
                return DialoguePhaseType.StartRace;

            return DialoguePhaseType.StartRace;
        }

        if (!hasInitialIntroPlayed)
            return DialoguePhaseType.Initial;

        return DialoguePhaseType.RepeatBeforeCollect;
    }

    private DialoguePhase GetPhase(DialoguePhaseType phaseType)
    {
        return dialoguePhases.Find(p => p.phaseType == phaseType);
    }

    private void BeginPhaseDialogue(DialoguePhaseType phaseType)
    {
        DialoguePhase phase = GetPhase(phaseType);
        if (phase == null || phase.lines == null || phase.lines.Count == 0)
        {
            Debug.LogWarning($"CharacterDialogue: falta la fase de diálogo {phaseType} o no tiene líneas.");
            return;
        }

        currentPhase = phaseType;
        currentLines = phase.lines;
        currentLineIndex = 0;
        showingSubtitle = true;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (subtitleText == null || currentLines == null || currentLineIndex >= currentLines.Count)
            return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        typewriterCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        subtitleText.text = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            subtitleText.text += line[i];
            if (typingSpeed > 0f)
                yield return new WaitForSeconds(typingSpeed);
            else
                yield return null;
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    private void CompleteCurrentLine()
    {
        if (!isTyping || currentLines == null || currentLineIndex >= currentLines.Count)
            return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        subtitleText.text = currentLines[currentLineIndex];
        isTyping = false;
    }

    private void AdvanceSubtitle()
    {
        if (!showingSubtitle) return;

        currentLineIndex++;
        if (currentLines == null || currentLineIndex >= currentLines.Count)
        {
            FinishCurrentPhase();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void FinishCurrentPhase()
    {
        showingSubtitle = false;
        isTyping = false;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (promptE != null && playerInsideTrigger) promptE.SetActive(true);

        switch (currentPhase)
        {
            case DialoguePhaseType.Initial:
                hasInitialIntroPlayed = true;
                break;
            case DialoguePhaseType.AfterCollect:
                hasAfterCollectPlayed = true;
                break;
            case DialoguePhaseType.StartRace:
                hasRaceStartPlayed = true;
                onReadyToStartRace?.Invoke();
                // Activar el BossAIController para iniciar la carrera
                if (bossAIController != null)
                {
                    bossAIController.enabled = true;
                    Debug.Log("[CharacterDialogue] BossAIController activado. Carrera iniciada.");
                }
                SetInteractionEnabled(false);
                break;
            default:
                break;
        }

        currentLines = null;
    }

    public void ShowLoseDialogue()
    {
        if (showingSubtitle) return;
        BeginPhaseDialogue(DialoguePhaseType.Lose);
    }

    public void ShowWinDialogue()
    {
        if (showingSubtitle) return;
        BeginPhaseDialogue(DialoguePhaseType.Win);
    }

    public void ResetDialogueState()
    {
        hasInitialIntroPlayed = false;
        hasAfterCollectPlayed = false;
        hasRaceStartPlayed = false;
        showingSubtitle = false;
        isTyping = false;
        currentLines = null;
        currentLineIndex = 0;
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (promptE != null) promptE.SetActive(false);
    }

    /// <summary>
    /// Permite intentar la carrera de nuevo después de perder.
    /// Resetea solo los flags de la carrera, no los anteriores.
    /// </summary>
    public void AllowRaceRetry()
    {
        hasRaceStartPlayed = false;
        Debug.Log("[CharacterDialogue] Carrera disponible para reintentar.");
    }
}
