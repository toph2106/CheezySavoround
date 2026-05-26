using UnityEngine;

public class MenuState : IGameState
{
    public GameState StateType => GameState.Menu;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Menu");
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Menu");
    }
}
