using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Danh sách Achievement")]
    public AchievementConfig[] configs;

    public event Action<AchievementConfig> OnAchievementUnlocked;

    private Dictionary<string, AchievementConfig> _configMap = new Dictionary<string, AchievementConfig>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _configMap.Clear();
        if (configs != null)
        {
            foreach (var cfg in configs)
            {
                if (cfg != null && !string.IsNullOrEmpty(cfg.achievementID))
                    _configMap[cfg.achievementID] = cfg;
            }
        }
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        PlateItem.OnAnyPlateExploded += HandlePlateExploded;
        PlateItem.OnAnyPlatePlaced += HandlePlatePlaced;

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGoldEarned += HandleGoldEarned;
            SaveSystem.Instance.OnSkinUnlocked += HandleSkinUnlocked;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
            GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.OnComboAchieved += HandleCombo;
        }
    }

    private void UnsubscribeEvents()
    {
        PlateItem.OnAnyPlateExploded -= HandlePlateExploded;
        PlateItem.OnAnyPlatePlaced -= HandlePlatePlaced;

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGoldEarned -= HandleGoldEarned;
            SaveSystem.Instance.OnSkinUnlocked -= HandleSkinUnlocked;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
            GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.OnComboAchieved -= HandleCombo;
        }
    }

    private void HandlePlateExploded()
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.TotalPlatesExploded;
        CheckAndUnlock(AchievementType.PlatesExploded, total);
    }

    private void HandlePlatePlaced()
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.TotalPlatesPlaced;
        CheckAndUnlock(AchievementType.PlatesPlaced, total);
    }

    private void HandleGoldEarned(int amount)
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.TotalGoldEarned;
        CheckAndUnlock(AchievementType.GoldEarned, total);
    }

    private void HandleSkinUnlocked(string skinID)
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.OwnedSkins.Count - 1;
        CheckAndUnlock(AchievementType.SkinsUnlocked, total);
    }

    private void HandleGameOver()
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.TotalGamesPlayed;
        CheckAndUnlock(AchievementType.GamesPlayed, total);
    }

    private void HandleLevelCompleted(int level, int score)
    {
        if (SaveSystem.Instance == null) return;
        int cleared = SaveSystem.Instance.Data.HighestUnlockedLevel - 1;
        CheckAndUnlock(AchievementType.LevelsCleared, cleared);
    }

    private void HandleCombo(int comboCount)
    {
        CheckAndUnlock(AchievementType.ComboReached, comboCount);
    }

    private void CheckAndUnlock(AchievementType type, int currentValue)
    {
        if (SaveSystem.Instance == null || configs == null) return;

        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.type != type) continue;

            AchievementData ach = SaveSystem.Instance.GetAchievement(cfg.achievementID);

            if (ach != null && ach.IsUnlocked) continue;

            // Cập nhật progress
            int progress = Mathf.Min(currentValue, cfg.targetValue);
            bool justUnlocked = currentValue >= cfg.targetValue;

            SaveSystem.Instance.UpdateAchievement(cfg.achievementID, progress, justUnlocked);

            if (justUnlocked)
            {
                Debug.Log($"[Achievement] 🏆 UNLOCKED: {cfg.displayName} ({cfg.achievementID})");
                OnAchievementUnlocked?.Invoke(cfg);
            }
            else
            {
                Debug.Log($"[Achievement] Progress: {cfg.achievementID} = {progress}/{cfg.targetValue}");
            }
        }

        SaveSystem.Instance.Save();
    }

    public (int progress, int target, bool unlocked, bool claimed) GetProgress(string achievementID)
    {
        if (!_configMap.ContainsKey(achievementID))
            return (0, 0, false, false);

        var cfg = _configMap[achievementID];
        var ach = SaveSystem.Instance?.GetAchievement(achievementID);

        int progress = ach?.Progress ?? 0;
        bool unlocked = ach?.IsUnlocked ?? false;
        bool claimed = ach?.IsRewardClaimed ?? false;

        return (progress, cfg.targetValue, unlocked, claimed);
    }

    public bool ClaimReward(string achievementID)
    {
        if (!_configMap.ContainsKey(achievementID)) return false;

        var cfg = _configMap[achievementID];
        bool success = SaveSystem.Instance.ClaimAchievementReward(achievementID, cfg.goldReward);

        if (success)
            Debug.Log($"[Achievement] Claimed reward: {cfg.displayName} → +{cfg.goldReward} gold");

        return success;
    }

    public AchievementConfig GetConfig(string achievementID)
    {
        return _configMap.ContainsKey(achievementID) ? _configMap[achievementID] : null;
    }

    public AchievementConfig[] GetAllConfigs()
    {
        return configs;
    }
}
