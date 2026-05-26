using UnityEngine;

public class AnimatingState : IGameState
{
    public GameState StateType => GameState.Animating;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Animating");
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Animating");
    }
}
