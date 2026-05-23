using UnityEngine;
using System.Collections;
using TMPro;

public class PoolableObject : MonoBehaviour
{
    [HideInInspector]
    public string prefabName;
    
    [Header("Settings")]
    public float lifeTime = 1.5f;
    public float floatSpeed = 1.5f;

    private TextMeshPro _textComponent;
    private ParticleSystem _particleSystem;

    void Awake()
    {
        _textComponent = GetComponent<TextMeshPro>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        StartCoroutine(AutoReturnRoutine());

        // Nếu là Text điểm số, tự động bay lên
        if (_textComponent != null)
        {
            StartCoroutine(FloatingTextRoutine());
        }

        // Nếu là Particle, tự động play
        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }
    }

    private IEnumerator AutoReturnRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(gameObject, prefabName);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FloatingTextRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;
            // Di chuyển dọc theo trục Z (bay lên trên theo màn hình top-down)
            transform.position = startPos + Vector3.forward * (elapsed * floatSpeed);
            yield return null;
        }
    }
}
