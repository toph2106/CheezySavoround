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
    public LevelTransition levelTransition;

    public event Action<GameState, GameState> OnStateChanged;

    public event Action OnGameStarted;
    public event Action OnGameOver;

    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnLevelCompleted;

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
    public static readonly int[] LevelPizzaTypeCount = new int[]
    {
        2,  // Level 1  — Dễ: chỉ 2 loại
        2,  // Level 2
        2,  // Level 3
        3,  // Level 4  — Thêm loại 3
        3,  // Level 5
        3,  // Level 6
        3,  // Level 7
        4,  // Level 8  — Thêm loại 4
        4,  // Level 9
        4,  // Level 10
        4,  // Level 11
        4,  // Level 12
        5,  // Level 13 — Thêm loại 5
        5,  // Level 14
        5,  // Level 15
        5,  // Level 16
        5,  // Level 17
        5,  // Level 18
        6,  // Level 19 — Max 6 loại
        6,  // Level 20
        6,  // Level 21
        6,  // Level 22
        6,  // Level 23
        6,  // Level 24
        6,  // Level 25
        6,  // Level 26
        6,  // Level 27
        6,  // Level 28
        6,  // Level 29
        6   // Level 30
    };

    public static readonly int[] LevelMinSlices = new int[]
    {
        1, 1, 1,        // Level 1-3:   1-2 miếng
        1, 1, 1, 1,     // Level 4-7:   1-3 miếng
        2, 2, 2,        // Level 8-10:  2-3 miếng
        2, 2, 2, 2, 2,  // Level 11-15: 2-4 miếng
        2, 2, 2,        // Level 16-18: 2-4 miếng
        3, 3, 3, 3, 3,  // Level 19-23: 3-5 miếng
        3, 3, 3, 3,     // Level 24-27: 3-5 miếng
        4, 4, 4         // Level 28-30: 4-6 miếng
    };

    public static readonly int[] LevelMaxSlices = new int[]
    {
        2, 2, 2,        // Level 1-3
        3, 3, 3, 3,     // Level 4-7
        3, 3, 3,        // Level 8-10
        4, 4, 4, 4, 4,  // Level 11-15
        4, 4, 4,        // Level 16-18
        5, 5, 5, 5, 5,  // Level 19-23
        5, 5, 5, 5,     // Level 24-27
        6, 6, 6         // Level 28-30
    };

    public static int GetPizzaTypeCount(int level)
    {
        int idx = Mathf.Clamp(level - 1, 0, LevelPizzaTypeCount.Length - 1);
        return LevelPizzaTypeCount[idx];
    }

    public static int GetMinSlices(int level)
    {
        int idx = Mathf.Clamp(level - 1, 0, LevelMinSlices.Length - 1);
        return LevelMinSlices[idx];
    }

    public static int GetMaxSlices(int level)
    {
        int idx = Mathf.Clamp(level - 1, 0, LevelMaxSlices.Length - 1);
        return LevelMaxSlices[idx];
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Application.targetFrameRate = 60; // Bắt buộc game chạy mượt ở 60 FPS trên mobile

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

    public void ReplayLevel()
    {
        StopAllCoroutines();

        _sessionScore = 0;
        OnScoreChanged?.Invoke(_sessionScore);

        int level = 1;
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            level = SaveSystem.Instance.Data.CurrentLevel;

        if (BoosterManager.Instance != null)
            BoosterManager.Instance.CancelBooster();

        if (TrayManager.Instance != null)
            TrayManager.Instance.ClearTray();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearGrid();
            GridManager.Instance.LoadLevel(level);
        }

        ChangeState(GameState.Playing);

        StartCoroutine(WaitForGridThenSpawnPlates());

        Debug.Log($"[Game] Replay Level {level}!");
    }

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

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (levelTransition != null)
            {
                StartCoroutine(TestCloudTransition());
            }
            else
            {
            }
        }

        _currentState?.Execute(this);
    }

    private System.Collections.IEnumerator TestCloudTransition()
    {
        Debug.Log("☁️ Đám mây bay vào...");
        levelTransition.PlayTransitionIn();
        
        yield return new WaitForSeconds(levelTransition.transitionDuration + 0.5f);

        Debug.Log("☁️ Đám mây bay ra...");
        levelTransition.PlayTransitionOut();
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
    public void ReplaySpecificLevel(int targetLevel)
    {
        StopAllCoroutines();

        ChangeState(GameState.Playing);

        _sessionScore = 0;
        OnScoreChanged?.Invoke(_sessionScore);

        if (BoosterManager.Instance != null)
            BoosterManager.Instance.CancelBooster();

        if (TrayManager.Instance != null)
            TrayManager.Instance.ClearTray();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.ClearGrid();
            GridManager.Instance.LoadLevel(targetLevel);
        }

        StartCoroutine(WaitForGridThenSpawnPlates());
    }
}
