using System;
using System.Collections.Generic;
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

    // === FSM ===
    private IGameState _currentState;
    private Dictionary<GameState, IGameState> _stateCache;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _stateCache = new Dictionary<GameState, IGameState>
        {
            { GameState.Menu,          new MenuState() },
            { GameState.Playing,       new PlayingState() },
            { GameState.CheckingCombo, new CheckingComboState() },
            { GameState.Animating,     new AnimatingState() },
            { GameState.GameOver,      new GameOverState() }
        };
    }

    void Start()
    {
        // Nạp level từ dữ liệu đã lưu (nếu SaveSystem đã sẵn sàng)
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
        {
            Debug.Log($"[GameManager] Dữ liệu đã tải: Vàng = {SaveSystem.Instance.Data.Gold}");
        }

        ChangeState(GameState.Playing);
    }

    // === Tự động lưu game khi tắt/tạm dừng ===
    void OnApplicationQuit()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && SaveSystem.Instance != null)
            SaveSystem.Instance.Save();
    }

    void Update()
    {
        _currentState?.Execute(this);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState && _currentState != null) return;

        GameState oldState = CurrentState;

        _currentState?.Exit(this);

        CurrentState = newState;

        if (_stateCache.TryGetValue(newState, out IGameState nextState))
        {
            _currentState = nextState;
            _currentState.Enter(this);
        }

        OnStateChanged?.Invoke(oldState, newState);

        Debug.Log($"[GameManager] {oldState} -> {newState}");
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public bool IsInteractable()
    {
        return CurrentState == GameState.Playing || 
               CurrentState == GameState.CheckingCombo || 
               CurrentState == GameState.Animating;
    }

    public IGameState GetCurrentStateInstance()
    {
        return _currentState;
    }
}
