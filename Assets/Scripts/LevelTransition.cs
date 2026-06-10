using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransition : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Thời gian bay vào/ra (giây)")]
    public float transitionDuration = 0.6f;

    [Tooltip("Hướng bay: từ bên nào bay vào")]
    public SlideDirection direction = SlideDirection.Left;

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Vector2 _hiddenPos;
    private Vector2 _shownPos;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _shownPos = Vector2.zero;

        CalculateHiddenPosition();

        _rect.anchoredPosition = _hiddenPos;
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void CalculateHiddenPosition()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        switch (direction)
        {
            case SlideDirection.Left:
                _hiddenPos = new Vector2(-screenWidth, 0);
                break;
            case SlideDirection.Right:
                _hiddenPos = new Vector2(screenWidth, 0);
                break;
            case SlideDirection.Top:
                _hiddenPos = new Vector2(0, screenHeight);
                break;
            case SlideDirection.Bottom:
                _hiddenPos = new Vector2(0, -screenHeight);
                break;
        }
    }

    public void PlayTransitionIn()
    {
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;
        CalculateHiddenPosition();
        StartCoroutine(SlideRoutine(_hiddenPos, _shownPos, 0f, 1f));
    }
    public void PlayTransitionOut()
    {
        CalculateHiddenPosition();

        Vector2 exitPos;
        switch (direction)
        {
            case SlideDirection.Left:
                exitPos = new Vector2(Screen.width, 0);
                break;
            case SlideDirection.Right:
                exitPos = new Vector2(-Screen.width, 0);
                break;
            case SlideDirection.Top:
                exitPos = new Vector2(0, -Screen.height);
                break;
            case SlideDirection.Bottom:
                exitPos = new Vector2(0, Screen.height);
                break;
            default:
                exitPos = _hiddenPos;
                break;
        }

        StartCoroutine(SlideOutRoutine(_shownPos, exitPos, 1f, 0f));
    }

    private IEnumerator SlideRoutine(Vector2 from, Vector2 to, float alphaFrom, float alphaTo)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            float eased = 1f - (1f - t) * (1f - t);

            _rect.anchoredPosition = Vector2.Lerp(from, to, eased);
            _canvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, eased);

            yield return null;
        }

        _rect.anchoredPosition = to;
        _canvasGroup.alpha = alphaTo;
    }

    private IEnumerator SlideOutRoutine(Vector2 from, Vector2 to, float alphaFrom, float alphaTo)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            float eased = t * t;

            _rect.anchoredPosition = Vector2.Lerp(from, to, eased);
            _canvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, eased);

            yield return null;
        }

        _rect.anchoredPosition = to;
        _canvasGroup.alpha = alphaTo;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
