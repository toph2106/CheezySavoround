using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager Instance { get; private set; }

    [Header("Booster Buttons")]
    public UnityEngine.UI.Button[] boosterButtons;

    [Header("Booster IDs")]
    public string[] boosterIDs = { "11", "12", "13", "14" };

    [Header("Visual Feedback")]
    public Color outlineColor = new Color(1f, 0f, 0f, 1f);
    public Vector2 outlineSize = new Vector2(6f, 6f);

    public string ActiveBoosterID { get; private set; } = "";

    public bool IsTargeting => !string.IsNullOrEmpty(ActiveBoosterID);

    private Camera _mainCamera;
    private int _activeButtonIndex = -1;
    private Outline[] _buttonOutlines;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _mainCamera = Camera.main;

        if (boosterButtons != null)
        {
            _buttonOutlines = new Outline[boosterButtons.Length];
            for (int i = 0; i < boosterButtons.Length; i++)
            {
                if (boosterButtons[i] == null) continue;

                Outline outline = boosterButtons[i].GetComponent<Outline>();
                if (outline == null)
                    outline = boosterButtons[i].gameObject.AddComponent<Outline>();

                outline.effectColor = outlineColor;
                outline.effectDistance = outlineSize;
                outline.enabled = false;
                _buttonOutlines[i] = outline;
            }
        }

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (boosterButtons == null) return;

        int count = Mathf.Min(boosterButtons.Length, boosterIDs.Length);
        for (int i = 0; i < count; i++)
        {
            if (boosterButtons[i] == null) continue;

            int idx = i;
            boosterButtons[i].onClick.RemoveAllListeners();
            boosterButtons[i].onClick.AddListener(() => ActivateBooster(idx));
        }
    }

    public void ActivateBooster(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= boosterIDs.Length) return;

        string id = boosterIDs[buttonIndex];

        if (ActiveBoosterID == id)
        {
            CancelBooster();
            return;
        }

        int count = PlayerPrefs.GetInt(id, 0);
        if (count <= 0)
        {
            ShopController shop = ShopController.Instance;
            if (shop == null)
            {
                shop = FindFirstObjectByType<ShopController>(FindObjectsInactive.Include);
            }
            if (shop != null)
            {
                shop.OpenShop();
                shop.ShowBooster();
            }
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying()) return;

        CancelBooster();

        ActiveBoosterID = id;
        _activeButtonIndex = buttonIndex;

        HighlightButton(buttonIndex, true);
    }

    public void CancelBooster()
    {
        if (_activeButtonIndex >= 0)
            HighlightButton(_activeButtonIndex, false);

        ActiveBoosterID = "";
        _activeButtonIndex = -1;
    }

    void Update()
    {
        if (!IsTargeting) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying()) return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBooster();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                PlateItem plate = hit.collider.GetComponent<PlateItem>();
                if (plate == null) plate = hit.collider.GetComponentInParent<PlateItem>();

                if (plate != null && plate.mySlot != null && !plate.isExploding)
                {
                    ExecuteBooster(ActiveBoosterID, plate);
                }
                else
                {
                }
            }
        }
    }

    private void ExecuteBooster(string id, PlateItem plate)
    {
        bool success = false;

        switch (id)
        {
            case "11": success = ExecuteCutter(plate); break;
            case "12": success = ExecuteSauce(plate); break;
            case "13": success = ExecuteTrash(plate); break;
            case "14": success = ExecuteExtraSlice(plate); break;
        }

        if (success)
        {
            GameplayHUD.UseBooster(id);

            var hud = FindFirstObjectByType<GameplayHUD>();
            if (hud != null) hud.RefreshAll();
            if (id != "13" && plate != null && plate.mySlot != null && GridManager.Instance != null)
            {
                GridManager.Instance.ProcessMergesAt(plate.mySlot);
            }
        }

        CancelBooster();
    }

    private bool ExecuteCutter(PlateItem plate)
    {
        if (plate.pizzaSlicesOnPlate.Count == 0)
        {
            Debug.Log("[Booster] Đĩa rỗng, không có gì để cắt!");
            return false;
        }

        int majorityType = GetMajorityType(plate);
        PizzaItem target = null;

        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice != null && slice.pizzaType != majorityType)
            {
                target = slice;
                break;
            }
        }

        if (target == null)
            target = plate.pizzaSlicesOnPlate[plate.pizzaSlicesOnPlate.Count - 1];

        if (target == null) return false;

        plate.pizzaSlicesOnPlate.Remove(target);
        Destroy(target.gameObject);

        plate.RearrangeSlicesAnimated();

        if (plate.pizzaSlicesOnPlate.Count == 0)
        {
            if (plate.mySlot != null)
            {
                plate.mySlot.isEmpty = true;
                plate.mySlot.currentPlate = null;
            }
            Destroy(plate.gameObject);
        }

        Debug.Log($"[Booster] Cutter: Đã cắt 1 miếng pizza type {target.pizzaType}");
        return true;
    }

    private bool ExecuteSauce(PlateItem plate)
    {
        if (plate.pizzaSlicesOnPlate.Count < 2)
        {
            Debug.Log("[Booster] Cần ít nhất 2 miếng pizza để dùng Sauce!");
            return false;
        }

        int majorityType = GetMajorityType(plate);

        PizzaItem target = null;
        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice != null && slice.pizzaType != majorityType)
            {
                target = slice;
                break;
            }
        }

        if (target == null)
        {
            Debug.Log("[Booster] Tất cả miếng đã cùng loại rồi!");
            return false;
        }

        int oldType = target.pizzaType;
        target.pizzaType = majorityType;

        ApplyPizzaVisual(target, majorityType, plate);

        Debug.Log($"[Booster] Sauce: Đổi miếng pizza từ type {oldType} → {majorityType}");

        CheckAndBloom(plate);

        return true;
    }
    private bool ExecuteTrash(PlateItem plate)
    {
        if (plate.mySlot == null) return false;

        Slot slot = plate.mySlot;

        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice != null) Destroy(slice.gameObject);
        }
        plate.pizzaSlicesOnPlate.Clear();

        slot.isEmpty = true;
        slot.currentPlate = null;

        Destroy(plate.gameObject);

        Debug.Log("[Booster] Trash: Đã xóa đĩa và giải phóng ô!");
        return true;
    }
    private bool ExecuteExtraSlice(PlateItem plate)
    {
        if (plate.pizzaSlicesOnPlate.Count >= 6)
        {
            Debug.Log("[Booster] Đĩa đã đầy 6 miếng!");
            return false;
        }

        if (plate.pizzaSlicesOnPlate.Count == 0)
        {
            Debug.Log("[Booster] Đĩa rỗng, không biết thêm loại gì!");
            return false;
        }

        int majorityType = GetMajorityType(plate);

        GameObject prefab = FindPizzaPrefab(majorityType, plate);
        if (prefab == null)
        {
            Debug.Log($"[Booster] Không tìm thấy prefab pizza type {majorityType}!");
            return false;
        }

        GameObject newSliceObj = Instantiate(prefab, plate.transform);
        PizzaItem newSlice = newSliceObj.GetComponent<PizzaItem>();
        if (newSlice == null)
        {
            Destroy(newSliceObj);
            return false;
        }
        newSlice.pizzaType = majorityType;
        newSlice.myPlate = plate;
        newSlice.mySlot = plate.mySlot;

        plate.pizzaSlicesOnPlate.Add(newSlice);

        int rotIdx = plate.pizzaSlicesOnPlate.Count - 1;
        newSlice.MoveTo(rotIdx);
        CheckAndBloom(plate);

        return true;
    }

    private int GetMajorityType(PlateItem plate)
    {
        Dictionary<int, int> typeCounts = new Dictionary<int, int>();
        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice == null) continue;
            if (!typeCounts.ContainsKey(slice.pizzaType))
                typeCounts[slice.pizzaType] = 0;
            typeCounts[slice.pizzaType]++;
        }

        int bestType = 0;
        int bestCount = 0;
        foreach (var kvp in typeCounts)
        {
            if (kvp.Value > bestCount)
            {
                bestCount = kvp.Value;
                bestType = kvp.Key;
            }
        }
        return bestType;
    }

    private GameObject FindPizzaPrefab(int pizzaType, PlateItem plate)
    {
        if (plate.pizzaPrefabs != null)
        {
            foreach (var prefab in plate.pizzaPrefabs)
            {
                if (prefab == null) continue;
                PizzaItem pi = prefab.GetComponent<PizzaItem>();
                if (pi != null && pi.pizzaType == pizzaType)
                    return prefab;
            }
        }

        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice != null && slice.pizzaType == pizzaType)
            {
                // Tạo clone
                return slice.gameObject;
            }
        }

        return null;
    }

    private void ApplyPizzaVisual(PizzaItem target, int newType, PlateItem plate)
    {
        PizzaItem reference = null;
        foreach (var slice in plate.pizzaSlicesOnPlate)
        {
            if (slice != null && slice != target && slice.pizzaType == newType)
            {
                reference = slice;
                break;
            }
        }

        if (reference == null) return;

        // Copy mesh và materials
        MeshFilter targetMF = target.GetComponentInChildren<MeshFilter>();
        MeshFilter refMF = reference.GetComponentInChildren<MeshFilter>();
        MeshRenderer targetMR = target.GetComponentInChildren<MeshRenderer>();
        MeshRenderer refMR = reference.GetComponentInChildren<MeshRenderer>();

        if (targetMF != null && refMF != null)
            targetMF.sharedMesh = refMF.sharedMesh;

        if (targetMR != null && refMR != null)
            targetMR.sharedMaterials = refMR.sharedMaterials;
    }

    private void CheckAndBloom(PlateItem plate)
    {
        if (plate == null || plate.pizzaSlicesOnPlate.Count != 6) return;

        int firstType = plate.pizzaSlicesOnPlate[0].pizzaType;
        bool allSame = true;
        foreach (var s in plate.pizzaSlicesOnPlate)
        {
            if (s == null || s.pizzaType != firstType) { allSame = false; break; }
        }

        if (allSame)
        {
            plate.ExplodePlate();
        }
    }

    private void HighlightButton(int index, bool highlight)
    {
        if (_buttonOutlines == null || index < 0 || index >= _buttonOutlines.Length) return;
        if (_buttonOutlines[index] == null) return;

        _buttonOutlines[index].effectColor = outlineColor;
        _buttonOutlines[index].effectDistance = outlineSize;
        _buttonOutlines[index].enabled = highlight;
    }
}
