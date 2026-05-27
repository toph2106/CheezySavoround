using UnityEngine;
using System.Collections.Generic;

public class PlateItem : MonoBehaviour
{
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
    private GridManager _gridManager;

    void Start()
    {
        _gridManager = FindFirstObjectByType<GridManager>();

        if (pizzaPrefabs.Length > 0 && pizzaSlicesOnPlate.Count == 0)
        {
            int count = Random.Range(minSlices, maxSlices + 1);
            for (int i = 0; i < count; i++)
            {
                int idx = Random.Range(0, pizzaPrefabs.Length);
                GameObject obj = Instantiate(pizzaPrefabs[idx], transform.position, Quaternion.identity, transform);
                obj.transform.localPosition = new Vector3(0, 0.7f, 0);
                obj.transform.localRotation = Quaternion.Euler(0, i * 60f, 0);

                PizzaItem slice = obj.GetComponent<PizzaItem>();
                slice.myPlate = this;
                pizzaSlicesOnPlate.Add(slice);
            }
        }
    }

    void Update()
    {
        if (_isPlaced || isExploding) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsInteractable()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider.gameObject == gameObject)
            {
                _isDragging = true;
                _startPos = transform.position;
            }
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, dragHeight, 0));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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

        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.PlaySquashStretch(transform);
        }

        if (_gridManager != null)
            _gridManager.ProcessMergesAt(mySlot);
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

        // === Game Juice: Pitch Shift âm thanh nổ theo combo ===
        if (GameJuice.Instance != null)
        {
            GameJuice.Instance.PlayExplosionSound();
        }

        // === Save System: Thưởng vàng khi nổ đĩa ===
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.AddGold(10);
        }

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