using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Toast thông báo khi unlock achievement.
/// Tự động trượt vào từ trên, hiện 3 giây rồi trượt ra.
/// 
/// Gắn lên 1 UI panel ở góc trên màn hình.
/// AchievementManager.OnAchievementUnlocked → hiện toast.
/// </summary>
public class AchievementToast : MonoBehaviour
{
    public static AchievementToast Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject toastPanel;
    public RawImage iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    [Header("Animation")]
    [Tooltip("Thời gian trượt vào/ra (giây)")]
    public float slideDuration = 0.4f;

    [Tooltip("Thời gian hiện toast (giây)")]
    public float displayDuration = 3f;

    [Tooltip("Khoảng cách trượt từ trên xuống (pixel)")]
    public float slideDistance = 150f;

    private RectTransform _panelRect;
    private Vector2 _hiddenPos;
    private Vector2 _shownPos;
    private Coroutine _currentToast;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (toastPanel != null)
        {
            _panelRect = toastPanel.GetComponent<RectTransform>();
            _shownPos = _panelRect.anchoredPosition;
            _hiddenPos = _shownPos + new Vector2(0, slideDistance);
            _panelRect.anchoredPosition = _hiddenPos;
            toastPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += ShowToast;
    }

    void OnDisable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= ShowToast;
    }

    public void ShowToast(AchievementConfig config)
    {
        if (toastPanel == null || _panelRect == null) return;

        // Cập nhật nội dung
        if (titleText != null)
            titleText.text = $"🏆 {config.displayName}";

        if (descText != null)
            descText.text = config.description;

        if (iconImage != null && config.icon != null)
            iconImage.texture = config.icon.texture;

        // Chạy animation
        if (_currentToast != null)
            StopCoroutine(_currentToast);

        _currentToast = StartCoroutine(ToastAnimation());
    }

    private IEnumerator ToastAnimation()
    {
        toastPanel.SetActive(true);
        _panelRect.anchoredPosition = _hiddenPos;

        // Slide in
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = 1f - (1f - t) * (1f - t); // ease out
            _panelRect.anchoredPosition = Vector2.Lerp(_hiddenPos, _shownPos, t);
            yield return null;
        }
        _panelRect.anchoredPosition = _shownPos;

        // Hiện
        yield return new WaitForSecondsRealtime(displayDuration);

        // Slide out
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            t = t * t; // ease in
            _panelRect.anchoredPosition = Vector2.Lerp(_shownPos, _hiddenPos, t);
            yield return null;
        }
        _panelRect.anchoredPosition = _hiddenPos;

        toastPanel.SetActive(false);
        _currentToast = null;
    }
}
