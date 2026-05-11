using UnityEngine;

/// <summary>
/// Teletransporta al jugador a un punto fijo al pulsar F1 cuando está dentro del trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerF1Teleport : MonoBehaviour
{
    [Header("Teleport")]
    [Tooltip("Transform de destino. Arrastra aquí un empty colocado en el punto exacto donde quieres que aparezca el jugador")]
    [SerializeField] private Transform targetTransform;

    [Tooltip("Tag del jugador que activará el teletransporte")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Si está activo, el script intentará usar el Rigidbody del jugador al teletransportar")]
    [SerializeField] private bool useRigidbodyIfAvailable = true;

    [Tooltip("Si está activo, el teleporter se desactiva después de usarse una vez")]
    [SerializeField] private bool oneShot = false;

    private PlayerStateMachine _currentPlayer;

    private void Awake()
    {
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
            ownCollider.isTrigger = true;

        if (targetTransform == null)
        {
            Debug.LogWarning($"[PlayerF1Teleport] {gameObject.name} no tiene targetTransform asignado. Crea un empty y arrástralo aquí.");
        }
    }

    private void Update()
    {
        if (_currentPlayer == null)
            return;

        if (!Input.GetKeyDown(KeyCode.F1))
            return;

        TeleportPlayer(_currentPlayer.gameObject);

        if (oneShot)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        PlayerStateMachine player = other.GetComponentInParent<PlayerStateMachine>();
        bool validPlayer = other.CompareTag(playerTag) || (player != null && player.gameObject.CompareTag(playerTag));

        if (!validPlayer)
            return;

        _currentPlayer = player != null ? player : other.GetComponentInParent<PlayerStateMachine>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentPlayer == null || other == null)
            return;

        PlayerStateMachine player = other.GetComponentInParent<PlayerStateMachine>();
        if (player == _currentPlayer || other.gameObject == _currentPlayer.gameObject)
        {
            _currentPlayer = null;
        }
    }

    private void TeleportPlayer(GameObject playerObject)
    {
        if (targetTransform == null)
        {
            Debug.LogWarning($"[PlayerF1Teleport] No se puede teletransportar porque falta targetTransform en {gameObject.name}.");
            return;
        }

        Vector3 destination = targetTransform.position;
        Rigidbody playerRb = playerObject.GetComponent<Rigidbody>();

        if (useRigidbodyIfAvailable && playerRb != null)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.position = destination;
        }

        playerObject.transform.position = destination;

        Physics.SyncTransforms();
        Debug.Log($"[PlayerF1Teleport] Jugador teletransportado a {destination}");
    }
}
