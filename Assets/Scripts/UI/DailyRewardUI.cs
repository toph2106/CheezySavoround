using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class DaySlotConfig
{
    [Tooltip("Ảnh icon phần thưởng (đồng xu, pizza, rương...)")]
    public Texture rewardIcon;

    [Tooltip("Số vàng nhận được")]
    public int goldAmount = 50;

    [Tooltip("Mô tả ngắn hiển thị dưới icon (VD: '150', 'TRASH CAN', 'BIG REWARDS')")]
    public string rewardLabel = "";
}

[Serializable]
public class DaySlotUI
{
    [Tooltip("RawImage nền ô ngày (để đổi sáng/tối)")]
    public RawImage slotBackground;

    [Tooltip("RawImage icon phần thưởng")]
    public RawImage rewardImage;

    [Tooltip("Text 'DAY X' phía trên")]
    public TextMeshProUGUI dayLabel;

    [Tooltip("Text mô tả / số vàng phía dưới")]
    public TextMeshProUGUI rewardText;

    [Tooltip("Text 'CLAIMED' — ẩn/hiện tùy trạng thái")]
    public TextMeshProUGUI claimedText;
}

public class DailyRewardUI : MonoBehaviour
{
    [Header("Panel Chính")]
    public GameObject dailyPanel;

    [Header("Cấu hình 7 ngày (Kéo icon + Gõ số vàng)")]
    public List<DaySlotConfig> dayConfigs = new List<DaySlotConfig>();

    [Header("UI 7 ô ngày (Kéo từ Hierarchy)")]
    public List<DaySlotUI> daySlotUIs = new List<DaySlotUI>();

    [Header("Ảnh nền ô ngày theo trạng thái")]
    [Tooltip("Ảnh nền ô ngày CHƯA ĐẾN (xám/mờ)")]
    public Texture slotLockedBG;

    [Tooltip("Ảnh nền ô ngày ĐANG CHỜ NHẬN (sáng/highlight)")]
    public Texture slotReadyBG;

    [Header("Màu xám khi đã nhận")]
    [Tooltip("Màu phủ lên ô khi ĐÃ NHẬN (tự làm tối — không cần ảnh riêng)")]
    public Color claimedDimColor = new Color(0.3f, 0.25f, 0.2f, 1f);

    [Tooltip("Màu bình thường (trắng = giữ nguyên ảnh gốc)")]
    public Color normalColor = Color.white;

    [Header("Nút Claim")]
    public Button claimButton;

    [Header("Màu nút Claim")]
    [Tooltip("Màu nút khi CÓ THỂ nhận")]
    public Color claimActiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    [Tooltip("Màu nút khi ĐÃ NHẬN (xám/nâu đen)")]
    public Color claimInactiveColor = new Color(0.3f, 0.25f, 0.2f, 1f);

    void Start()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged += RefreshUI;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        }

        RefreshUI();
    }

    void OnDestroy()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged -= RefreshUI;
    }

    public void OpenDaily()
    {
        if (dailyPanel != null) dailyPanel.SetActive(true);
        RefreshUI();
    }

    public void CloseDaily()
    {
        if (dailyPanel != null) dailyPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (SaveSystem.Instance == null) return;

        var data = SaveSystem.Instance.Data;
        bool canClaim = SaveSystem.Instance.CanClaimDailyReward();

        int daysSince = SaveSystem.Instance.GetDaysSinceLastClaim();
        int nextDay = data.DailyRewardDay + 1;
        if (daysSince < 0 || daysSince > 1) nextDay = 0;
        if (nextDay >= 7) nextDay = 0;

        int slotCount = Mathf.Min(daySlotUIs.Count, dayConfigs.Count);
        for (int i = 0; i < slotCount && i < 7; i++)
        {
            DaySlotUI slot = daySlotUIs[i];
            DaySlotConfig config = dayConfigs[i];

            if (slot == null) continue;

            if (slot.dayLabel != null)
                slot.dayLabel.text = $"DAY {i + 1}";

            if (slot.rewardImage != null && config.rewardIcon != null)
                slot.rewardImage.texture = config.rewardIcon;

            if (slot.rewardText != null)
            {
                if (string.IsNullOrEmpty(config.rewardLabel))
                    slot.rewardText.text = config.goldAmount.ToString();
                else
                    slot.rewardText.text = config.rewardLabel;
            }

            bool isClaimed = data.DailyRewardClaimed[i];
            bool isReady = canClaim && (i == nextDay);

            if (slot.slotBackground != null)
            {
                if (isClaimed)
                {
                    slot.slotBackground.color = claimedDimColor;
                }
                else if (isReady)
                {
                    if (slotReadyBG != null) slot.slotBackground.texture = slotReadyBG;
                    slot.slotBackground.color = normalColor;
                }
                else
                {
                    if (slotLockedBG != null) slot.slotBackground.texture = slotLockedBG;
                    slot.slotBackground.color = normalColor;
                }
            }

            if (slot.rewardImage != null)
                slot.rewardImage.color = isClaimed ? claimedDimColor : normalColor;

            if (slot.claimedText != null)
                slot.claimedText.gameObject.SetActive(isClaimed);

            if (slot.rewardText != null)
                slot.rewardText.gameObject.SetActive(!isClaimed);
        }

        if (claimButton != null)
        {
            claimButton.interactable = canClaim;

            Image btnImage = claimButton.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = canClaim ? claimActiveColor : claimInactiveColor;
        }
    }

    private void OnClaimButtonClicked()
    {
        if (SaveSystem.Instance == null) return;

        var data = SaveSystem.Instance.Data;
        int daysSince = SaveSystem.Instance.GetDaysSinceLastClaim();
        int nextDay = data.DailyRewardDay + 1;
        if (daysSince < 0 || daysSince > 1) nextDay = 0;
        if (nextDay >= 7) nextDay = 0;

        if (nextDay < dayConfigs.Count && dayConfigs[nextDay].goldAmount > 0)
        {
            if (nextDay < SaveSystem.DailyGoldRewards.Length)
                SaveSystem.DailyGoldRewards[nextDay] = dayConfigs[nextDay].goldAmount;
        }

        int goldReceived = SaveSystem.Instance.ClaimDailyReward();

        if (goldReceived > 0)
            Debug.Log($"[DailyRewardUI] Đã nhận {goldReceived} vàng!");

        RefreshUI();
    }
}
