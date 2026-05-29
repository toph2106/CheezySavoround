using UnityEngine;

public class MenuState : IGameState
{
    public GameState StateType => GameState.Menu;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Menu");
        if (manager.menuPanel != null) manager.menuPanel.SetActive(true);
        if (manager.gamePanel != null) manager.gamePanel.SetActive(false);

        if (GridManager.Instance != null) GridManager.Instance.gameObject.SetActive(false);
        if (TrayManager.Instance != null) TrayManager.Instance.gameObject.SetActive(false);
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Menu");
        if (manager.menuPanel != null) manager.menuPanel.SetActive(false);
    }
}
