using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton persistente que lleva la cuenta global de logros de metas.
///
/// REGLA: Cuando cualquier boss alcanza la Meta N un total de 3 veces,
///        la meta queda "masterizada". A partir de ese momento todos los
///        episodios nuevos arrancan con los +100 pts ya contabilizados
///        y la meta deja de dar puntos adicionales.
///
/// También registra el mejor reward acumulado de toda la sesión
/// para saber cuánto progresa el entrenamiento.
/// </summary>
public class GlobalMetaTracker : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────
    public static GlobalMetaTracker Instance { get; private set; }

    // ─── Configuración ────────────────────────────────────────────────
    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Veces que hay que alcanzar una meta para que quede masterizada")]
    [SerializeField] public int masteryThreshold = 3;
    [Tooltip("Puntos que da alcanzar una meta")]
    [SerializeField] public float metaReward = 100f;

    // ─── Estado global (índice 0 = Meta1 … índice 4 = Meta5) ─────────
    [Header("=== ESTADO (runtime) ===")]
    [SerializeField] private int[] metaCompletionCount = new int[5];
    [SerializeField] private bool[] metaMastered       = new bool[5];

    /// <summary>Cuántos bosses han alcanzado cada meta en total.</summary>
    public int[] MetaCompletionCount => metaCompletionCount;
    /// <summary>True si la meta i ya está masterizada (≥3 completions).</summary>
    public bool[] MetaMastered => metaMastered;

    // ─── Stats de entrenamiento ───────────────────────────────────────
    public float AllTimeBestReward   { get; private set; } = float.NegativeInfinity;
    public float AllTimeWorstReward  { get; private set; } = float.PositiveInfinity;
    public float AverageReward       => _episodeCount > 0 ? _totalRewardSum / _episodeCount : 0f;
    public int   TotalEpisodes       => _episodeCount;
    public int   MetasMastered       => System.Array.FindAll(metaMastered, m => m).Length;

    private float _totalRewardSum;
    private int   _episodeCount;

    // ─── Lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Reset();
    }

    // ─── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Informa de que un boss alcanzó Meta(metaIndex+1).
    /// Retorna true si fue la primera vez que se masterizó (para notificación).
    /// </summary>
    public bool RegisterMetaReached(int metaIndex)
    {
        if (metaIndex < 0 || metaIndex >= 5) return false;

        metaCompletionCount[metaIndex]++;

        bool justMastered = !metaMastered[metaIndex]
                            && metaCompletionCount[metaIndex] >= masteryThreshold;
        if (justMastered)
        {
            metaMastered[metaIndex] = true;
            Debug.Log($"[GlobalMetaTracker] 🏆 Meta{metaIndex + 1} MASTERIZADA " +
                      $"({metaCompletionCount[metaIndex]} completions)");
        }
        return justMastered;
    }

    /// <summary>
    /// Devuelve cuántos puntos debe añadir un boss al inicio de su episodio
    /// por las metas ya masterizadas (las que ya no necesita descubrir).
    /// </summary>
    public float GetStartingBonus()
    {
        float bonus = 0f;
        for (int i = 0; i < 5; i++)
            if (metaMastered[i]) bonus += metaReward;
        return bonus;
    }

    /// <summary>True si la meta metaIndex ya está masterizada.</summary>
    public bool IsMetaMastered(int metaIndex) =>
        metaIndex >= 0 && metaIndex < 5 && metaMastered[metaIndex];

    /// <summary>Registra el reward total de un episodio para estadísticas.</summary>
    public void RegisterEpisodeReward(float reward)
    {
        _episodeCount++;
        _totalRewardSum += reward;
        if (reward > AllTimeBestReward)  AllTimeBestReward  = reward;
        if (reward < AllTimeWorstReward) AllTimeWorstReward = reward;
    }

    /// <summary>Reinicia todos los contadores (útil para nuevas sesiones de entrenamiento).</summary>
    public void Reset()
    {
        metaCompletionCount = new int[5];
        metaMastered        = new bool[5];
        _totalRewardSum     = 0f;
        _episodeCount       = 0;
        AllTimeBestReward   = float.NegativeInfinity;
        AllTimeWorstReward  = float.PositiveInfinity;
    }

    // ─── Debug GUI ────────────────────────────────────────────────────
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, 180));
        GUILayout.Box("[ GLOBAL META TRACKER ]");
        for (int i = 0; i < 5; i++)
        {
            string mark = metaMastered[i] ? "🏆" : $"{metaCompletionCount[i]}/{masteryThreshold}";
            GUILayout.Label($"  Meta{i + 1}: {mark}");
        }
        GUILayout.Label($"  Episodios: {_episodeCount}");
        GUILayout.Label($"  Avg Reward: {AverageReward:F1}");
        GUILayout.Label($"  Best: {AllTimeBestReward:F1}");
        GUILayout.Label($"  Metas masterizadas: {MetasMastered}/5");
        GUILayout.EndArea();
    }
}
