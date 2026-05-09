using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestiona la piscina de bosses ML en la escena de entrenamiento.
///
/// • Instancia N copias del prefab Boss (personajePrincipal + BossAgent)
/// • Los coloca en la fogata al inicio
/// • Cuando un boss toca el suelo "Muerte", el BossAgent llama a EndEpisode()
///   y OnEpisodeBegin() lo respawnea en la fogata automáticamente
/// • Expone estadísticas de entrenamiento en tiempo real
/// • Implementa "best boss tracking": el boss con mayor reward acumulado
///   se destaca visualmente y sus variables se usan como referencia
/// </summary>
public class BossSpawner : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Prefab del Boss (personajePrincipal con BossAgent añadido)")]
    public GameObject bossPrefab;

    [Tooltip("Número de bosses a entrenar en paralelo")]
    [Range(1, 16)]
    public int bossCount = 4;

    [Tooltip("Transform de la Fogata (punto de spawn)")]
    public Transform fogataSpawn;

    [Tooltip("Radio de separación entre bosses en el spawn")]
    public float spawnSpread = 3f;

    // ─── Estado runtime ───────────────────────────────────────────────
    private List<BossAgent> _bosses = new List<BossAgent>();
    private BossAgent       _bestBoss;

    [Header("=== ESTADO (runtime) ===")]
    [SerializeField] private float bestCumulativeReward = float.NegativeInfinity;
    [SerializeField] private int   totalEpisodes;
    [SerializeField] private int   activeBosses;

    // ─── Materiales de debug ─────────────────────────────────────────
    [Header("=== VISUAL DEBUG ===")]
    [Tooltip("Material que se pone al mejor boss para identificarlo fácilmente")]
    public Material bestBossMaterial;

    // ─── Lifecycle ────────────────────────────────────────────────────
    private void Start()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("[BossSpawner] No hay bossPrefab asignado.");
            return;
        }
        SpawnAllBosses();
    }

    private void Update()
    {
        TrackBestBoss();
        UpdateStats();
    }

    // ─── Spawn ───────────────────────────────────────────────────────

    private void SpawnAllBosses()
    {
        for (int i = 0; i < bossCount; i++)
        {
            Vector3 offset = new Vector3(
                (i % 4) * spawnSpread - spawnSpread * 1.5f,
                0.2f,
                (i / 4) * spawnSpread
            );
            Vector3 pos = fogataSpawn != null
                ? fogataSpawn.position + offset
                : transform.position + offset;

            var go = Instantiate(bossPrefab, pos, Quaternion.identity);
            go.name = $"Boss_{i + 1}";

            var agent = go.GetComponent<BossAgent>();
            if (agent == null) agent = go.AddComponent<BossAgent>();

            agent.fogataSpawn = fogataSpawn;
            _bosses.Add(agent);
        }

        activeBosses = _bosses.Count;
        Debug.Log($"[BossSpawner] {bossCount} bosses spawneados en la fogata.");
    }

    // ─── Tracking del mejor boss ──────────────────────────────────────

    private void TrackBestBoss()
    {
        BossAgent candidate = null;
        float best = float.NegativeInfinity;

        foreach (var b in _bosses)
        {
            if (b == null) continue;
            float r = b.GetCumulativeReward();
            if (r > best) { best = r; candidate = b; }
        }

        if (candidate != null && candidate != _bestBoss)
        {
            // Quitar highlight del anterior
            if (_bestBoss != null && bestBossMaterial != null)
                ResetBossVisual(_bestBoss);

            _bestBoss = candidate;
            bestCumulativeReward = best;

            // Destacar nuevo mejor boss
            if (bestBossMaterial != null)
                SetBossVisual(_bestBoss, bestBossMaterial);

            Debug.Log($"[BossSpawner] 👑 Nuevo mejor boss: {candidate.name} (reward={best:F1})");
        }
    }

    private void UpdateStats()
    {
        totalEpisodes = GlobalMetaTracker.Instance != null
            ? GlobalMetaTracker.Instance.TotalEpisodes
            : 0;

        activeBosses = 0;
        foreach (var b in _bosses)
            if (b != null && b.gameObject.activeInHierarchy) activeBosses++;
    }

    // ─── Helpers visuales ─────────────────────────────────────────────

    private void SetBossVisual(BossAgent boss, Material mat)
    {
        var renderers = boss.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.material = mat;
    }

    private void ResetBossVisual(BossAgent boss)
    {
        // No guardamos los materiales originales aquí por simplicidad;
        // el material se restaura al reiniciarse el prefab o manualmente.
    }

    // ─── GUI de estadísticas ──────────────────────────────────────────
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 270, 160));
        GUILayout.Box("[ BOSS SPAWNER ]");
        GUILayout.Label($"  Bosses activos: {activeBosses}/{bossCount}");
        GUILayout.Label($"  Episodios totales: {totalEpisodes}");
        GUILayout.Label($"  Mejor reward sesión: {bestCumulativeReward:F1}");
        if (_bestBoss != null)
            GUILayout.Label($"  Mejor boss: {_bestBoss.name}");
        if (GlobalMetaTracker.Instance != null)
        {
            GUILayout.Label($"  Avg reward: {GlobalMetaTracker.Instance.AverageReward:F1}");
            GUILayout.Label($"  All-time best: {GlobalMetaTracker.Instance.AllTimeBestReward:F1}");
        }
        GUILayout.EndArea();
    }
}
