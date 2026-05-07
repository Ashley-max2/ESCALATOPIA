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

    [Header("Estabilidad Física del Boss")]
    [Tooltip("Rigidbody del boss que se configurará para evitar giros/choques extraños con el player.")]
    [SerializeField] private Rigidbody bossRigidbody;

    [Tooltip("Antes de iniciar carrera, bloquea desplazamiento horizontal y rotaciones para que no lo empujen ni giren.")]
    [SerializeField] private bool lockBossBeforeRace = true;

    [Tooltip("Cuando la carrera ya empezó, bloquea la rotación física en X/Z para que no se vuelque al chocar.")]
    [SerializeField] private bool freezeTiltRotation = true;

    private RigidbodyConstraints _originalConstraints;
    private bool _hasCachedConstraints;

    public bool raceStarted { get; private set; } = false;

    private void Start()
    {
        CacheBossPhysics();

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

        ApplyBossPhysicsForCurrentPhase();

        // "cuando se acabe de pasar todos los mensajes el NPC Interactable se desactive y empieze a correr."
        // Se desactiva el COMPONENTE NPC, NO EL GAMEOBJECT. 
        // ¡Así el Boss no desaparece si pusiste el script en el mismo jefe!
        if (npcInteractable != null)
        {
            // Bloquear permanentemente: ya no responde al player aunque entre en la zona
            npcInteractable.isLocked = true;
            npcInteractable.ForceReset();
            npcInteractable.enabled = false;
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

    public void SetFinalObjectActive(bool active)
    {
        if (finalObject != null)
        {
            finalObject.SetActive(active);
        }
    }

    private void ResetRaceState()
    {
        raceStarted = false;

        ApplyBossPhysicsForCurrentPhase();

        // Reactivamos el NPC de inicio y reiniciamos sus diálogos
        // Reactivamos el COMPONENTE NPC de inicio
        if (npcInteractable != null)
        {
            npcInteractable.isLocked = false;          // permite volver a interactuar
            npcInteractable.hasFinishedDialogue = false;
            npcInteractable.enabled = true;
        }

        // El Boss espera apagado a que termine el diálogo de nuevo
        if (bossAI != null)
        {
            bossAI.enabled = false; 
        }

        // El objeto final se apaga hasta que vuelvan a correr
        SetFinalObjectActive(false);
    }

    private void CacheBossPhysics()
    {
        if (bossRigidbody == null && bossAI != null)
            bossRigidbody = bossAI.GetComponent<Rigidbody>();

        if (bossRigidbody != null)
        {
            _originalConstraints = bossRigidbody.constraints;
            _hasCachedConstraints = true;
        }
    }

    private void ApplyBossPhysicsForCurrentPhase()
    {
        if (bossRigidbody == null)
            return;

        bossRigidbody.isKinematic = false;
        bossRigidbody.velocity = Vector3.zero;
        bossRigidbody.angularVelocity = Vector3.zero;

        if (!raceStarted && lockBossBeforeRace)
        {
            // Mantiene gravedad en Y para que pueda caer al suelo, pero evita empujes laterales y giros.
            bossRigidbody.constraints = RigidbodyConstraints.FreezePositionX |
                                        RigidbodyConstraints.FreezePositionZ |
                                        RigidbodyConstraints.FreezeRotation;
            return;
        }

        RigidbodyConstraints baseConstraints = _hasCachedConstraints ? _originalConstraints : RigidbodyConstraints.None;

        if (freezeTiltRotation)
        {
            bossRigidbody.constraints = baseConstraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            bossRigidbody.constraints = baseConstraints;
        }
    }
}
