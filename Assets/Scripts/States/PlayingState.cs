using UnityEngine;

public class PlayingState : IGameState
{
    public GameState StateType => GameState.Playing;

    public void Enter(GameManager manager)
    {
        if (manager.gamePanel != null) manager.gamePanel.SetActive(true);

        if (GridManager.Instance != null) GridManager.Instance.gameObject.SetActive(true);
        if (TrayManager.Instance != null) TrayManager.Instance.gameObject.SetActive(true);
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
    }
}
