using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CheckpointSaveTrigger : MonoBehaviour
{
    [Header("Checkpoint")]
    [Tooltip("Punto exacto donde reaparecera el jugador. Si esta vacio, usa la posicion de este objeto.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Tag del jugador que activa el guardado")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si esta activo, solo guarda una vez por partida")]
    [SerializeField] private bool saveOnlyOnce = true;

    private bool _alreadySaved;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_alreadySaved && saveOnlyOnce)
            return;

        if (other == null)
            return;

        PlayerStateMachine psm = other.GetComponentInParent<PlayerStateMachine>();
        if (psm == null)
            return;

        bool validByTag = other.CompareTag(playerTag) || psm.gameObject.CompareTag(playerTag);
        if (!validByTag)
            return;

        Vector3 spawn = spawnPoint != null ? spawnPoint.position : transform.position;
        string sceneName = SceneManager.GetActiveScene().name;

        GameProgressDatabase.SaveSceneAndSpawn(sceneName, spawn);

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.RecordCheckpoint();
        }

        // Actualizar también la posición de respawn en runtime
        if (psm != null)
            psm.LastGroundedPosition = spawn;

        // Notificar al CheckpointManager (para coherencia entre escenas)
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.SetRuntimeSpawn(spawn);

        _alreadySaved = true;
        Debug.Log("[Checkpoint] Guardado en JSON: " + spawn + " | Escena: " + sceneName);
    }
}
