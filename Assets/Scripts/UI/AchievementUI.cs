using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Panel Achievement chính — popup giống Daily Reward.
/// Chứa ScrollView với danh sách achievement slot.
/// 
/// Hierarchy nên có:
///   AchievementPanel (this script)
///     ┣ Background / Header
///     ┣ CloseButton
///     ┗ ScrollView
///         ┗ Viewport
///             ┗ Content (Vertical Layout Group)
///                 ┣ [AchievementSlot prefab sẽ được spawn ở đây]
/// </summary>
public class AchievementUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject achievementPanel;

    [Header("Scroll Content")]
    [Tooltip("Content object bên trong ScrollView (có Vertical Layout Group)")]
    public Transform scrollContent;

    [Header("Prefab")]
    [Tooltip("Prefab AchievementSlot — sẽ được Instantiate cho mỗi achievement")]
    public GameObject slotPrefab;

    [Header("Nút đóng")]
    public UnityEngine.UI.Button closeButton;

    private List<AchievementSlotUI> _spawnedSlots = new List<AchievementSlotUI>();
    private bool _built = false;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseAchievement);
        }

        // Lắng nghe unlock để tự refresh
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
    }

    // ==================== MỞ / ĐÓNG ====================

    public void OpenAchievement()
    {
        if (achievementPanel != null)
            achievementPanel.SetActive(true);

        BuildSlots();
        RefreshAll();
    }

    public void CloseAchievement()
    {
        if (achievementPanel != null)
            achievementPanel.SetActive(false);
    }

    // ==================== BUILD & REFRESH ====================

    /// <summary>
    /// Tạo slot UI cho mỗi achievement config (chỉ tạo 1 lần).
    /// </summary>
    private void BuildSlots()
    {
        if (_built) return;
        if (AchievementManager.Instance == null || slotPrefab == null || scrollContent == null) return;

        var allConfigs = AchievementManager.Instance.GetAllConfigs();
        if (allConfigs == null) return;

        foreach (var cfg in allConfigs)
        {
            if (cfg == null) continue;

            GameObject obj = Instantiate(slotPrefab, scrollContent);
            obj.SetActive(true);

            AchievementSlotUI slot = obj.GetComponent<AchievementSlotUI>();
            if (slot != null)
            {
                slot.Setup(cfg);
                _spawnedSlots.Add(slot);
            }
        }

        _built = true;
    }

    /// <summary>
    /// Refresh tất cả slot (khi mở panel hoặc khi có achievement mới).
    /// </summary>
    public void RefreshAll()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) slot.Refresh();
        }
    }

    private void OnAchievementUnlocked(AchievementConfig config)
    {
        // Nếu panel đang mở → refresh ngay
        if (achievementPanel != null && achievementPanel.activeSelf)
            RefreshAll();
    }
}
