using UnityEngine;
using System;
using System.Collections.Generic;

public class PlateItem : MonoBehaviour
{
    public static event Action OnAnyPlateExploded;
    public static event Action OnAnyPlatePlaced;
    [Header("Spawner Settings")]
    public GameObject[] pizzaPrefabs;
    public int minSlices = 1;
    public int maxSlices = 3;

    [Header("Drag Settings")]
    public float dragHeight = 2f;
    public LayerMask slotLayer;

    public List<PizzaItem> pizzaSlicesOnPlate = new List<PizzaItem>();
    public bool isExploding = false;
    public Slot mySlot;

    private Vector3 _startPos;
    private Slot _hoveredSlot;
    private bool _isDragging = false;
    private bool _isPlaced = false;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;

        if (SkinManager.Instance != null)
            SkinManager.Instance.RegisterPlate(this);

        if (pizzaPrefabs.Length > 0 && pizzaSlicesOnPlate.Count == 0)
        {
            // Lấy level hiện tại để xác định số loại pizza + số miếng
            int currentLevel = 1;
            if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
                currentLevel = SaveSystem.Instance.Data.CurrentLevel;

            // Số loại pizza được phép (vd: level 1-3 chỉ 2 loại, level 4-7 là 3 loại...)
            int allowedTypes = GameManager.GetPizzaTypeCount(currentLevel);
            int maxPrefabIndex = Mathf.Min(allowedTypes, pizzaPrefabs.Length);

            // Số miếng pizza trên đĩa theo level
            int levelMin = GameManager.GetMinSlices(currentLevel);
            int levelMax = GameManager.GetMaxSlices(currentLevel);
            // Clamp với giá trị Inspector (nếu bạn muốn override)
            int finalMin = Mathf.Max(minSlices, levelMin);
            int finalMax = Mathf.Min(maxSlices, levelMax);
            if (finalMax < finalMin) finalMax = finalMin;

            int count = UnityEngine.Random.Range(finalMin, finalMax + 1);
            for (int i = 0; i < count; i++)
            {
                // Chỉ random trong N loại đầu tiên
                int idx = UnityEngine.Random.Range(0, maxPrefabIndex);
                GameObject obj = Instantiate(pizzaPrefabs[idx], transform.position, Quaternion.identity, transform);
                obj.transform.localPosition = new Vector3(0, 0.7f, 0);
                obj.transform.localRotation = Quaternion.Euler(0, i * 60f, 0);

                PizzaItem slice = obj.GetComponent<PizzaItem>();
                slice.myPlate = this;
                pizzaSlicesOnPlate.Add(slice);
            }
        }
    }

    void OnDestroy()
    {
        if (SkinManager.Instance != null)
            SkinManager.Instance.UnregisterPlate(this);
    }

    void Update()
    {
        if (_isPlaced || isExploding) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsInteractable()) return;

        // Chặn kéo đĩa khi đang chọn booster
        if (BoosterManager.Instance != null && BoosterManager.Instance.IsTargeting) return;

        if (_mainCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider.gameObject == gameObject)
            {
                _isDragging = true;
                _startPos = transform.position;
            }
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float dist))
                transform.position = ray.GetPoint(dist);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, slotLayer))
                hit.collider.TryGetComponent(out _hoveredSlot);
            else
                _hoveredSlot = null;

            if (GhostPreview.Instance != null)
            {
                if (_hoveredSlot != null)
                {
                    bool isValid = _hoveredSlot.isEmpty;
                    GhostPreview.Instance.Show(_hoveredSlot, this, isValid);
                }
                else
                {
                    GhostPreview.Instance.Hide();
                }
            }
        }

        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            _isDragging = false;

            if (GhostPreview.Instance != null)
                GhostPreview.Instance.DestroyGhost();

            if (_hoveredSlot != null && _hoveredSlot.isEmpty)
            {
                PlacePlateOnSlot(_hoveredSlot);
            }
            else
            {
                if (_hoveredSlot != null && !_hoveredSlot.isEmpty && GameJuice.Instance != null)
                {
                    GameJuice.Instance.PlayShake(_hoveredSlot.transform);

                    if (_hoveredSlot.currentPlate != null)
                    {
                        GameJuice.Instance.PlayShake(_hoveredSlot.currentPlate.transform);
                    }
                }
                transform.position = _startPos;
            }
        }
    }

    public void PlacePlateOnSlot(Slot slot)
    {
        Vector3 pos = slot.transform.position;
        pos.y += 0.2f;
        transform.position = pos;

        transform.SetParent(null); 

        _isPlaced = true;
        slot.isEmpty = false;
        slot.currentPlate = this;
        mySlot = slot;

        foreach (PizzaItem slice in pizzaSlicesOnPlate)
            slice.mySlot = mySlot;

        if (TrayManager.Instance != null)
        {
            TrayManager.Instance.OnPlatePlaced();
        }

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.Data.TotalPlatesPlaced++;
        OnAnyPlatePlaced?.Invoke();

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.PlaySquashStretch(transform);
        }

        if (GridManager.Instance != null)
            GridManager.Instance.ProcessMergesAt(mySlot);
    }

    public void ExplodePlate()
    {
        if (isExploding) return;
        isExploding = true;

        if (mySlot != null)
        {
            mySlot.isEmpty = true;
            mySlot.currentPlate = null;
        }

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.PlayExplosionSound();
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.AddGold(10);
            SaveSystem.Instance.Data.TotalPlatesExploded++;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(100);

        OnAnyPlateExploded?.Invoke();

        if (GameManager.Instance != null && ObjectPooler.Instance != null)
        {
            if (GameManager.Instance.explosionPrefab != null)
            {
                ObjectPooler.Instance.SpawnFromPool(GameManager.Instance.explosionPrefab, transform.position, GameManager.Instance.explosionPrefab.transform.rotation);
            }
            if (GameManager.Instance.floatingTextPrefab != null)
            {
                ObjectPooler.Instance.SpawnFromPool(GameManager.Instance.floatingTextPrefab, transform.position, GameManager.Instance.floatingTextPrefab.transform.rotation);
            }
        }

        foreach (PizzaItem s in pizzaSlicesOnPlate)
            if (s != null) Destroy(s.gameObject);

        pizzaSlicesOnPlate.Clear();
        Destroy(gameObject);
    }

    public void RearrangeSlicesAnimated()
    {
        pizzaSlicesOnPlate.RemoveAll(s => s == null);
        for (int i = 0; i < pizzaSlicesOnPlate.Count; i++)
            pizzaSlicesOnPlate[i].MoveTo(i);
    }

    public void RearrangeSlicesImmediate()
    {
        pizzaSlicesOnPlate.RemoveAll(s => s == null);
        for (int i = 0; i < pizzaSlicesOnPlate.Count; i++)
            pizzaSlicesOnPlate[i].SnapTo(i);
    }
}