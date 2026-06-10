using System;
using System.Collections;
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

    [Header("Level Transition")]
    [Tooltip("Kéo LevelTransition object vào đây (nếu có)")]
    public LevelTransition levelTransition;

    public event Action<GameState, GameState> OnStateChanged;

    public event Action OnGameStarted;
    public event Action OnGameOver;

    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnLevelCompleted; // (completedLevel, nextLevel)

    private IGameState _currentState;
    private Dictionary<GameState, IGameState> _stateCache;

    private int _sessionScore = 0;
    public int SessionScore => _sessionScore;

    public static readonly int[] LevelScoreTargets = new int[]
    {
        500,   // Level 1
        700,   // Level 2
        900,   // Level 3
        1100,  // Level 4
        1300,  // Level 5
        1600,  // Level 6
        1900,  // Level 7
        2200,  // Level 8
        2500,  // Level 9
        2800,  // Level 10
        3000,  // Level 11
        3300,  // Level 12
        3600,  // Level 13
        4000,  // Level 14
        4500,  // Level 15
        5000,  // Level 16
        5500,  // Level 17
        6000,  // Level 18
        6500,  // Level 19
        7000,  // Level 20
        7500,  // Level 21
        8000,  // Level 22
        8500,  // Level 23
        9000,  // Level 24
        9500,  // Level 25
        10000, // Level 26
        11000, // Level 27
        12000, // Level 28
        13000, // Level 29
        15000  // Level 30
    };

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
        ChangeState(GameState.Menu);
    }

    public void StartGame()
    {
        _sessionScore = 0;
        OnScoreChanged?.Invoke(_sessionScore);

        int level = 1;
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            level = SaveSystem.Instance.Data.CurrentLevel;

        ChangeState(GameState.Playing);

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearGrid();
            GridManager.Instance.LoadLevel(level);
        }

        StartCoroutine(WaitForGridThenSpawnPlates());
    }

    private IEnumerator WaitForGridThenSpawnPlates()
    {
        while (GridManager.Instance != null && !GridManager.Instance.IsReady())
            yield return null;

        if (TrayManager.Instance != null)
            TrayManager.Instance.SpawnNewPlates();
    }

    public void ReturnToMenu()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.ClearGrid();

        ChangeState(GameState.Menu);
    }

    /// <summary>
    /// Cộng điểm khi nổ đĩa. Tự động tính nhân combo.
    /// </summary>
    public void AddScore(int basePoints)
    {
        int comboMultiplier = 1;
        if (GameJuice.Instance != null)
        {
            comboMultiplier = Mathf.Max(1, GetCurrentCombo());
        }

        int points = basePoints * comboMultiplier;
        _sessionScore += points;

        OnScoreChanged?.Invoke(_sessionScore);

        CheckLevelTarget();
    }

    private int GetCurrentCombo()
    {
        if (GameJuice.Instance != null)
            return Mathf.Max(1, GameJuice.Instance.ComboCount);
        return 1;
    }

    private void CheckLevelTarget()
    {
        if (SaveSystem.Instance == null) return;

        int currentLevel = SaveSystem.Instance.Data.CurrentLevel;

        if (currentLevel > LevelScoreTargets.Length)
            return;

        int targetIndex = currentLevel - 1;
        if (targetIndex < 0 || targetIndex >= LevelScoreTargets.Length)
            return;

        int target = LevelScoreTargets[targetIndex];

        if (_sessionScore >= target)
        {
            StartCoroutine(AdvanceToNextLevel(currentLevel));
        }
    }

    private IEnumerator AdvanceToNextLevel(int completedLevel)
    {
        int nextLevel = completedLevel + 1;

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.UpdateHighScore(_sessionScore);
            SaveSystem.Instance.Data.CurrentLevel = nextLevel;
            SaveSystem.Instance.UnlockNextLevel();
            SaveSystem.Instance.Save();
        }

        OnLevelCompleted?.Invoke(completedLevel, nextLevel);

        if (nextLevel > 30)
        {
            ChangeState(GameState.GameOver);
            yield break;
        }

        ChangeState(GameState.Animating);

        if (levelTransition != null)
        {
            levelTransition.PlayTransitionIn();
            yield return new WaitForSeconds(levelTransition.transitionDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (GridManager.Instance != null)
            GridManager.Instance.ClearGrid();

        yield return null;

        _sessionScore = 0;
        OnScoreChanged?.Invoke(_sessionScore);

        if (GridManager.Instance != null)
            GridManager.Instance.LoadLevel(nextLevel);

        if (levelTransition != null)
        {
            levelTransition.PlayTransitionOut();
            yield return new WaitForSeconds(levelTransition.transitionDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        while (GridManager.Instance != null && !GridManager.Instance.IsReady())
            yield return null;

        if (TrayManager.Instance != null)
            TrayManager.Instance.SpawnNewPlates();

        ChangeState(GameState.Playing);

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

        if (Input.GetKeyDown(KeyCode.T))
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

        if (newState == GameState.Playing && oldState == GameState.Menu)
            OnGameStarted?.Invoke();

        if (newState == GameState.GameOver)
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Data.TotalGamesPlayed++;
                SaveSystem.Instance.UpdateHighScore(_sessionScore);
                SaveSystem.Instance.Save();
            }
            OnGameOver?.Invoke();
        }

    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public bool IsInteractable()
    {
        return CurrentState == GameState.Playing || 
               CurrentState == GameState.CheckingCombo;
    }

    public IGameState GetCurrentStateInstance()
    {
        return _currentState;
    }
}
