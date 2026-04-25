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

    [Header("Subtítulos de Resultado (LEGACY)")]
    [Tooltip("Script que muestra los subtítulos de victoria y derrota (opcional si usas CharacterDialogue)")]
    public RaceResultSubtitles raceResultSubtitles;

    [Header("Nuevo Sistema de Diálogos")]
    [Tooltip("Script CharacterDialogue del personaje (si se asigna, usará este en lugar de RaceResultSubtitles)")]
    public CharacterDialogue characterDialogue;

    [Tooltip("Script que reinicia la carrera después de perder (si se asigna con nuevo sistema de diálogos)")]
    public RaceRestartController raceRestartController;

    private bool useCharacterDialogue = false;

    private void Start()
    {
        useCharacterDialogue = characterDialogue != null;
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
        if (useCharacterDialogue && characterDialogue != null)
        {
            characterDialogue.ShowLoseDialogue();
            // Reiniciar la carrera después de mostrar el diálogo
            if (raceRestartController != null)
            {
                Invoke(nameof(RestartRaceAfterDelay), 2f); // pequeño delay para que se vea el diálogo
            }
        }
        else if (raceResultSubtitles != null)
        {
            raceResultSubtitles.ShowDefeat();
            // Si usas el sistema antiguo, el BossManager también reinicia
            if (bossManager != null)
            {
                bossManager.RestartRace();
            }
        }
        // Si no usas ni el nuevo ni el antiguo, al menos reinicia con BossManager
        else if (bossManager != null)
        {
            bossManager.RestartRace();
        }
    }

    private void RestartRaceAfterDelay()
    {
        if (raceRestartController != null)
        {
            raceRestartController.RestartRace();
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
        if (useCharacterDialogue && characterDialogue != null)
        {
            Debug.Log("[BossRaceManager] Llamando ShowWinDialogue() del CharacterDialogue...");
            characterDialogue.ShowWinDialogue();
        }
        else if (raceResultSubtitles != null)
        {
            Debug.Log("[BossRaceManager] Llamando ShowVictory() del RaceResultSubtitles...");
            raceResultSubtitles.ShowVictory();
        }
        else
        {
            Debug.LogError("[BossRaceManager] No hay CharacterDialogue ni RaceResultSubtitles asignados.");
        }

        // 5. Hacer el diamante invisible sin desactivar el GameObject completo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
