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
    [Tooltip("Script CharacterDialogue del personaje (Boss1).")]
    public CharacterDialogue characterDialogue;

    [Tooltip("Script Boss2Dialogue (Boss2). Usar en vez de characterDialogue.")]
    public Boss2Dialogue boss2Dialogue;

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
            characterDialogue.onLoseDialogueFinished.AddListener(HandleLoseDialogueFinished);
        if (boss2Dialogue != null)
            boss2Dialogue.onLoseDialogueFinished.AddListener(HandleLoseDialogueFinished);

        ShowGadget();
    }

    private void OnDisable()
    {
        if (characterDialogue != null)
            characterDialogue.onLoseDialogueFinished.RemoveListener(HandleLoseDialogueFinished);
        if (boss2Dialogue != null)
            boss2Dialogue.onLoseDialogueFinished.RemoveListener(HandleLoseDialogueFinished);
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
        // Oculta collider y renderers sin desactivar el GameObject,
        // para que BossRaceManager siga escuchando onLoseDialogueFinished.
        HideGadget();

        // Detiene y teletransporta al Boss a su punto de inicio
        if (bossManager != null)
        {
            bossManager.StopBossAfterWin();
        }

        if (bossTeleporter != null)
        {
            bossTeleporter.ForceTeleport(bossCollider);
        }

        if (characterDialogue != null) characterDialogue.ShowLoseDialogue();
        else if (boss2Dialogue != null) boss2Dialogue.ShowLoseDialogue();
    }

    private void HideGadget()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    private void ShowGadget()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
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
        if (boss2Dialogue != null)
        {
            boss2Dialogue.AllowRaceRetry();
            boss2Dialogue.SetInteractionEnabled(true);
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
        if (characterDialogue != null)       characterDialogue.ShowWinDialogue();
        else if (boss2Dialogue != null)      boss2Dialogue.ShowWinDialogue();
        else Debug.LogError("[BossRaceManager] No hay CharacterDialogue ni Boss2Dialogue asignado.");

        // 5. Hacer el diamante invisible sin desactivar el GameObject completo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
