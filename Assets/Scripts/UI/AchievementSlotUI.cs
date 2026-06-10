using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;
    public TextMeshProUGUI claimButtonText;
    public GameObject completedOverlay;

    [Header("Màu trạng thái")]
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color unlockedColor = new Color(1f, 0.85f, 0.3f, 1f);
    public Color claimedColor = new Color(0.3f, 0.7f, 0.3f, 1f);

    private string _achievementID;
    private AchievementConfig _config;

    public void Setup(AchievementConfig config)
    {
        _config = config;
        _achievementID = config.achievementID;

        if (nameText != null)
            nameText.text = config.displayName;

        if (descriptionText != null)
            descriptionText.text = config.description;

        if (iconImage != null && config.icon != null)
            iconImage.texture = config.icon.texture;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimClicked);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (_config == null || AchievementManager.Instance == null) return;

        var (progress, target, unlocked, claimed) = AchievementManager.Instance.GetProgress(_achievementID);

        if (progressBar != null)
        {
            progressBar.maxValue = target;
            progressBar.value = progress;
        }

        if (progressText != null)
            progressText.text = $"{progress} / {target}";

        if (claimButton != null)
        {
            if (claimed)
            {
                claimButton.interactable = false;
                if (claimButtonText != null) claimButtonText.text = "ĐÃ NHẬN";
            }
            else if (unlocked)
            {
                claimButton.interactable = true;
                if (claimButtonText != null) claimButtonText.text = $"NHẬN +{_config.goldReward}";
            }
            else
            {
                claimButton.interactable = false;
                if (claimButtonText != null) claimButtonText.text = "CHƯA ĐẠT";
            }
        }

        if (completedOverlay != null)
            completedOverlay.SetActive(claimed);

        if (nameText != null)
        {
            if (claimed) nameText.color = claimedColor;
            else if (unlocked) nameText.color = unlockedColor;
            else nameText.color = lockedColor;
        }
    }

    private void OnClaimClicked()
    {
        if (AchievementManager.Instance == null) return;

        bool success = AchievementManager.Instance.ClaimReward(_achievementID);
        if (success)
        {
            Refresh();
        }
    }
}
