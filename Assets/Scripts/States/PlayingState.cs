using UnityEngine;

public class PlayingState : IGameState
{
    public GameState StateType => GameState.Playing;

    public void Enter(GameManager manager)
    {
        Debug.Log("[FSM] Vào trạng thái: Playing");
        // Bật UI lúc chơi (hiện vàng, combo, nút pause...)
        if (manager.gamePanel != null) manager.gamePanel.SetActive(true);

        // Bật lại bàn cờ 3D khi vào game
        if (GridManager.Instance != null) GridManager.Instance.gameObject.SetActive(true);
        if (TrayManager.Instance != null) TrayManager.Instance.gameObject.SetActive(true);
    }

    public void Execute(GameManager manager)
    {
    }

    public void Exit(GameManager manager)
    {
        Debug.Log("[FSM] Rời trạng thái: Playing");
    }
}
