using UnityEngine;

public class MenuState : IGameState
{
    public GameState StateType => GameState.Menu;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Menu");
        // Hiện menu UI, ẩn UI lúc chơi
        if (manager.menuPanel != null) manager.menuPanel.SetActive(true);
        if (manager.gamePanel != null) manager.gamePanel.SetActive(false);

        // Ẩn bàn cờ 3D để không bị chồng chéo lên Menu
        if (GridManager.Instance != null) GridManager.Instance.gameObject.SetActive(false);
        if (TrayManager.Instance != null) TrayManager.Instance.gameObject.SetActive(false);
    }

    public void Execute(GameManager manager)
    {
        // Chờ người chơi nhấn Start
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Menu");
        // Ẩn menu UI
        if (manager.menuPanel != null) manager.menuPanel.SetActive(false);
    }
}
