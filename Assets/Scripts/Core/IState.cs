/// <summary>
/// Interfaz base para todos los estados del jugador.
/// Implementa el patrón State para gestionar las mecánicas del Player.
/// </summary>
public interface IState
{
    void Enter();
    void Execute();
    void FixedExecute();
    void Exit();
}
