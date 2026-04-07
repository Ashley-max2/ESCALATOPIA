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

    [Header("Subtítulos de Resultado")]
    [Tooltip("Script que muestra los subtítulos de victoria y derrota")]
    public RaceResultSubtitles raceResultSubtitles;

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

        // 3. Reactiva el objeto
        gameObject.SetActive(true);

        // 4. Mostrar subtítulos de derrota (teletransporta al player a la fogata al acabar)
        if (raceResultSubtitles != null)
        {
            raceResultSubtitles.ShowDefeat();
        }

        // 5. Se llama a BossManager para reiniciarlo
        if (bossManager != null)
        {
            bossManager.RestartRace();
        }
    }

    private void HandlePlayerWin()
    {
        Debug.Log("[BossRaceManager] HandlePlayerWin() llamado.");

        // 1. Desactivar solo el Collider del diamante para evitar re-triggers
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Desactivar el NPC de la puerta ANTES de ShowVictory
        //    (su OnDisable oculta el PanelNPC → si lo hacemos antes, ShowVictory lo reactiva limpio)
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

        // 4. Mostrar subtítulos de victoria (ahora nada apagará el panel durante la corrutina)
        if (raceResultSubtitles != null)
        {
            Debug.Log("[BossRaceManager] Llamando ShowVictory()...");
            raceResultSubtitles.ShowVictory();
        }
        else
        {
            Debug.LogError("[BossRaceManager] raceResultSubtitles es NULL. Asígnalo en el Inspector del BossRaceManager.");
        }

        // 5. Hacer el diamante invisible sin desactivar el GameObject completo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
