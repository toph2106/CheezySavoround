using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayHUD : MonoBehaviour
{
    public Slider levelSlider;

    public TextMeshProUGUI currentLevelText;

    public TextMeshProUGUI nextLevelText;

    public TextMeshProUGUI coinText;

    public TextMeshProUGUI[] boosterCountTexts;

    public string[] boosterIDs = new string[]
    {
        "11",
        "12",
        "13",
        "14"
    };
    public float sliderLerpSpeed = 5f;

    private float _targetSliderValue = 0f;
    private int _displayedCoin = 0;
    private int _targetCoin = 0;

    void Start()
    {
        if (levelSlider != null)
        {
            levelSlider.minValue = 0f;
            levelSlider.maxValue = 1f;
            levelSlider.value = 0f;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += OnScoreChanged;
            GameManager.Instance.OnLevelCompleted += OnLevelCompleted;
            GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnDataChanged += OnDataChanged;
        }

        RefreshAll();

        if (boosterCountTexts == null || boosterCountTexts.Length == 0)
            Debug.LogWarning("[HUD] ⚠️ boosterCountTexts CHƯA KÉO! Size = 0");
        else
        {
            for (int i = 0; i < boosterCountTexts.Length; i++)
                Debug.Log($"[HUD] boosterCountTexts[{i}]: {(boosterCountTexts[i] != null ? boosterCountTexts[i].name : "NULL")}");
        }

        if (boosterIDs == null || boosterIDs.Length == 0)
            Debug.LogWarning("[HUD] ⚠️ boosterIDs CHƯA ĐIỀN!");
        else
        {
            for (int i = 0; i < boosterIDs.Length; i++)
                Debug.Log($"[HUD] boosterIDs[{i}]: \"{boosterIDs[i]}\" → số lượng: {PlayerPrefs.GetInt(boosterIDs[i], 0)}");
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
            GameManager.Instance.OnLevelCompleted -= OnLevelCompleted;
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnDataChanged -= OnDataChanged;
        }
    }

    private static string[] _numberCache;

    void Awake()
    {
        if (_numberCache == null)
        {
            _numberCache = new string[10001];
            for (int i = 0; i <= 10000; i++)
            {
                _numberCache[i] = i.ToString();
            }
        }
    }

    void Update()
    {
        // Slider lướt mượt
        if (levelSlider != null)
        {
            levelSlider.value = Mathf.Lerp(levelSlider.value, _targetSliderValue, Time.deltaTime * sliderLerpSpeed);
        }

        // Coin đếm lên mượt
        if (_displayedCoin != _targetCoin)
        {
            if (Mathf.Abs(_targetCoin - _displayedCoin) <= 1)
                _displayedCoin = _targetCoin;
            else
                _displayedCoin = (int)Mathf.Lerp(_displayedCoin, _targetCoin, Time.deltaTime * 8f);

            if (coinText != null)
            {
                if (_displayedCoin >= 0 && _displayedCoin <= 10000)
                    coinText.text = _numberCache[_displayedCoin];
                else
                    coinText.text = _displayedCoin.ToString(); // Rất hiếm khi đạt tới đây
            }
        }
    }

    public void RefreshAll()
    {
        RefreshLevelDisplay();
        RefreshCoin();
        RefreshBoosters();
        RefreshSlider();
    }


    private void RefreshLevelDisplay()
    {
        int currentLevel = 1;
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
            currentLevel = SaveSystem.Instance.Data.CurrentLevel;

        int nextLevel = currentLevel + 1;

        if (currentLevelText != null)
            currentLevelText.text = currentLevel.ToString();

        if (nextLevelText != null)
            nextLevelText.text = nextLevel.ToString();
    }


    private void RefreshSlider()
    {
        if (GameManager.Instance == null || SaveSystem.Instance == null)
        {
            _targetSliderValue = 0f;
            return;
        }

        int currentLevel = SaveSystem.Instance.Data.CurrentLevel;
        int targetIndex = currentLevel - 1;

        if (targetIndex < 0 || targetIndex >= GameManager.LevelScoreTargets.Length)
        {
            _targetSliderValue = 1f;
            return;
        }

        int target = GameManager.LevelScoreTargets[targetIndex];
        int score = GameManager.Instance.SessionScore;

        _targetSliderValue = Mathf.Clamp01((float)score / target);
    }


    private void RefreshCoin()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
        {
            _targetCoin = SaveSystem.Instance.Data.Gold;

            // Lần đầu hiển thị thì nhảy thẳng, không animate
            if (coinText != null && _displayedCoin == 0 && _targetCoin > 0)
            {
                _displayedCoin = _targetCoin;
                coinText.text = _displayedCoin.ToString();
            }
        }
    }

    private void RefreshBoosters()
    {
        if (SaveSystem.Instance == null || SaveSystem.Instance.Data == null) return;
        if (boosterCountTexts == null || boosterIDs == null) return;

        int count = Mathf.Min(boosterCountTexts.Length, boosterIDs.Length);
        for (int i = 0; i < count; i++)
        {
            if (boosterCountTexts[i] == null) continue;

            string id = boosterIDs[i];
            int qty = GetBoosterCount(id);
            if (qty > 0)
            {
                boosterCountTexts[i].text = qty.ToString();
            }
            else
            {
                boosterCountTexts[i].text = "+";
            }
        }
    }

    private int GetBoosterCount(string boosterID)
    {
        return PlayerPrefs.GetInt(boosterID, 0);
    }
    public static void AddBooster(string boosterID, int amount)
    {
        int current = PlayerPrefs.GetInt(boosterID, 0);
        PlayerPrefs.SetInt(boosterID, current + amount);
        PlayerPrefs.Save();
        Debug.Log($"[Booster] Cộng {amount}x {boosterID} → tổng: {current + amount}");
    }
    public static bool UseBooster(string boosterID)
    {
        int current = PlayerPrefs.GetInt(boosterID, 0);
        if (current <= 0) return false;

        PlayerPrefs.SetInt(boosterID, current - 1);
        PlayerPrefs.Save();
        Debug.Log($"[Booster] Dùng 1x {boosterID} → còn: {current - 1}");
        return true;
    }

    private void OnScoreChanged(int newScore)
    {
        RefreshSlider();
    }

    private void OnLevelCompleted(int completedLevel, int nextLevel)
    {
        if (levelSlider != null)
            levelSlider.value = 0f;

        _targetSliderValue = 0f;
        RefreshLevelDisplay();
    }

    private void OnStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.Playing)
        {
            RefreshAll();
        }
    }

    private void OnDataChanged()
    {
        RefreshCoin();
        RefreshBoosters();
    }
}