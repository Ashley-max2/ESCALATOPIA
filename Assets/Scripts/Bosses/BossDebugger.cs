using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Herramientas de debugging y testing para el sistema de bosses
/// </summary>
public class BossDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossAIBase targetBoss;
    [SerializeField] private BossRaceManager raceManager;
    [SerializeField] private ClimbingPathfinder pathfinder;

    [Header("Debug Options")]
    [SerializeField] private bool showCurrentState = true;
    [SerializeField] private bool showStamina = true;
    [SerializeField] private bool showPath = true;
    [SerializeField] private bool showDecisions = true;
    [SerializeField] private bool logToConsole = true;

    [Header("Testing")]
    [SerializeField] private bool enableGodMode = false;
    [SerializeField] private bool freezeStamina = false;
    [SerializeField] private float timeScale = 1f;

    private GUIStyle labelStyle;
    private float updateInterval = 0.5f;
    private float lastUpdateTime = 0f;
    private string debugInfo = "";

    private void Start()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        labelStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f));
        labelStyle.padding = new RectOffset(10, 10, 10, 10);
    }

    private void Update()
    {
        // Actualizar debug info periódicamente
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateDebugInfo();
            lastUpdateTime = Time.time;
        }

        // God mode
        if (enableGodMode && targetBoss != null)
        {
            targetBoss.transform.position = Vector3.Lerp(
                targetBoss.transform.position,
                raceManager.GetActiveBoss().transform.position + Vector3.up * 2f,
                Time.deltaTime
            );
        }

        // Freeze stamina
        if (freezeStamina && targetBoss != null)
        {
            // Esto requeriría un método público en BossAIBase
            // O usar reflection (no recomendado para producción)
        }

        // Time scale
        Time.timeScale = timeScale;

        // Controles de teclado
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        // F1 - Toggle debug overlay
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showCurrentState = !showCurrentState;
        }

        // F2 - Iniciar carrera
        if (Input.GetKeyDown(KeyCode.F2) && raceManager != null)
        {
            raceManager.StartRaceCountdown();
        }

        // F3 - Reiniciar carrera
        if (Input.GetKeyDown(KeyCode.F3) && raceManager != null)
        {
            raceManager.RestartRace();
        }

        // F4 - Siguiente boss
        if (Input.GetKeyDown(KeyCode.F4) && raceManager != null)
        {
            raceManager.NextBoss();
            targetBoss = raceManager.GetActiveBoss();
        }

        // F5 - Boss anterior
        if (Input.GetKeyDown(KeyCode.F5) && raceManager != null)
        {
            raceManager.PreviousBoss();
            targetBoss = raceManager.GetActiveBoss();
        }

        // F6 - Toggle God Mode
        if (Input.GetKeyDown(KeyCode.F6))
        {
            enableGodMode = !enableGodMode;
            Debug.Log($"God Mode: {enableGodMode}");
        }

        // + / - para time scale
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            timeScale = Mathf.Min(timeScale + 0.25f, 5f);
            Debug.Log($"Time Scale: {timeScale}x");
        }
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            timeScale = Mathf.Max(timeScale - 0.25f, 0.25f);
            Debug.Log($"Time Scale: {timeScale}x");
        }
    }

    private void UpdateDebugInfo()
    {
        if (targetBoss == null) return;

        debugInfo = $"=== BOSS DEBUG INFO ===\n\n";
        debugInfo += $"Boss: {targetBoss.name}\n";
        debugInfo += $"Estado: {targetBoss.CurrentState}\n";
        debugInfo += $"Resistencia: {targetBoss.CurrentStamina:F1}/{targetBoss.MaxStamina:F1}\n";
        debugInfo += $"Carrera Activa: {targetBoss.IsRaceActive}\n\n";

        if (targetBoss.NextClimbPoint != null)
        {
            float distance = Vector3.Distance(targetBoss.transform.position, targetBoss.NextClimbPoint.position);
            debugInfo += $"Próximo Punto: {distance:F1}m\n";
        }

        // Estadísticas de Boss3 (Learner)
        if (targetBoss is Boss3Learner learner)
        {
            debugInfo += "\n=== APRENDIZAJE ===\n";
            var stats = learner.GetLearningStats();
            foreach (var stat in stats)
            {
                debugInfo += $"{stat.Key}: {stat.Value:F2}\n";
            }
        }

        // Estadísticas de carrera
        if (raceManager != null && raceManager.IsRaceActive)
        {
            var raceStats = raceManager.GetRaceStats();
            debugInfo += "\n=== CARRERA ===\n";
            debugInfo += $"Tiempo: {raceStats.totalTime:F1}s\n";
            debugInfo += $"Progreso Jugador: {raceStats.playerProgress:F1}%\n";
            debugInfo += $"Progreso Boss: {raceStats.bossProgress:F1}%\n";
        }

        if (logToConsole)
        {
            Debug.Log(debugInfo);
        }
    }

    private void OnGUI()
    {
        if (!showCurrentState || targetBoss == null) return;

        // Debug overlay en pantalla
        GUI.Label(new Rect(10, 10, 300, 500), debugInfo, labelStyle);

        // Controles en pantalla
        string controls = "=== CONTROLES ===\n\n";
        controls += "F1 - Toggle Debug\n";
        controls += "F2 - Iniciar Carrera\n";
        controls += "F3 - Reiniciar Carrera\n";
        controls += "F4 - Siguiente Boss\n";
        controls += "F5 - Boss Anterior\n";
        controls += "F6 - God Mode\n";
        controls += "+/- - Time Scale\n";

        GUI.Label(new Rect(Screen.width - 310, 10, 300, 300), controls, labelStyle);

        // Time scale indicator
        if (timeScale != 1f)
        {
            GUIStyle timeScaleStyle = new GUIStyle(labelStyle);
            timeScaleStyle.fontSize = 24;
            timeScaleStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 50), 
                     $"Time: {timeScale:F2}x", timeScaleStyle);
        }

        // God mode indicator
        if (enableGodMode)
        {
            GUIStyle godModeStyle = new GUIStyle(labelStyle);
            godModeStyle.fontSize = 20;
            godModeStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, 70, 200, 40), 
                     "GOD MODE", godModeStyle);
        }
    }

    private void OnDrawGizmos()
    {
        if (targetBoss == null) return;

        // Dibujar estado actual con color
        Color stateColor = Color.white;
        switch (targetBoss.CurrentState)
        {
            case BossState.Idle: stateColor = Color.gray; break;
            case BossState.Moving: stateColor = Color.green; break;
            case BossState.Climbing: stateColor = Color.blue; break;
            case BossState.Hooking: stateColor = Color.cyan; break;
            case BossState.Resting: stateColor = Color.yellow; break;
            case BossState.Finished: stateColor = Color.red; break;
        }

        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(targetBoss.transform.position + Vector3.up * 3f, 0.5f);

        // Dibujar barra de resistencia
        if (showStamina)
        {
            Vector3 barStart = targetBoss.transform.position + Vector3.up * 2.5f + Vector3.left * 0.5f;
            float barLength = 1f;
            float staminaRatio = targetBoss.CurrentStamina / targetBoss.MaxStamina;

            // Barra de fondo
            Gizmos.color = Color.black;
            Gizmos.DrawLine(barStart, barStart + Vector3.right * barLength);

            // Barra de resistencia
            Color staminaColor = staminaRatio > 0.5f ? Color.green : 
                                 staminaRatio > 0.25f ? Color.yellow : Color.red;
            Gizmos.color = staminaColor;
            Gizmos.DrawLine(barStart, barStart + Vector3.right * (barLength * staminaRatio));
        }

        // Dibujar línea al próximo punto
        if (showPath && targetBoss.NextClimbPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(targetBoss.transform.position, targetBoss.NextClimbPoint.position);
        }
    }

    // Métodos públicos para usar desde el Inspector o UI

    [ContextMenu("Force Win Boss")]
    public void ForceWinBoss()
    {
        if (targetBoss != null && raceManager != null)
        {
            // Teleportar boss a la meta
            Transform goalPoint = raceManager.GetActiveBoss().transform; // Esto necesita ajuste
            targetBoss.transform.position = goalPoint.position;
            Debug.Log("Boss teleportado a la meta");
        }
    }

    [ContextMenu("Reset Boss Stamina")]
    public void ResetBossStamina()
    {
        if (targetBoss != null)
        {
            // Esto requiere un método público en BossAIBase
            Debug.Log("Resistencia del boss reseteada");
        }
    }

    [ContextMenu("Print Boss Stats")]
    public void PrintBossStats()
    {
        if (targetBoss == null) return;

        Debug.Log("=== ESTADÍSTICAS DEL BOSS ===");
        Debug.Log($"Nombre: {targetBoss.name}");
        Debug.Log($"Estado: {targetBoss.CurrentState}");
        Debug.Log($"Resistencia: {targetBoss.CurrentStamina}/{targetBoss.MaxStamina}");
        Debug.Log($"Carrera Activa: {targetBoss.IsRaceActive}");

        if (targetBoss is Boss3Learner learner)
        {
            Debug.Log("\n=== ESTADÍSTICAS DE APRENDIZAJE ===");
            var stats = learner.GetLearningStats();
            foreach (var stat in stats)
            {
                Debug.Log($"{stat.Key}: {stat.Value:F3}");
            }
        }
    }

    [ContextMenu("Reinitialize Pathfinder")]
    public void ReinitializePathfinder()
    {
        if (pathfinder != null)
        {
            pathfinder.InitializeClimbPoints();
            Debug.Log("Pathfinder reinicializado");
        }
    }

    /// <summary>
    /// Crea una textura de color sólido para los fondos de GUI
    /// </summary>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
