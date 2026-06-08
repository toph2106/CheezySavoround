using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton quản lý Achievement — hoàn toàn event-driven.
/// Chỉ lắng nghe event từ Core game, KHÔNG can thiệp logic game.
/// 
/// Setup: Gắn lên 1 GameObject trong scene, kéo AchievementConfig assets vào mảng configs.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Danh sách Achievement (kéo ScriptableObject vào đây)")]
    [Tooltip("Thêm / bớt / sắp xếp lại tùy ý — code tự xử lý")]
    public AchievementConfig[] configs;

    /// <summary>
    /// Phát ra khi 1 achievement được unlock.
    /// UI lắng nghe event này để hiện toast.
    /// </summary>
    public event Action<AchievementConfig> OnAchievementUnlocked;

    // Cache nhanh: ID → Config
    private Dictionary<string, AchievementConfig> _configMap = new Dictionary<string, AchievementConfig>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build config lookup
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

    // ==================== EVENT SUBSCRIPTION ====================

    private void SubscribeEvents()
    {
        // PlateItem (static events)
        PlateItem.OnAnyPlateExploded += HandlePlateExploded;
        PlateItem.OnAnyPlatePlaced += HandlePlatePlaced;

        // SaveSystem
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGoldEarned += HandleGoldEarned;
            SaveSystem.Instance.OnSkinUnlocked += HandleSkinUnlocked;
        }

        // GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += HandleGameOver;
        }

        // GameJuice
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
        }

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.OnComboAchieved -= HandleCombo;
        }
    }

    // ==================== EVENT HANDLERS ====================

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
        int total = SaveSystem.Instance.Data.OwnedSkins.Count - 1; // -1 vì "default" không tính
        CheckAndUnlock(AchievementType.SkinsUnlocked, total);
    }

    private void HandleGameOver()
    {
        if (SaveSystem.Instance == null) return;
        int total = SaveSystem.Instance.Data.TotalGamesPlayed;
        CheckAndUnlock(AchievementType.GamesPlayed, total);
    }

    private void HandleCombo(int comboCount)
    {
        CheckAndUnlock(AchievementType.ComboReached, comboCount);
    }

    // ==================== CORE LOGIC ====================

    /// <summary>
    /// Kiểm tra tất cả achievement thuộc type này.
    /// Nếu currentValue >= targetValue → unlock.
    /// </summary>
    private void CheckAndUnlock(AchievementType type, int currentValue)
    {
        if (SaveSystem.Instance == null || configs == null) return;

        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.type != type) continue;

            AchievementData ach = SaveSystem.Instance.GetAchievement(cfg.achievementID);

            // Đã unlock rồi → skip
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

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Lấy progress hiện tại của 1 achievement.
    /// Trả về (progress, target, isUnlocked, isClaimed).
    /// </summary>
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

    /// <summary>
    /// Claim reward cho achievement. Trả về true nếu thành công.
    /// </summary>
    public bool ClaimReward(string achievementID)
    {
        if (!_configMap.ContainsKey(achievementID)) return false;

        var cfg = _configMap[achievementID];
        bool success = SaveSystem.Instance.ClaimAchievementReward(achievementID, cfg.goldReward);

        if (success)
            Debug.Log($"[Achievement] Claimed reward: {cfg.displayName} → +{cfg.goldReward} gold");

        return success;
    }

    /// <summary>
    /// Lấy config theo ID.
    /// </summary>
    public AchievementConfig GetConfig(string achievementID)
    {
        return _configMap.ContainsKey(achievementID) ? _configMap[achievementID] : null;
    }

    /// <summary>
    /// Lấy tất cả configs (dùng cho UI render danh sách).
    /// </summary>
    public AchievementConfig[] GetAllConfigs()
    {
        return configs;
    }
}
