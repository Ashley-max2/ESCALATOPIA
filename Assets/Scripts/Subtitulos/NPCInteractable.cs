/*using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Sistema simple de interacción con NPCs.
/// - Muestra "{E}" cuando el Player se acerca
/// - Presionar E muestra subtítulos secuencialmente
/// - Sin modificar scripts existentes del proyecto
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 3f;

    [Header("Dialogue")]
    [SerializeField] private List<string> subtitles = new List<string>();

    private int currentSubtitleIndex = 0;
    private bool isPlayerNear = false;
    private float subtitleTimer = 0f;
    private bool showingSubtitle = false;

    /// <summary>
    /// Se activa cuando el Player entra en el rango del NPC
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            promptE.SetActive(true);
            currentSubtitleIndex = 0;
            subtitleTimer = 0f;
            showingSubtitle = false;
        }
    }

    /// <summary>
    /// Se activa cuando el Player sale del rango del NPC
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptE.SetActive(false);
            subtitlePanel.SetActive(false);
            currentSubtitleIndex = 0;
            showingSubtitle = false;
        }
    }

    private void Update()
    {
        if (!isPlayerNear) return;

        // Escuchar tecla E para interactuar
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!showingSubtitle && currentSubtitleIndex < subtitles.Count)
            {
                ShowSubtitle();
            }
            else if (showingSubtitle && subtitleTimer > 0.5f) // Permite skip si pasó 0.5s
            {
                NextSubtitle();
            }
        }

        // Actualizar timer para cambiar automáticamente de subtítulo
        if (showingSubtitle)
        {
            subtitleTimer += Time.deltaTime;
            if (subtitleTimer >= subtitleDuration)
            {
                HideSubtitle();
            }
        }
    }

    /// <summary>
    /// Muestra el subtítulo actual
    /// </summary>
    private void ShowSubtitle()
    {
        if (currentSubtitleIndex >= subtitles.Count)
            return;

        promptE.SetActive(false);
        subtitlePanel.SetActive(true);
        subtitleText.text = subtitles[currentSubtitleIndex];
        subtitleTimer = 0f;
        showingSubtitle = true;
    }

    /// <summary>
    /// Avanza al siguiente subtítulo
    /// </summary>
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
            HideSubtitle();
        }
    }

    /// <summary>
    /// Oculta el panel de subtítulos y vuelve a mostrar el {E}
    /// </summary>
    private void HideSubtitle()
    {
        subtitlePanel.SetActive(false);
        promptE.SetActive(true);
        showingSubtitle = false;
        subtitleTimer = 0f;
    }
}*/

using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class NPCInteractable : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptE;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 3f;

    [Header("Dialogue")]
    [SerializeField] private List<string> subtitles = new List<string>();

    private int currentSubtitleIndex = 0;
    public bool isPlayerNear = false;
    private float subtitleTimer = 0f;
    private bool showingSubtitle = false;
    public bool hasFinishedDialogue = false;
    
    void Start()
    {
        promptE.SetActive(false);
        subtitlePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNear = true;
            promptE.SetActive(true);
            currentSubtitleIndex = 0;
            subtitleTimer = 0f;
            showingSubtitle = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerNear = false;
            promptE.SetActive(false);
            subtitlePanel.SetActive(false);
            currentSubtitleIndex = 0;
            showingSubtitle = false;
        }
    }

    private void OnDisable()
    {
        isPlayerNear = false;
        if (promptE != null) promptE.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        showingSubtitle = false;
    }

    public void Update()
    {
        if (!isPlayerNear) return;

        // Escuchar tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (subtitles == null || subtitles.Count == 0)
            {
                // Si no hay subtítulos, consideramos que la charla termina nada más darle a E
                hasFinishedDialogue = true;
                HideSubtitle();
                return;
            }

            if (!showingSubtitle && currentSubtitleIndex < subtitles.Count)
            {
                ShowSubtitle();
            }
            else if (showingSubtitle && subtitleTimer > 0.5f) // Permite skip después de 0.5s
            {
                NextSubtitle();
            }
        }

        // Actualizar timer
        if (showingSubtitle)
        {
            subtitleTimer += Time.deltaTime;
            if (subtitleTimer >= subtitleDuration)
            {
                HideSubtitle();
            }
        }
    }

    private void ShowSubtitle()
    {
        if (currentSubtitleIndex >= subtitles.Count)
            return;

        promptE.SetActive(false);
        subtitlePanel.SetActive(true);
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
        subtitlePanel.SetActive(false);
        promptE.SetActive(true);
        showingSubtitle = false;
        subtitleTimer = 0f;
    }
}