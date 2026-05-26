using UnityEngine;

public class GameOverState : IGameState
{
    public GameState StateType => GameState.GameOver;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: GameOver");

        if (manager.gameOverPanel != null)
        {
            manager.gameOverPanel.SetActive(true);
        }
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: GameOver");

        if (manager.gameOverPanel != null)
        {
            manager.gameOverPanel.SetActive(false);
        }
    }
}
