using UnityEngine;
using System;
using System.Collections;

public class GameJuice : MonoBehaviour
{
    public static GameJuice Instance { get; private set; }

    public event Action<int> OnComboAchieved;

    [Header("Squash & Stretch")]
    public float squashDuration = 0.3f;
    public float squashScaleXZ = 1.25f;

    public float squashScaleY = 0.7f;

    public float stretchScaleY = 1.2f;

    public float stretchScaleXZ = 0.85f;

    [Header("Shake")]
    public float shakeDuration = 0.3f;

    public float shakeIntensity = 0.15f;

    public float shakeFrequency = 30f;

    [Header("Pitch Shift - Âm thanh nổ")]
    public AudioClip explosionClip;

    public AudioClip placeClip;

    [Tooltip("Pitch cơ bản")]
    public float basePitch = 1.0f;

    public float pitchStep = 0.1f;

    public float maxPitch = 1.6f;

    private AudioSource _audioSource;

    private int _comboCount = 0;
    public int ComboCount => _comboCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;
    }

    public void PlaySquashStretch(Transform target)
    {
        if (target == null) return;
        StartCoroutine(SquashStretchRoutine(target));
    }

    private IEnumerator SquashStretchRoutine(Transform target)
    {
        PlayPlaceSound();

        float elapsed = 0f;

        Vector3 originalScale = target.localScale;
        float origX = originalScale.x;
        float origY = originalScale.y;
        float origZ = originalScale.z;

        while (elapsed < squashDuration)
        {
            if (target == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / squashDuration;

            float mulXZ, mulY;

            if (t < 0.25f)
            {
                float p = t / 0.25f;
                float ease = Mathf.Sin(p * Mathf.PI * 0.5f);
                mulXZ = Mathf.Lerp(1f, squashScaleXZ, ease);
                mulY = Mathf.Lerp(1f, squashScaleY, ease);
            }
            else if (t < 0.5f)
            {
                float p = (t - 0.25f) / 0.25f;
                float ease = Mathf.Sin(p * Mathf.PI * 0.5f);
                mulXZ = Mathf.Lerp(squashScaleXZ, stretchScaleXZ, ease);
                mulY = Mathf.Lerp(squashScaleY, stretchScaleY, ease);
            }
            else if (t < 0.75f)
            {
                float p = (t - 0.5f) / 0.25f;
                float ease = Mathf.Sin(p * Mathf.PI * 0.5f);
                mulXZ = Mathf.Lerp(stretchScaleXZ, 1.05f, ease);
                mulY = Mathf.Lerp(stretchScaleY, 0.95f, ease);
            }
            else
            {
                float p = (t - 0.75f) / 0.25f;
                float ease = Mathf.Sin(p * Mathf.PI * 0.5f);
                mulXZ = Mathf.Lerp(1.05f, 1f, ease);
                mulY = Mathf.Lerp(0.95f, 1f, ease);
            }

            target.localScale = new Vector3(origX * mulXZ, origY * mulY, origZ * mulXZ);
            yield return null;
        }

        if (target != null)
            target.localScale = originalScale;
    }

    public void PlayShake(Transform target)
    {
        if (target == null) return;
        StartCoroutine(ShakeRoutine(target));
    }

    private IEnumerator ShakeRoutine(Transform target)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            if (target == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;

            float decay = 1f - t;
            float currentIntensity = shakeIntensity * decay;

            float offsetX = Mathf.Sin(elapsed * shakeFrequency) * currentIntensity;
            float offsetZ = Mathf.Cos(elapsed * shakeFrequency * 0.7f) * currentIntensity * 0.5f;

            target.localPosition = originalPos + new Vector3(offsetX, 0f, offsetZ);
            yield return null;
        }

        if (target != null)
            target.localPosition = originalPos;
    }

    public void ResetCombo()
    {
        _comboCount = 0;
    }

    public void PlayExplosionSound()
    {
        if (explosionClip == null || _audioSource == null) return;

        float pitch = basePitch + (_comboCount * pitchStep);
        pitch = Mathf.Min(pitch, maxPitch);

        _audioSource.pitch = pitch;
        _audioSource.PlayOneShot(explosionClip);

        _comboCount++;

        OnComboAchieved?.Invoke(_comboCount);
        if (SaveSystem.Instance != null && _comboCount > SaveSystem.Instance.Data.HighestCombo)
            SaveSystem.Instance.Data.HighestCombo = _comboCount;
    }

    private void PlayPlaceSound()
    {
        if (placeClip == null || _audioSource == null) return;

        _audioSource.pitch = 1f;
        _audioSource.PlayOneShot(placeClip);
    }
}
