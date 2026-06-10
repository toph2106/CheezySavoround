using UnityEngine;

public class GameOverState : IGameState
{
    public GameState StateType => GameState.GameOver;

    public void Enter(GameManager manager)
    {

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

        if (manager.gameOverPanel != null)
        {
            manager.gameOverPanel.SetActive(false);
        }
    }
}
