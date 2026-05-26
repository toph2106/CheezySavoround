using UnityEngine;

public class PlayingState : IGameState
{
    public GameState StateType => GameState.Playing;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Playing");
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Playing");
    }
}
