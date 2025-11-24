using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IState _currentState;

    private void Start()
    {
        // Estado inicial
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
