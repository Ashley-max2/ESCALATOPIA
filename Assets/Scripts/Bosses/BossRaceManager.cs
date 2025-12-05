using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestor principal de carreras contra bosses
/// Controla el inicio, progreso y finalización de las carreras de escalada
/// </summary>
public class BossRaceManager : MonoBehaviour
{
    [Header("Race Configuration")]
    [SerializeField] private Transform raceStartPoint;
    [SerializeField] private Transform raceGoalPoint;
    [SerializeField] private float raceStartDelay = 3f;
    [SerializeField] private bool autoStartRace = false;

    [Header("Boss Setup")]
    [SerializeField] private BossAIBase activeBoss;
    [SerializeField] private List<BossAIBase> availableBosses = new List<BossAIBase>();
    [SerializeField] private int currentBossIndex = 0;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerController playerController;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI raceStatusText;
    [SerializeField] private TextMeshProUGUI playerProgressText;
    [SerializeField] private TextMeshProUGUI bossProgressText;
    [SerializeField] private GameObject raceResultsPanel;
    [SerializeField] private TextMeshProUGUI resultsText;

    [Header("Race Progress Tracking")]
    [SerializeField] private float checkpointRadius = 3f;
    [SerializeField] private List<Transform> checkpoints = new List<Transform>();

    // Race state
    private bool raceActive = false;
    private bool raceCompleted = false;
    private float raceStartTime = 0f;
    private float playerRaceTime = 0f;
    private float bossRaceTime = 0f;
    private RaceWinner winner = RaceWinner.None;

    // Progress tracking
    private int playerCheckpointIndex = 0;
    private int bossCheckpointIndex = 0;
    private float playerDistanceToGoal = 0f;
    private float bossDistanceToGoal = 0f;

    // Statistics
    private RaceStatistics raceStats = new RaceStatistics();

    private void Start()
    {
        InitializeRace();

        if (autoStartRace)
        {
            Invoke("StartRaceCountdown", 2f);
        }
    }

    private void Update()
    {
        if (raceActive && !raceCompleted)
        {
            UpdateRaceProgress();
            CheckRaceCompletion();
            UpdateUI();
        }
    }

    /// <summary>
    /// Inicializa la carrera
    /// </summary>
    private void InitializeRace()
    {
        // Encontrar jugador si no está asignado
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // Encontrar todos los bosses en la escena si no están asignados
        if (availableBosses.Count == 0)
        {
            availableBosses.AddRange(FindObjectsOfType<BossAIBase>());
        }

        // Seleccionar el primer boss
        if (availableBosses.Count > 0 && activeBoss == null)
        {
            SelectBoss(0);
        }

        // Ocultar panel de resultados
        if (raceResultsPanel != null)
        {
            raceResultsPanel.SetActive(false);
        }

        // Generar checkpoints si no existen
        if (checkpoints.Count == 0)
        {
            GenerateCheckpoints();
        }
    }

    /// <summary>
    /// Selecciona un boss específico
    /// </summary>
    public void SelectBoss(int index)
    {
        if (index < 0 || index >= availableBosses.Count) return;

        // Desactivar boss anterior
        if (activeBoss != null)
        {
            activeBoss.gameObject.SetActive(false);
        }

        currentBossIndex = index;
        activeBoss = availableBosses[index];
        activeBoss.gameObject.SetActive(true);

        Debug.Log($"Boss seleccionado: {activeBoss.name}");
    }

    /// <summary>
    /// Inicia la cuenta regresiva de la carrera
    /// </summary>
    public void StartRaceCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    /// <summary>
    /// Corrutina de cuenta regresiva
    /// </summary>
    private System.Collections.IEnumerator CountdownCoroutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            for (int i = (int)raceStartDelay; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            countdownText.text = "¡GO!";
            yield return new WaitForSeconds(0.5f);
            
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(raceStartDelay);
        }

        StartRace();
    }

    /// <summary>
    /// Inicia la carrera
    /// </summary>
    private void StartRace()
    {
        raceActive = true;
        raceCompleted = false;
        raceStartTime = Time.time;
        winner = RaceWinner.None;

        // Resetear checkpoints
        playerCheckpointIndex = 0;
        bossCheckpointIndex = 0;

        // Posicionar jugador
        if (playerTransform != null && raceStartPoint != null)
        {
            playerTransform.position = raceStartPoint.position;
            playerTransform.rotation = raceStartPoint.rotation;
        }

        // Iniciar boss
        if (activeBoss != null)
        {
            activeBoss.StartRace();
        }

        // Resetear estadísticas
        raceStats = new RaceStatistics();

        if (raceStatusText != null)
        {
            raceStatusText.text = "¡Carrera iniciada!";
        }

        Debug.Log("¡Carrera iniciada!");
    }

    /// <summary>
    /// Actualiza el progreso de la carrera
    /// </summary>
    private void UpdateRaceProgress()
    {
        if (playerTransform != null && raceGoalPoint != null)
        {
            playerDistanceToGoal = Vector3.Distance(playerTransform.position, raceGoalPoint.position);
        }

        if (activeBoss != null && raceGoalPoint != null)
        {
            bossDistanceToGoal = Vector3.Distance(activeBoss.transform.position, raceGoalPoint.position);
        }

        // Actualizar checkpoints
        UpdateCheckpoints();

        // Actualizar estadísticas
        raceStats.totalTime = Time.time - raceStartTime;
        raceStats.playerProgress = CalculateProgress(playerCheckpointIndex, playerDistanceToGoal);
        raceStats.bossProgress = CalculateProgress(bossCheckpointIndex, bossDistanceToGoal);
    }

    /// <summary>
    /// Actualiza el progreso de checkpoints
    /// </summary>
    private void UpdateCheckpoints()
    {
        // Checkpoints del jugador
        if (playerCheckpointIndex < checkpoints.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(playerTransform.position, 
                                                          checkpoints[playerCheckpointIndex].position);
            if (distanceToCheckpoint < checkpointRadius)
            {
                playerCheckpointIndex++;
                Debug.Log($"Jugador alcanzó checkpoint {playerCheckpointIndex}");
            }
        }

        // Checkpoints del boss
        if (activeBoss != null && bossCheckpointIndex < checkpoints.Count)
        {
            float distanceToCheckpoint = Vector3.Distance(activeBoss.transform.position, 
                                                          checkpoints[bossCheckpointIndex].position);
            if (distanceToCheckpoint < checkpointRadius)
            {
                bossCheckpointIndex++;
                Debug.Log($"Boss alcanzó checkpoint {bossCheckpointIndex}");
            }
        }
    }

    /// <summary>
    /// Calcula el progreso total (0-100%)
    /// </summary>
    private float CalculateProgress(int checkpointIndex, float distanceToGoal)
    {
        if (checkpoints.Count == 0) return 0f;

        float checkpointProgress = (float)checkpointIndex / checkpoints.Count;
        float distanceProgress = 1f - (distanceToGoal / 100f); // Normalizado

        return Mathf.Clamp01((checkpointProgress * 0.8f) + (distanceProgress * 0.2f)) * 100f;
    }

    /// <summary>
    /// Verifica si la carrera se completó
    /// </summary>
    private void CheckRaceCompletion()
    {
        bool playerFinished = playerDistanceToGoal < 2f;
        bool bossFinished = activeBoss != null && bossDistanceToGoal < 2f;

        if (playerFinished || bossFinished)
        {
            if (!raceCompleted)
            {
                EndRace(playerFinished, bossFinished);
            }
        }
    }

    /// <summary>
    /// Finaliza la carrera
    /// </summary>
    private void EndRace(bool playerFinished, bool bossFinished)
    {
        raceCompleted = true;
        raceActive = false;

        float endTime = Time.time - raceStartTime;

        // Determinar ganador
        if (playerFinished && !bossFinished)
        {
            winner = RaceWinner.Player;
            playerRaceTime = endTime;
        }
        else if (bossFinished && !playerFinished)
        {
            winner = RaceWinner.Boss;
            bossRaceTime = endTime;
        }
        else if (playerFinished && bossFinished)
        {
            // Ambos llegaron, el más rápido gana
            playerRaceTime = endTime;
            bossRaceTime = endTime;
            
            if (playerDistanceToGoal < bossDistanceToGoal)
            {
                winner = RaceWinner.Player;
            }
            else
            {
                winner = RaceWinner.Boss;
            }
        }

        // Detener boss
        if (activeBoss != null)
        {
            activeBoss.StopRace();
        }

        // Actualizar estadísticas finales
        raceStats.winner = winner;
        raceStats.playerFinalTime = playerRaceTime > 0 ? playerRaceTime : endTime;
        raceStats.bossFinalTime = bossRaceTime > 0 ? bossRaceTime : endTime;

        ShowResults();
        Debug.Log($"¡Carrera terminada! Ganador: {winner}");
    }

    /// <summary>
    /// Muestra los resultados de la carrera
    /// </summary>
    private void ShowResults()
    {
        if (raceResultsPanel != null)
        {
            raceResultsPanel.SetActive(true);
        }

        string resultMessage = "";
        
        switch (winner)
        {
            case RaceWinner.Player:
                resultMessage = "¡VICTORIA!\n\n";
                resultMessage += $"Has derrotado a {activeBoss.name}\n";
                resultMessage += $"Tiempo: {playerRaceTime:F2}s\n";
                resultMessage += $"Diferencia: {Mathf.Abs(playerRaceTime - bossRaceTime):F2}s\n";
                break;

            case RaceWinner.Boss:
                resultMessage = "DERROTA\n\n";
                resultMessage += $"{activeBoss.name} te ha superado\n";
                resultMessage += $"Tiempo del Boss: {bossRaceTime:F2}s\n";
                resultMessage += $"Diferencia: {Mathf.Abs(playerRaceTime - bossRaceTime):F2}s\n";
                break;

            case RaceWinner.None:
                resultMessage = "EMPATE\n\n";
                resultMessage += "¡Increíble carrera!\n";
                break;
        }

        resultMessage += $"\nProgreso jugador: {raceStats.playerProgress:F1}%\n";
        resultMessage += $"Progreso boss: {raceStats.bossProgress:F1}%";

        if (resultsText != null)
        {
            resultsText.text = resultMessage;
        }

        Debug.Log(resultMessage);
    }

    /// <summary>
    /// Actualiza la UI de la carrera
    /// </summary>
    private void UpdateUI()
    {
        if (playerProgressText != null)
        {
            playerProgressText.text = $"Jugador: {raceStats.playerProgress:F1}%\nDistancia: {playerDistanceToGoal:F1}m";
        }

        if (bossProgressText != null && activeBoss != null)
        {
            bossProgressText.text = $"{activeBoss.name}: {raceStats.bossProgress:F1}%\nDistancia: {bossDistanceToGoal:F1}m";
        }

        if (raceStatusText != null)
        {
            float elapsedTime = Time.time - raceStartTime;
            raceStatusText.text = $"Tiempo: {elapsedTime:F1}s";
        }
    }

    /// <summary>
    /// Genera checkpoints automáticamente entre inicio y meta
    /// </summary>
    private void GenerateCheckpoints()
    {
        if (raceStartPoint == null || raceGoalPoint == null) return;

        checkpoints.Clear();
        int numCheckpoints = 5;

        for (int i = 1; i <= numCheckpoints; i++)
        {
            float t = (float)i / (numCheckpoints + 1);
            Vector3 position = Vector3.Lerp(raceStartPoint.position, raceGoalPoint.position, t);
            
            GameObject checkpoint = new GameObject($"Checkpoint_{i}");
            checkpoint.transform.position = position;
            checkpoint.transform.SetParent(transform);
            
            checkpoints.Add(checkpoint.transform);
        }

        Debug.Log($"Generados {numCheckpoints} checkpoints automáticos");
    }

    /// <summary>
    /// Reinicia la carrera
    /// </summary>
    public void RestartRace()
    {
        raceActive = false;
        raceCompleted = false;
        
        if (raceResultsPanel != null)
        {
            raceResultsPanel.SetActive(false);
        }

        StartRaceCountdown();
    }

    /// <summary>
    /// Siguiente boss
    /// </summary>
    public void NextBoss()
    {
        int nextIndex = (currentBossIndex + 1) % availableBosses.Count;
        SelectBoss(nextIndex);
        
        if (raceResultsPanel != null)
        {
            raceResultsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Boss anterior
    /// </summary>
    public void PreviousBoss()
    {
        int prevIndex = currentBossIndex - 1;
        if (prevIndex < 0) prevIndex = availableBosses.Count - 1;
        SelectBoss(prevIndex);
        
        if (raceResultsPanel != null)
        {
            raceResultsPanel.SetActive(false);
        }
    }

    // Getters públicos
    public bool IsRaceActive => raceActive;
    public RaceStatistics GetRaceStats() => raceStats;
    public BossAIBase GetActiveBoss() => activeBoss;

    // Debug visual
    private void OnDrawGizmos()
    {
        // Dibujar inicio
        if (raceStartPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(raceStartPoint.position, 2f);
        }

        // Dibujar meta
        if (raceGoalPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(raceGoalPoint.position, 2f);
        }

        // Dibujar checkpoints
        if (checkpoints != null && checkpoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform checkpoint in checkpoints)
            {
                if (checkpoint != null)
                {
                    Gizmos.DrawWireSphere(checkpoint.position, checkpointRadius);
                }
            }
        }

        // Dibujar línea de progreso
        if (Application.isPlaying && raceActive)
        {
            if (playerTransform != null && raceGoalPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(playerTransform.position, raceGoalPoint.position);
            }

            if (activeBoss != null && raceGoalPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(activeBoss.transform.position, raceGoalPoint.position);
            }
        }
    }
}

/// <summary>
/// Estadísticas de la carrera
/// </summary>
[System.Serializable]
public class RaceStatistics
{
    public float totalTime;
    public float playerProgress;
    public float bossProgress;
    public float playerFinalTime;
    public float bossFinalTime;
    public RaceWinner winner;
}

public enum RaceWinner
{
    None,
    Player,
    Boss
}
