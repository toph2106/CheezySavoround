using UnityEngine;

public class GhostPreview : MonoBehaviour
{
    public static GhostPreview Instance { get; private set; }

    [Header("Ghost Settings")]
    [Range(0f, 1f)]
    public float ghostAlpha = 0.35f;

    public Color ghostTint = new Color(0.5f, 1f, 0.7f, 1f);

    public Color invalidTint = new Color(1f, 0.4f, 0.4f, 1f);

    private GameObject _ghostObject;
    private Material _ghostMaterial;
    private Slot _currentSlot;
    private bool _lastValidState = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(Slot slot, PlateItem plate, bool isValid = true)
    {
        if (slot == null || plate == null) return;

        if (_currentSlot == slot && _lastValidState == isValid && _ghostObject != null && _ghostObject.activeSelf)
            return;

        if (_ghostObject == null)
        {
            CreateGhost(plate);
        }

        if (_ghostObject == null) return;

        Vector3 pos = slot.transform.position;
        pos.y += 0.2f;
        _ghostObject.transform.position = pos;
        _ghostObject.transform.rotation = Quaternion.identity;
        
        _ghostObject.transform.localScale = plate.transform.lossyScale * 1.05f;

        if (_ghostMaterial != null)
        {
            Color color = isValid ? ghostTint : invalidTint;
            color.a = ghostAlpha;

            if (_ghostMaterial.HasProperty("_Color"))
                _ghostMaterial.SetColor("_Color", color);

            if (_ghostMaterial.HasProperty("_BaseColor"))
                _ghostMaterial.SetColor("_BaseColor", color);
        }

        _ghostObject.SetActive(true);

        _currentSlot = slot;
        _lastValidState = isValid;
    }

    public void Hide()
    {
        if (_ghostObject != null)
            _ghostObject.SetActive(false);

        _currentSlot = null;
    }

    public void DestroyGhost()
    {
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
            _ghostObject = null;
        }

        if (_ghostMaterial != null)
        {
            Destroy(_ghostMaterial);
            _ghostMaterial = null;
        }

        _currentSlot = null;
    }

    private void CreateGhost(PlateItem plate)
    {
        MeshFilter sourceMF = plate.GetComponent<MeshFilter>();
        MeshRenderer sourceMR = plate.GetComponent<MeshRenderer>();

        if (sourceMF == null || sourceMF.sharedMesh == null) return;

        _ghostObject = new GameObject("GhostPreview");

        MeshFilter ghostMF = _ghostObject.AddComponent<MeshFilter>();
        ghostMF.sharedMesh = sourceMF.sharedMesh;

        MeshRenderer ghostMR = _ghostObject.AddComponent<MeshRenderer>();
        _ghostMaterial = CreateTransparentMaterial(sourceMR.sharedMaterial);
        ghostMR.material = _ghostMaterial;

        ghostMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ghostMR.receiveShadows = false;

        _ghostObject.SetActive(false);
    }

    private Material CreateTransparentMaterial(Material source)
    {
        Material mat = new Material(source);

        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3f); 
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f); 
            mat.SetFloat("_Blend", 0f);  
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.renderQueue = 3000;
        }

        return mat;
    }
}
