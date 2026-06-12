using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TouchEffectManager : MonoBehaviour
{
    [Header("TapVFX")]
    public GameObject imageTemplate;

    private Transform topCanvasTransform;

    void Start()
    {
        if (imageTemplate != null)
        {
            imageTemplate.SetActive(false);
        }
        GameObject canvasObj = new GameObject("Top_VFX_Canvas");
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 32000;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        topCanvasTransform = canvasObj.transform;
        DontDestroyOnLoad(canvasObj);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && imageTemplate != null && topCanvasTransform != null)
        {
            GameObject cloneEffect = Instantiate(imageTemplate, topCanvasTransform);
            cloneEffect.SetActive(true);
            cloneEffect.transform.position = Input.mousePosition;

            StartCoroutine(AnimateEffect(cloneEffect));
        }
    }

    private IEnumerator AnimateEffect(GameObject effectObj)
    {
        Graphic img = effectObj.GetComponent<Graphic>();
        Vector3 startScale = Vector3.zero;    
        Vector3 endScale = Vector3.one * 1.5f; 

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (effectObj == null) yield break;

            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            effectObj.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                img.color = c;
            }

            yield return null;
        }

        Destroy(effectObj);
    }
}