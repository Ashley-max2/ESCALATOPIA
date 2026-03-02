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
        // 1. Hace desaparecer el objeto final temporalmente
        gameObject.SetActive(false);

        // 2. Teletransporta al Boss a su punto de inicio
        if (bossTeleporter != null)
        {
            bossTeleporter.ForceTeleport(bossCollider);
        }

        // 3. Reactiva el objeto (aunque BossManager luego lo gestionará ocultándolo hasta que vuelva a empezar)
        gameObject.SetActive(true);

        // 4. Se llama a BossManager para reiniciarlo
        if (bossManager != null)
        {
            bossManager.RestartRace();
        }
    }

    private void HandlePlayerWin()
    {
        // 1. Hace desaparecer el objeto final
        gameObject.SetActive(false);

        // 2. Se le desactiva el NPC Interactable de la puerta a créditos
        if (creditDoorNPCInteractable != null)
        {
            creditDoorNPCInteractable.gameObject.SetActive(false);
        }

        // 3. Se activa el script de cambio de escena para abrir los créditos
        if (creditSceneChanger != null)
        {
            creditSceneChanger.Unlock();
            creditSceneChanger.gameObject.SetActive(true);
            creditSceneChanger.enabled = true;
        }
    }
}
