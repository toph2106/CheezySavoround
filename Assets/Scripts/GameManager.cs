using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    CheckingCombo,
    Animating,
    GameOver
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Menu;

    [Header("UI Components")]
    public GameObject gameOverPanel;

    [Header("VFX Prefabs")]
    public GameObject explosionPrefab;
    public GameObject floatingTextPrefab;

    public event Action<GameState, GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ChangeState(GameState.Playing);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState oldState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(oldState, newState);

        if (newState == GameState.GameOver && gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log($"[GameManager] {oldState} -> {newState}");
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public bool IsInteractable()
    {
        // Cho phép kéo thả đĩa liên tục ngay cả khi pizza đang bay/merge
        return CurrentState == GameState.Playing || 
               CurrentState == GameState.CheckingCombo || 
               CurrentState == GameState.Animating;
    }
}
