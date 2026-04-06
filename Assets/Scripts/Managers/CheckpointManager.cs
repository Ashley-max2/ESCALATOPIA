using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que lee el JSON de checkpoint y
/// teletransporta al jugador SIEMPRE 1 frame DESPUÉS de que todos
/// los Start() hayan corrido (incluido PlayerStateMachine.Start()).
///
/// Coloca este componente en un GameObject de la escena MainMenu
/// (o en cualquier escena que cargue primero).
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Tooltip("Nombres exactos de las escenas donde NO se aplica el checkpoint (MainMenu, Creditos…)")]
    [SerializeField] private string[] ignoredScenes = { "MainMenu", "Creditos" };

    // ------------------------------------------------------------------ //
    #region Unity Messages
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Suscribirse ANTES de que sceneLoaded pueda disparar
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Caso especial: si el objeto nace en la escena de juego directamente
        // (p.ej. al darle a Play desde el Editor estando en Level_0),
        // el evento sceneLoaded ya se disparó antes de que existiéramos.
        // Lo comprobamos manualmente aquí.
        CheckAndScheduleSpawnForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Dispara cada vez que se carga una escena NUEVA.
    /// Cuando viene del MainMenu → Level_0, este es el punto de entrada principal.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndScheduleSpawnForScene(scene.name);
    }
    #endregion

    // ------------------------------------------------------------------ //
    #region Spawn Scheduling

    /// <summary>
    /// Comprueba la escena ACTUALMENTE abierta (útil al arrancar el Editor en Level_0).
    /// </summary>
    private void CheckAndScheduleSpawnForCurrentScene()
    {
        CheckAndScheduleSpawnForScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Lee el JSON y, si hay spawn guardado para esta escena,
    /// lanza la coroutine que espera 1 frame antes de teleportar.
    /// </summary>
    private void CheckAndScheduleSpawnForScene(string sceneName)
    {
        if (IsIgnoredScene(sceneName))
            return;

        if (!GameProgressDatabase.HasSave())
            return;

        GameProgressData save = GameProgressDatabase.Load();
        if (save == null || !save.HasSpawnPosition)
            return;

        // Escena del save debe coincidir con la escena actual (si está rellena)
        if (!string.IsNullOrWhiteSpace(save.Scene) &&
            !string.Equals(save.Scene, sceneName, System.StringComparison.OrdinalIgnoreCase))
            return;

        Vector3 spawnPos = save.GetSpawnPosition();

        // Cancelar cualquier coroutine anterior y lanzar una nueva
        StopAllCoroutines();
        StartCoroutine(TeleportAfterStartRoutine(spawnPos));
    }

    /// <summary>
    /// Espera 1 frame completo (todos los Start() ya han corrido)
    /// y luego teleporta al jugador.
    /// </summary>
    private IEnumerator TeleportAfterStartRoutine(Vector3 position)
    {
        // yield return null → siguiente frame → Start() de TODOS los objetos
        // ya se habrá ejecutado (incluido PlayerStateMachine.Start()).
        yield return null;

        PlayerStateMachine player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("[CheckpointManager] No se encontró PlayerStateMachine en la escena.");
            yield break;
        }

        TeleportPlayer(player, position);
    }

    private static void TeleportPlayer(PlayerStateMachine player, Vector3 position)
    {
        Rigidbody rb = player.Rb;

        if (rb != null)
        {
            // Guardar estado cinemático original
            bool wasKinematic = rb.isKinematic;

            // Hacer cinemático para que el motor de física no corrija la posición
            rb.isKinematic = true;

            // Cero velocidades
            rb.velocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Mover usando rb.position (no transform.position) para actualizar
            // la posición interna del motor de física directamente y evitar
            // el "snap back" que ocurre cuando Rigidbody.interpolation = Interpolate
            rb.position = position;

            // Restaurar estado cinemático
            rb.isKinematic = wasKinematic;
        }

        // Mover también el transform (por si rb es null o por coherencia visual)
        player.transform.position   = position;
        player.LastGroundedPosition = position;

        // Forzar sincronización inmediata entre Transform y el motor de física
        // Evita que en el siguiente FixedUpdate el rigidbody "recuerde" la pos anterior
        Physics.SyncTransforms();

        Debug.Log($"[CheckpointManager] Jugador teletransportado al checkpoint: {position}");
    }
    #endregion

    // ------------------------------------------------------------------ //
    #region Helpers
    private static PlayerStateMachine FindPlayer()
    {
        PlayerStateMachine[] found = Object.FindObjectsByType<PlayerStateMachine>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return found != null && found.Length > 0 ? found[0] : null;
    }

    private bool IsIgnoredScene(string sceneName)
    {
        if (ignoredScenes == null) return false;
        foreach (string s in ignoredScenes)
            if (string.Equals(sceneName, s, System.StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
    #endregion

    // ------------------------------------------------------------------ //
    #region Public API
    /// <summary>
    /// Actualiza el LastGroundedPosition del jugador en runtime (llamado desde CheckpointSaveTrigger).
    /// </summary>
    public void SetRuntimeSpawn(Vector3 position)
    {
        PlayerStateMachine player = FindPlayer();
        if (player != null)
            player.LastGroundedPosition = position;
    }

    /// <summary>
    /// Borra el save JSON. Llámalo desde el botón "Nueva Partida" del MainMenu.
    /// </summary>
    public static void ClearSave()
    {
        GameProgressDatabase.DeleteSave();
        PlayerStateMachine player = FindPlayer();
        if (player != null)
            player.LastGroundedPosition = player.transform.position;
        Debug.Log("[CheckpointManager] Save borrado. Nueva partida.");
    }
    #endregion
}
