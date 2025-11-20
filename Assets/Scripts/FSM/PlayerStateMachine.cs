using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{

    public PlayerStateSO estadoActual;

    [Header("Estados")]

    public PlayerStateSO idleState;
    public PlayerStateSO walkingState;
    public PlayerStateSO runningState;
    public PlayerStateSO jumpingState;
    public PlayerStateSO fallingState;
    public PlayerStateSO climbingState;
    public PlayerStateSO wallJumpState;
    public PlayerStateSO deadState;

    [HideInInspector] public PlayerMovement movimiento;
    [HideInInspector] public PlayerJump salto;
    [HideInInspector] public Escalada escalada;
    [HideInInspector] public SaltoEscalado saltoEscalado;
    [HideInInspector] public PlayerFallDetector fallDetector;
    [HideInInspector] public ResistenceController resistencia;

    private void Start()
    {
        movimiento = GetComponent<PlayerMovement>();
        salto = GetComponent<PlayerJump>();
        escalada = GetComponent<Escalada>();
        saltoEscalado = GetComponent<SaltoEscalado>();
        fallDetector = GetComponent<PlayerFallDetector>();
        resistencia = GetComponent<ResistenceController>();

        CambiarEstado(idleState);

    }

    void Update()
    {
        if (estadoActual != null)
            estadoActual.Decide(this); 
    }

    public void CambiarEstado (PlayerStateSO nuevoEstado)
    {
        if (estadoActual != null)
        {
            estadoActual.Exit(this);
        }

        estadoActual = nuevoEstado;

        if (estadoActual != null)
        {
            estadoActual.Enter(this);
        }

        Debug.Log("Estado cambiado a: " + estadoActual.name);
    }

}