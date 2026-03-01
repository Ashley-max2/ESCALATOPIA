using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Header("Referencias de la Carrera")]
    [Tooltip("El controlador de IA del Boss.")]
    public BossAIController bossAI;

    [Tooltip("El NPC con el que se interactúa para iniciar la carrera.")]
    public NPCInteractable npcInteractable;

    [Tooltip("El objeto final que se activará cuando comience la carrera.")]
    public GameObject finalObject;

    public bool raceStarted { get; private set; } = false;

    private void Start()
    {
        // Al inicio, el manager oculta el final y desactiva el boss
        ResetRaceState();
    }

    private void Update()
    {
        // "hasta que no haya pasado los mensajes del canvas de NPC Interactable el Boss AI Controller este desactivado"
        if (!raceStarted && npcInteractable != null && npcInteractable.hasFinishedDialogue)
        {
            StartRace();
        }
    }

    private void StartRace()
    {
        raceStarted = true;

        // "cuando se acabe de pasar todos los mensajes el NPC Interactable se desactive y empieze a correr."
        // Se desactiva el COMPONENTE NPC, NO EL GAMEOBJECT. 
        // ¡Así el Boss no desaparece si pusiste el script en el mismo jefe!
        if (npcInteractable != null)
        {
            npcInteractable.enabled = false;
            
            // Ocultamos el cartel de E por si se quedó encendido
            npcInteractable.isPlayerNear = false;
            npcInteractable.Update(); 
        }

        if (bossAI != null)
        {
            bossAI.enabled = true; // Activa la IA
        }

        // "Aquí se activara el objeto final."
        if (finalObject != null)
        {
            finalObject.SetActive(true); 
        }
    }

    public void RestartRace()
    {
        // "y el Bossmanager lo reinicia."
        ResetRaceState();
    }

    private void ResetRaceState()
    {
        raceStarted = false;

        // Reactivamos el NPC de inicio y reiniciamos sus diálogos
        // Reactivamos el COMPONENTE NPC de inicio
        if (npcInteractable != null)
        {
            npcInteractable.hasFinishedDialogue = false; 
            npcInteractable.enabled = true;
        }

        // El Boss espera apagado a que termine el diálogo de nuevo
        if (bossAI != null)
        {
            bossAI.enabled = false; 
        }

        // El objeto final se apaga hasta que vuelvan a correr
        if (finalObject != null)
        {
            finalObject.SetActive(false);
        }
    }
}
