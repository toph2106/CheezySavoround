using UnityEngine;

public class CheckingComboState : IGameState
{
    public GameState StateType => GameState.CheckingCombo;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: CheckingCombo");
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: CheckingCombo");
    }
}
