using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    public static UIAnimator Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ===================== PANEL SLIDE =====================

    public void SlideIn(RectTransform panel, SlideDirection direction = SlideDirection.Bottom, float duration = 0.4f, Action onComplete = null)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(true);

        Vector2 targetPos = panel.anchoredPosition;
        Vector2 startPos = targetPos;

        switch (direction)
        {
            case SlideDirection.Bottom: startPos.y -= Screen.height; break;
            case SlideDirection.Top: startPos.y += Screen.height; break;
            case SlideDirection.Left: startPos.x -= Screen.width; break;
            case SlideDirection.Right: startPos.x += Screen.width; break;
        }

        StartCoroutine(AnimatePosition(panel, startPos, targetPos, duration, EaseOutBack, onComplete));
    }

    public void SlideOut(RectTransform panel, SlideDirection direction = SlideDirection.Bottom, float duration = 0.3f, Action onComplete = null)
    {
        if (panel == null) return;

        Vector2 startPos = panel.anchoredPosition;
        Vector2 targetPos = startPos;

        switch (direction)
        {
            case SlideDirection.Bottom: targetPos.y -= Screen.height; break;
            case SlideDirection.Top: targetPos.y += Screen.height; break;
            case SlideDirection.Left: targetPos.x -= Screen.width; break;
            case SlideDirection.Right: targetPos.x += Screen.width; break;
        }

        StartCoroutine(AnimatePosition(panel, startPos, targetPos, duration, EaseInBack, () =>
        {
            panel.anchoredPosition = startPos;
            panel.gameObject.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    // ===================== SCALE POP =====================

    public void ScalePop(Transform target, float punchScale = 1.2f, float duration = 0.3f, Action onComplete = null)
    {
        if (target == null) return;
        StartCoroutine(AnimateScalePop(target, punchScale, duration, onComplete));
    }

    // ===================== PULSE (GIỮ NGUYÊN SCALE GỐC) =====================

    public Coroutine StartPulse(Transform target, float minMul = 0.97f, float maxMul = 1.05f, float speed = 1.5f)
    {
        if (target == null) return null;
        return StartCoroutine(PulseLoop(target, target.localScale, minMul, maxMul, speed));
    }

    public void StopPulse(Transform target, Coroutine pulseCoroutine)
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
    }

    // ===================== SHAKE =====================

    public void Shake(Transform target, float intensity = 8f, float duration = 0.3f)
    {
        if (target == null) return;
        StartCoroutine(ShakeLoop(target, intensity, duration));
    }

    // ===================== FADE =====================

    public void FadeIn(CanvasGroup cg, float duration = 0.3f, Action onComplete = null)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(true);
        StartCoroutine(AnimateFloat(0f, 1f, duration, EaseLinear, v => cg.alpha = v, onComplete));
    }

    public void FadeOut(CanvasGroup cg, float duration = 0.3f, Action onComplete = null)
    {
        if (cg == null) return;
        StartCoroutine(AnimateFloat(1f, 0f, duration, EaseLinear, v => cg.alpha = v, () =>
        {
            cg.gameObject.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    // ===================== COLOR FLASH =====================

    public void ColorFlash(Graphic target, Color flashColor, float duration = 0.4f)
    {
        if (target == null) return;
        StartCoroutine(FlashColor(target, flashColor, duration));
    }

    // ===================== STAGGER (GIỮ NGUYÊN SCALE GỐC) =====================

    public void StaggerPopIn(List<Transform> targets, float delay = 0.06f, float punchScale = 1.15f, float duration = 0.25f)
    {
        StartCoroutine(StaggerAnimation(targets, delay, punchScale, duration));
    }

    // ===================== COROUTINES =====================

    private IEnumerator AnimatePosition(RectTransform rt, Vector2 from, Vector2 to, float duration, Func<float, float> ease, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = ease(Mathf.Clamp01(elapsed / duration));
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, t);
            yield return null;
        }
        rt.anchoredPosition = to;
        onComplete?.Invoke();
    }

    private IEnumerator AnimateScalePop(Transform target, float punchScale, float duration, Action onComplete)
    {
        Vector3 original = target.localScale;
        Vector3 punch = original * punchScale;
        float half = duration * 0.4f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / half));
            if (target != null) target.localScale = Vector3.Lerp(original, punch, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration - half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutBounce(Mathf.Clamp01(elapsed / (duration - half)));
            if (target != null) target.localScale = Vector3.Lerp(punch, original, t);
            yield return null;
        }

        if (target != null) target.localScale = original;
        onComplete?.Invoke();
    }

    private IEnumerator PulseLoop(Transform target, Vector3 originalScale, float minMul, float maxMul, float speed)
    {
        while (target != null)
        {
            float mul = Mathf.Lerp(minMul, maxMul, (Mathf.Sin(Time.unscaledTime * speed * Mathf.PI * 2f) + 1f) * 0.5f);
            target.localScale = originalScale * mul;
            yield return null;
        }
    }

    private IEnumerator ShakeLoop(Transform target, float intensity, float duration)
    {
        Vector3 original = target.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float dampening = 1f - (elapsed / duration);
            float x = UnityEngine.Random.Range(-1f, 1f) * intensity * dampening;
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity * dampening;
            if (target != null) target.localPosition = original + new Vector3(x, y, 0);
            yield return null;
        }
        if (target != null) target.localPosition = original;
    }

    private IEnumerator AnimateFloat(float from, float to, float duration, Func<float, float> ease, Action<float> setter, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = ease(Mathf.Clamp01(elapsed / duration));
            setter(Mathf.Lerp(from, to, t));
            yield return null;
        }
        setter(to);
        onComplete?.Invoke();
    }

    private IEnumerator FlashColor(Graphic target, Color flashColor, float duration)
    {
        Color original = target.color;
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            target.color = Color.Lerp(original, flashColor, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            target.color = Color.Lerp(flashColor, original, elapsed / half);
            yield return null;
        }
        target.color = original;
    }

    private IEnumerator StaggerAnimation(List<Transform> targets, float delay, float punchScale, float duration)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            ScalePop(targets[i], punchScale, duration);
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    // ===================== EASING =====================

    private static float EaseLinear(float t) => t;
    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    private static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private static float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    private static float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
        if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }
}

public enum SlideDirection { Top, Bottom, Left, Right }
