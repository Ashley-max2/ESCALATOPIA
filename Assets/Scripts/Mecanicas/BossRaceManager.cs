using UnityEngine;

public class BossRaceManager : MonoBehaviour
{
    [Header("Configuración de Tags")]
    public string playerTag = "Player";
    public string bossTag = "Boss";

    [Header("Referencias (Victoria del Boss)")]
    [Tooltip("El teletransportador que devolverá al boss a su posición inicial")]
    public HazardTeleporter bossTeleporter;

    [Tooltip("El manager que reinicia el estado de la carrera")]
    public BossManager bossManager;

    [Header("Referencias (Victoria del Player)")]
    [Tooltip("El NPC Interactable de la puerta hacia los créditos que será desactivado")]
    public NPCInteractable creditDoorNPCInteractable;

    [Tooltip("El script/GameObject de cambio de escena que será activado")]
    public SceneChangeTrigger creditSceneChanger;

    [Header("Sistema de Diálogos")]
    [Tooltip("Script CharacterDialogue del personaje para mostrar diálogos de victoria y derrota")]
    public CharacterDialogue characterDialogue;

    [Tooltip("Teletransportador de la fogata. Solo se usa cuando el Boss gana y el jugador pierde.")]
    public HazardTeleporter campfireTeleporter;

    private Collider playerCollider;

    private void Start()
    {
        CachePlayerCollider();
    }

    private void OnEnable()
    {
        if (characterDialogue != null)
        {
            characterDialogue.onLoseDialogueFinished.AddListener(HandleLoseDialogueFinished);
        }
    }

    private void OnDisable()
    {
        if (characterDialogue != null)
        {
            characterDialogue.onLoseDialogueFinished.RemoveListener(HandleLoseDialogueFinished);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bossTag))
        {
            HandleBossWin(other);
        }
        else if (other.CompareTag(playerTag))
        {
            HandlePlayerWin();
        }
    }

    private void HandleBossWin(Collider bossCollider)
    {
        AnalyticsManager.Instance?.SetSessionOutcome("boss_won");

        // 1. Hace desaparecer el objeto final temporalmente
        gameObject.SetActive(false);

        // 2. Teletransporta al Boss a su punto de inicio
        if (bossTeleporter != null)
        {
            bossTeleporter.ForceTeleport(bossCollider);
        }

        // 3. Reactiva el objeto
        gameObject.SetActive(true);

        // 4. Mostrar diálogo de derrota
        if (characterDialogue != null)
        {
            characterDialogue.ShowLoseDialogue();
        }
    }

    private void HandleLoseDialogueFinished()
    {
        if (bossManager != null)
        {
            bossManager.RestartRace();
        }

        if (characterDialogue != null)
        {
            characterDialogue.AllowRaceRetry();
            characterDialogue.SetInteractionEnabled(true);
        }

        TeleportPlayerToCheckpoint();
    }

    private void CachePlayerCollider()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (playerGO == null)
            return;

        playerCollider = playerGO.GetComponentInChildren<Collider>();
        if (playerCollider == null)
        {
            playerCollider = playerGO.GetComponent<Collider>();
        }
    }

    private void TeleportPlayerToCheckpoint()
    {
        if (campfireTeleporter == null)
            return;

        if (playerCollider == null)
        {
            CachePlayerCollider();
        }

        if (playerCollider != null)
        {
            campfireTeleporter.ForceTeleport(playerCollider);
        }
    }

    private void HandlePlayerWin()
    {
        Debug.Log("[BossRaceManager] HandlePlayerWin() llamado.");

        AnalyticsManager.Instance?.FinishBossAttempt(GetBossAnalyticsId(), true);
        AnalyticsManager.Instance?.SetSessionOutcome("completed");

        // 1. Desactivar solo el Collider del diamante para evitar re-triggers
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Desactivar el NPC de la puerta ANTES de mostrar resultado
        //    (su OnDisable oculta el PanelNPC → si lo hacemos antes, se reactiva limpio)
        if (creditDoorNPCInteractable != null)
        {
            creditDoorNPCInteractable.gameObject.SetActive(false);
        }

        // 3. Activar el script de cambio de escena
        if (creditSceneChanger != null)
        {
            creditSceneChanger.Unlock();
            creditSceneChanger.gameObject.SetActive(true);
            creditSceneChanger.enabled = true;
        }

        // 4. Mostrar diálogo de victoria
        if (characterDialogue != null)
        {
            Debug.Log("[BossRaceManager] Mostrando diálogo de victoria...");
            characterDialogue.ShowWinDialogue();
        }
        else
        {
            Debug.LogError("[BossRaceManager] No hay CharacterDialogue asignado.");
        }

        // 5. Hacer el diamante invisible sin desactivar el GameObject completo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    private string GetBossAnalyticsId()
    {
        if (bossManager != null)
            return bossManager.GetBossAnalyticsId();

        return gameObject.name;
    }
}
