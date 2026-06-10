using UnityEngine;

public class AnimatingState : IGameState
{
    public GameState StateType => GameState.Animating;

    public void Enter(GameManager manager)
    {
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
    }
}
