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
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    [Header("VFX Prefabs")]
    public GameObject explosionPrefab;
    public GameObject floatingTextPrefab;

    public event Action<GameState, GameState> OnStateChanged;

    // === Achievement Events ===
    public event Action OnGameStarted;
    public event Action OnGameOver;

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
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)

        ChangeState(GameState.Menu);
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.Menu);
    }

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
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.DeleteSave();
            }
        }

        if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.LeftControl))
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.ResetDailyReward();
                
                var dailyUI = FindFirstObjectByType<DailyRewardUI>();
                if (dailyUI != null) dailyUI.RefreshUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SkipOneDay();
                
                var dailyUI = FindFirstObjectByType<DailyRewardUI>();
                if (dailyUI != null) dailyUI.RefreshUI();
            }
        }

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

        // Achievement events
        if (newState == GameState.Playing && oldState == GameState.Menu)
            OnGameStarted?.Invoke();

        if (newState == GameState.GameOver)
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Data.TotalGamesPlayed++;
            OnGameOver?.Invoke();
        }

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
