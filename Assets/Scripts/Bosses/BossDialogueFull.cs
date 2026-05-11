using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Versión independiente para bosses de la UX de CharacterDialogue.
/// - Muestra prompt "E" cuando el jugador está cerca
/// - Tipo máquina de escribir
/// - Avanza con 'E' o auto-advance
/// - Dispara `onDialogueFinished` al terminar
/// No modifica `CharacterDialogue` y puede usarse en paralelo.
/// </summary>
public class BossDialogueFull : MonoBehaviour
{
    [Header("UI References")]
    public GameObject promptE;
    public GameObject subtitlePanel;
    public TextMeshProUGUI subtitleText;

    [Header("Behaviour")]
    public float typingSpeed = 0.04f;
    public float autoAdvanceDelay = 1.25f;

    [TextArea(3,8)]
    public List<string> lines = new List<string>();

    [Tooltip("Si está en true, bloquear interacción durante el diálogo")]
    public bool blockWhileShowing = true;

    public UnityEvent onDialogueFinished;

    private bool playerInside = false;
    private bool showing = false;
    private bool isTyping = false;
    private Coroutine typeRoutine;
    private int currentIndex = 0;
    private float autoTimer = 0f;
    private TMP_Text promptText;
    private string lastPromptLabel = string.Empty;

    void Start()
    {
        promptText = promptE != null ? promptE.GetComponentInChildren<TMP_Text>(true) : null;
        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }

    void Update()
    {
        UpdatePromptKeyLabel();

        if (!showing)
        {
            if (playerInside && promptE != null)
                promptE.SetActive(true);
            return;
        }

        if (!isTyping)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= autoAdvanceDelay)
            {
                Advance();
            }
        }

        if (InteractInput.PressedThisFrame())
        {
            if (!showing && playerInside)
            {
                StartDialogue();
            }
            else if (isTyping)
            {
                CompleteInstant();
            }
            else
            {
                Advance();
            }
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (!showing && promptE != null) promptE.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (promptE != null) promptE.SetActive(false);
        if (showing)
        {
            // cancelar diálogo si el jugador se va
            StopDialogue();
        }
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Count == 0) return;
        currentIndex = 0;
        showing = true;
        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(true);
        ShowLine();
    }

    private void ShowLine()
    {
        autoTimer = 0f;
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        string lineToShow = InteractInput.ReplaceInteractPlaceholder(lines[currentIndex]);
        typeRoutine = StartCoroutine(TypeText(lineToShow));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        subtitleText.text = string.Empty;
        for (int i = 0; i < text.Length; i++)
        {
            subtitleText.text += text[i];
            if (typingSpeed > 0f) yield return new WaitForSeconds(typingSpeed);
            else yield return null;
        }
        isTyping = false;
        typeRoutine = null;
    }

    private void CompleteInstant()
    {
        if (!isTyping) return;
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        subtitleText.text = InteractInput.ReplaceInteractPlaceholder(lines[currentIndex]);
        isTyping = false;
        autoTimer = 0f;
    }

    private void Advance()
    {
        if (isTyping)
        {
            CompleteInstant();
            return;
        }

        currentIndex++;
        autoTimer = 0f;
        if (currentIndex >= lines.Count)
        {
            FinishDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    private void FinishDialogue()
    {
        showing = false;
        isTyping = false;
        currentIndex = 0;
        autoTimer = 0f;
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        onDialogueFinished?.Invoke();
    }

    public void StopDialogue()
    {
        FinishDialogue();
    }
}
