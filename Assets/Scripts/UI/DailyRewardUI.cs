using System;
using System.Collections;
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

    [Header("Ảnh nền ô ngày")]
    [Tooltip("Ảnh nền ô bình thường / đã nhận (pu dalily 1)")]
    public Texture slotLockedBG;

    [Tooltip("Ảnh nền ô ĐANG CHỜ NHẬN — to hơn (pu dalily 1.1)")]
    public Texture slotReadyBG;

    [Header("Scale ô Ready (to hơn ô thường)")]
    [Tooltip("Nhân scale ô Ready so với gốc. Kéo lúc Play để chỉnh cho khớp demo")]
    [Range(1f, 2f)]
    public float readyScale = 1.3f;

    [Header("Màu ô ngày")]
    public Color normalColor = Color.white;
    public Color claimedDimColor = new Color(0.3f, 0.25f, 0.2f, 1f);

    [Header("Nút Claim")]
    public Button claimButton;

    [Header("Màu nút Claim")]
    public Color claimActiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color claimInactiveColor = new Color(0.3f, 0.25f, 0.2f, 1f);

    [Header("Hiệu ứng")]
    [Tooltip("Thời gian rải ô ra từ trái qua phải (giây)")]
    public float staggerDelay = 0.08f;

    private Dictionary<int, Vector3> _originalScales = new Dictionary<int, Vector3>();
    private Dictionary<string, Vector3> _originalChildScales = new Dictionary<string, Vector3>();
    private Coroutine _readyPulse;
    private Transform _readyPulseTarget;
    private Coroutine _staggerCoroutine;
    private bool _initialized;


    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        for (int i = 0; i < daySlotUIs.Count; i++)
        {
            var slot = daySlotUIs[i];
            if (slot == null) continue;

            if (slot.slotBackground != null)
            {
                Vector3 s = slot.slotBackground.transform.localScale;
                if (s != Vector3.zero)
                    _originalScales[i] = s;
            }

            SaveChildScale(i, "day", slot.dayLabel);
            SaveChildScale(i, "reward", slot.rewardText);
            SaveChildScale(i, "claimed", slot.claimedText);
            SaveChildScale(i, "icon", slot.rewardImage);
        }

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        }

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged += RefreshUI;
    }

    void OnEnable()
    {
        EnsureInitialized();
    }

    void OnDestroy()
    {
        StopAllPulseAndStagger();
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged -= RefreshUI;
    }


    public void OpenDaily()
    {
        if (dailyPanel != null) dailyPanel.SetActive(true);
        gameObject.SetActive(true);

        EnsureInitialized();
        RefreshUI();

        StartCoroutine(DelayedStagger());
    }

    public void CloseDaily()
    {
        StopAllPulseAndStagger();
        if (dailyPanel != null) dailyPanel.SetActive(false);
    }

    private IEnumerator DelayedStagger()
    {
        yield return null;
        PlayStaggerIn();
    }


    public void RefreshUI()
    {
        EnsureInitialized();
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
                Transform slotTf = slot.slotBackground.transform;

                if (isClaimed)
                {
                    if (slotLockedBG != null) slot.slotBackground.texture = slotLockedBG;
                    slot.slotBackground.color = claimedDimColor;
                    if (_originalScales.ContainsKey(i)) slotTf.localScale = _originalScales[i];
                }
                else if (isReady)
                {
                    if (slotReadyBG != null) slot.slotBackground.texture = slotReadyBG;
                    slot.slotBackground.color = normalColor;
                    if (_originalScales.ContainsKey(i))
                        slotTf.localScale = _originalScales[i] * readyScale;
                }
                else
                {
                    if (slotLockedBG != null) slot.slotBackground.texture = slotLockedBG;
                    slot.slotBackground.color = normalColor;
                    if (_originalScales.ContainsKey(i)) slotTf.localScale = _originalScales[i];
                }
            }

            if (isReady)
            {
                float inv = 1f / readyScale;
                ApplyChildScale(i, "day", slot.dayLabel, inv);
                ApplyChildScale(i, "reward", slot.rewardText, inv);
                ApplyChildScale(i, "claimed", slot.claimedText, inv);
                ApplyChildScale(i, "icon", slot.rewardImage, inv);
            }
            else
            {
                RestoreChildScale(i, "day", slot.dayLabel);
                RestoreChildScale(i, "reward", slot.rewardText);
                RestoreChildScale(i, "claimed", slot.claimedText);
                RestoreChildScale(i, "icon", slot.rewardImage);
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

        StopAllPulseAndStagger();
        int goldReceived = SaveSystem.Instance.ClaimDailyReward();

        if (goldReceived > 0)

        RefreshUI();
        StartPulseForReadySlot();
    }


    private int GetReadySlotIndex()
    {
        if (SaveSystem.Instance == null) return -1;
        var data = SaveSystem.Instance.Data;
        if (!SaveSystem.Instance.CanClaimDailyReward()) return -1;

        int daysSince = SaveSystem.Instance.GetDaysSinceLastClaim();
        int nextDay = data.DailyRewardDay + 1;
        if (daysSince < 0 || daysSince > 1) nextDay = 0;
        if (nextDay >= 7) nextDay = 0;
        return nextDay;
    }

    private void StartPulseForReadySlot()
    {
        StopPulse();
        int readyIdx = GetReadySlotIndex();
        if (readyIdx < 0 || readyIdx >= daySlotUIs.Count) return;

        var slot = daySlotUIs[readyIdx];
        if (slot == null || slot.slotBackground == null) return;

        _readyPulseTarget = slot.slotBackground.transform;
        _readyPulse = StartCoroutine(PulseReady(_readyPulseTarget));
    }

    private IEnumerator PulseReady(Transform target)
    {
        Vector3 baseScale = target.localScale;
        while (target != null)
        {
            float mul = Mathf.Lerp(0.96f, 1.04f, (Mathf.Sin(Time.unscaledTime * 3f) + 1f) * 0.5f);
            target.localScale = baseScale * mul;
            yield return null;
        }
    }

    private void PlayStaggerIn()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_staggerCoroutine != null) StopCoroutine(_staggerCoroutine);
        StopPulse();
        _staggerCoroutine = StartCoroutine(StaggerSlots());
    }

    private IEnumerator StaggerSlots()
    {
        int readyIdx = GetReadySlotIndex();

        List<Transform> slots = new List<Transform>();
        List<Vector3> targets = new List<Vector3>();

        for (int i = 0; i < daySlotUIs.Count; i++)
        {
            if (daySlotUIs[i] == null || daySlotUIs[i].slotBackground == null) continue;
            Transform t = daySlotUIs[i].slotBackground.transform;
            Vector3 baseScale = _originalScales.ContainsKey(i) ? _originalScales[i] : t.localScale;

            Vector3 targetScale = (i == readyIdx) ? baseScale * readyScale : baseScale;

            slots.Add(t);
            targets.Add(targetScale);
            t.localScale = Vector3.zero;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            StartCoroutine(PopScale(slots[i], targets[i], 0.25f));
            yield return new WaitForSecondsRealtime(staggerDelay);
        }

        yield return new WaitForSecondsRealtime(0.3f);
        StartPulseForReadySlot();
        _staggerCoroutine = null;
    }

    private IEnumerator PopScale(Transform target, Vector3 targetScale, float duration)
    {
        Vector3 overshoot = targetScale * 1.1f;
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            t = 1f - (1f - t) * (1f - t);
            target.localScale = Vector3.Lerp(Vector3.zero, overshoot, t);
            yield return null;
        }

        elapsed = 0f;
        float remaining = duration - half;
        while (elapsed < remaining && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / remaining);
            t = 1f - (1f - t) * (1f - t);
            target.localScale = Vector3.Lerp(overshoot, targetScale, t);
            yield return null;
        }

        if (target != null) target.localScale = targetScale;
    }


    private void StopPulse()
    {
        if (_readyPulse != null) { StopCoroutine(_readyPulse); _readyPulse = null; }
        _readyPulseTarget = null;
    }

    private void StopAllPulseAndStagger()
    {
        StopPulse();
        if (_staggerCoroutine != null) { StopCoroutine(_staggerCoroutine); _staggerCoroutine = null; }
    }


    private void SaveChildScale(int slotIdx, string key, Component child)
    {
        if (child == null) return;
        string id = $"{slotIdx}_{key}";
        _originalChildScales[id] = child.transform.localScale;
    }

    private void ApplyChildScale(int slotIdx, string key, Component child, float multiplier)
    {
        if (child == null) return;
        string id = $"{slotIdx}_{key}";
        if (_originalChildScales.ContainsKey(id))
        {
            Vector3 orig = _originalChildScales[id];
            child.transform.localScale = new Vector3(
                orig.x * multiplier,
                orig.y * multiplier,
                orig.z
            );
        }
    }

    private void RestoreChildScale(int slotIdx, string key, Component child)
    {
        if (child == null) return;
        string id = $"{slotIdx}_{key}";
        if (_originalChildScales.ContainsKey(id))
            child.transform.localScale = _originalChildScales[id];
    }
}
