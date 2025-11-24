using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float velocidadRotacion = 10f;

    [HideInInspector] public float inputHorizontal;
    [HideInInspector] public float inputVertical;
    [HideInInspector] public bool inputCorrer;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Transform camaraTransform;

    private IState _currentState;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraTransform = Camera.main.transform;

        SetState(new IdleState());
    }

    private void Update()
    {
        _currentState?.Update(this);
    }

    public void SetState(IState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }
}
