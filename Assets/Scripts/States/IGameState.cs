
public interface IGameState
{

    GameState StateType { get; }

    void Enter(GameManager manager);

    void Execute(GameManager manager);

    void Exit(GameManager manager);
}
