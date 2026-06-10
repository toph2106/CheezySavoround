using System.Collections.Generic;
using UnityEngine;

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

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
    }


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
    public void RefreshAll()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) slot.Refresh();
        }
    }

    private void OnAchievementUnlocked(AchievementConfig config)
    {
        if (achievementPanel != null && achievementPanel.activeSelf)
            RefreshAll();
    }
}
