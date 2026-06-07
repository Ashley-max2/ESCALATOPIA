using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Header("Referencias de la Carrera")]
    [Tooltip("El controlador de IA del Boss.")]
    public BossAIController bossAI;

    [Tooltip("El Boss Levitante (asignar si la carrera es contra él en lugar de la IA terrestre).")]
    public BossLevitante bossLevitante;

    [Tooltip("El NPC con el que se interactúa para iniciar la carrera (Boss1).")]
    public NPCInteractable npcInteractable;

    [Tooltip("Diálogo del Boss2. Usar en vez de npcInteractable si no hay NPCInteractable.")]
    public Boss2Dialogue boss2Dialogue;

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
        if (!raceStarted)
        {
            bool dialogueDone = (npcInteractable != null && npcInteractable.hasFinishedDialogue)
                             || (boss2Dialogue   != null && boss2Dialogue.hasFinishedDialogue);
            if (dialogueDone) StartRace();
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
            npcInteractable.isLocked = true;
            npcInteractable.ForceReset();
            npcInteractable.enabled = false;
        }
        if (boss2Dialogue != null)
        {
            boss2Dialogue.isLocked = true;
            boss2Dialogue.SetInteractionEnabled(false);
        }

        if (bossLevitante != null)
        {
            bossLevitante.Activate();
        }
        else if (bossAI != null)
        {
            bossAI.enabled = true; // Activa la IA
        }

        // "Aquí se activara el objeto final."
        if (finalObject != null)
        {
            finalObject.SetActive(true); 
        }
    }

    public void StopBossAfterWin()
    {
        // Detiene el boss inmediatamente tras ganar la carrera, sin reiniciar el estado completo.
        // RestartRace() se llamará después de que termine el diálogo de derrota.
        if (bossLevitante != null)
            bossLevitante.ResetBoss();
        else if (bossAI != null)
            bossAI.enabled = false;

        if (bossRigidbody != null)
        {
            bossRigidbody.velocity = Vector3.zero;
            bossRigidbody.angularVelocity = Vector3.zero;
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
        if (npcInteractable != null)
        {
            npcInteractable.isLocked = false;
            npcInteractable.hasFinishedDialogue = false;
            npcInteractable.enabled = true;
        }
        if (boss2Dialogue != null)
        {
            boss2Dialogue.isLocked = false;
            boss2Dialogue.AllowRaceRetry();
            boss2Dialogue.SetInteractionEnabled(true);
        }

        // El Boss espera apagado a que termine el diálogo de nuevo
        if (bossLevitante != null)
        {
            bossLevitante.ResetBoss();
        }
        else if (bossAI != null)
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
