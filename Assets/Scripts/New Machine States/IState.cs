public interface IState
{
    void Enter(PlayerController p);
    void Update(PlayerController p);
    void Exit(PlayerController p);
}
