public interface IState
{
    void Enter(PlayerController player);
    void Exit(PlayerController player);
    void Update(PlayerController player);
}
