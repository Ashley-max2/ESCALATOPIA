using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Muestra subtítulos de victoria o derrota al terminar la carrera contra el Boss.
///
/// VICTORIA (player llega primero al diamante):
///   - Muestra los subtítulos uno tras otro automáticamente y desaparecen solos.
///
/// DERROTA (boss llega primero):
///   - Muestra los subtítulos automáticamente uno tras otro.
///   - Al acabar el último, el HazardTeleporter se encarga del fade + teletransporte
///     a la fogata (usa el sistema ya existente en la escena).
/// </summary>
public class RaceResultSubtitles : MonoBehaviour
{
    // ── UI ──────────────────────────────────────────────────────────────────
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private float subtitleDuration = 5f;

    // ── SUBTÍTULOS DE VICTORIA ───────────────────────────────────────────────
    [Header("Victoria (Player gana)")]
    [SerializeField] private List<string> victorySubtitles = new List<string>()
    {
        "¡Lo has conseguido!",
        "Has llegado primero al diamante.",
        "¡Eres el ganador!"
    };

    // ── SUBTÍTULOS DE DERROTA ────────────────────────────────────────────────
    [Header("Derrota (Boss gana)")]
    [SerializeField] private List<string> defeatSubtitles = new List<string>()
    {
        "Has perdido...",
        "El boss ha llegado antes que tú.",
        "Volviendo a la fogata..."
    };

    [Header("Derrota – Respawn")]
    [Tooltip("El HazardTeleporter que se encargará del fade + teletransporte a la fogata.\n" +
             "Asegúrate de que su campo Checkpoint apunte a la fogata.")]
    [SerializeField] private HazardTeleporter campfireTeleporter;

    [Tooltip("El Transform del Player (se rellena automáticamente por tag 'Player').")]
    [SerializeField] private Transform playerTransform;

    // ── ESTADO INTERNO ───────────────────────────────────────────────────────
    private bool isRunning = false;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);

        if (playerTransform == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) playerTransform = playerGO.transform;
        }

        // DEBUG: confirmar que el script está vivo y con referencias
        Debug.Log($"[RaceResultSubtitles] Start OK. subtitlePanel={(subtitlePanel != null ? subtitlePanel.name : "NULL")}, subtitleText={(subtitleText != null ? "OK" : "NULL")}");
    }

    // ─── API PÚBLICA ─────────────────────────────────────────────────────────

    /// <summary>Llamar cuando el Player gana (toca el diamante primero).</summary>
    public void ShowVictory()
    {
        Debug.Log($"[RaceResultSubtitles] ShowVictory() llamado. isRunning={isRunning}, gameObject.activeInHierarchy={gameObject.activeInHierarchy}");
        if (isRunning) return;
        StartCoroutine(PlayAutoSubtitles(victorySubtitles, onFinished: null));
    }

    /// <summary>Llamar cuando el Boss gana.
    /// Muestra los subtítulos de derrota y al terminar lanza el HazardTeleporter.</summary>
    public void ShowDefeat()
    {
        if (isRunning) return;
        StartCoroutine(PlayAutoSubtitles(defeatSubtitles, onFinished: TeleportViaCampfireTeleporter));
    }

    // ─── CORRUTINA ────────────────────────────────────────────────────────────

    private IEnumerator PlayAutoSubtitles(List<string> lines, System.Action onFinished)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("[RaceResultSubtitles] La lista de subtítulos está vacía.");
            onFinished?.Invoke();
            yield break;
        }

        Debug.Log($"[RaceResultSubtitles] Iniciando {lines.Count} subtítulos.");
        isRunning = true;
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
            Debug.Log("[RaceResultSubtitles] subtitlePanel activado.");
        }
        else
        {
            Debug.LogError("[RaceResultSubtitles] subtitlePanel es NULL. Asígnalo en el Inspector.");
        }

        foreach (string line in lines)
        {
            Debug.Log($"[RaceResultSubtitles] Mostrando: '{line}'");
            if (subtitleText != null) subtitleText.text = line;
            else Debug.LogError("[RaceResultSubtitles] subtitleText es NULL. Asígnalo en el Inspector.");
            yield return new WaitForSeconds(subtitleDuration);
        }

        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        isRunning = false;
        Debug.Log("[RaceResultSubtitles] Subtítulos terminados.");

        onFinished?.Invoke();
    }

    // ─── RESPAWN ─────────────────────────────────────────────────────────────

    private void TeleportViaCampfireTeleporter()
    {
        if (campfireTeleporter == null)
        {
            Debug.LogWarning("RaceResultSubtitles: campfireTeleporter no asignado. " +
                             "Asigna un HazardTeleporter apuntando a la fogata.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("RaceResultSubtitles: playerTransform no encontrado.");
            return;
        }

        // Usar el collider del player para que HazardTeleporter lo mueva con su fade
        Collider playerCollider = playerTransform.GetComponentInChildren<Collider>();
        if (playerCollider == null)
            playerCollider = playerTransform.GetComponent<Collider>();

        if (playerCollider != null)
        {
            campfireTeleporter.ForceTeleport(playerCollider);
        }
        else
        {
            Debug.LogWarning("RaceResultSubtitles: No se encontró Collider en el Player.");
        }
    }
}
