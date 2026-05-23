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
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying()) return;

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
        }

        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            if (_hoveredSlot != null && _hoveredSlot.isEmpty)
                PlacePlateOnSlot(_hoveredSlot);
            else
                transform.position = _startPos;
        }
    }

    public void PlacePlateOnSlot(Slot slot)
    {
        Vector3 pos = slot.transform.position;
        pos.y += 0.2f;
        transform.position = pos;

        _isPlaced = true;
        slot.isEmpty = false;
        slot.currentPlate = this;
        mySlot = slot;

        foreach (PizzaItem slice in pizzaSlicesOnPlate)
            slice.mySlot = mySlot;

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