using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementToast : MonoBehaviour
{
    public static AchievementToast Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject toastPanel;
    public RawImage iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;

    [Header("Animation")]
    public float slideDuration = 0.4f;

    public float displayDuration = 3f;

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

        if (titleText != null)
            titleText.text = $"🏆 {config.displayName}";

        if (descText != null)
            descText.text = config.description;

        if (iconImage != null && config.icon != null)
            iconImage.texture = config.icon.texture;

        if (_currentToast != null)
            StopCoroutine(_currentToast);

        _currentToast = StartCoroutine(ToastAnimation());
    }

    private IEnumerator ToastAnimation()
    {
        toastPanel.SetActive(true);
        _panelRect.anchoredPosition = _hiddenPos;

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

        yield return new WaitForSecondsRealtime(displayDuration);

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
