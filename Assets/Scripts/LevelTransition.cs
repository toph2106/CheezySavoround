using System.Collections;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [Header("Cài Đặt Thời Gian")]
    public float transitionDuration = 0.6f;

    [Header("2 Đám Mây Đóng Mở Cửa")]
    public RectTransform leftCloud;
    public RectTransform rightCloud;

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private float GetOffscreenOffset()
    {
        RectTransform rect = GetComponent<RectTransform>();
        return rect.rect.width > 0 ? rect.rect.width : Screen.width;
    }

    public void PlayTransitionIn()
    {
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        float offset = GetOffscreenOffset();

        if (leftCloud != null) leftCloud.anchoredPosition = new Vector2(-offset, 0);
        if (rightCloud != null) rightCloud.anchoredPosition = new Vector2(offset, 0);

        StartCoroutine(SlideInRoutine(offset));
    }

    public void PlayTransitionOut()
    {
        StartCoroutine(SlideOutRoutine(GetOffscreenOffset()));
    }

    private IEnumerator SlideInRoutine(float offset)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float eased = 1f - (1f - t) * (1f - t);

            if (leftCloud != null)
                leftCloud.anchoredPosition = Vector2.Lerp(new Vector2(-offset, 0), Vector2.zero, eased);

            if (rightCloud != null)
                rightCloud.anchoredPosition = Vector2.Lerp(new Vector2(offset, 0), Vector2.zero, eased);

            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
            yield return null;
        }

        if (leftCloud != null) leftCloud.anchoredPosition = Vector2.zero;
        if (rightCloud != null) rightCloud.anchoredPosition = Vector2.zero;
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator SlideOutRoutine(float offset)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float eased = t * t;

            if (leftCloud != null)
                leftCloud.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-offset, 0), eased);

            if (rightCloud != null)
                rightCloud.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(offset, 0), eased);

            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
            yield return null;
        }

        if (leftCloud != null) leftCloud.anchoredPosition = new Vector2(-offset, 0);
        if (rightCloud != null) rightCloud.anchoredPosition = new Vector2(offset, 0);

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}