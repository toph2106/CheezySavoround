using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisabledSlotData
{
    public int x;
    public int y;
}

[System.Serializable]
public class LevelData
{
    public int levelID;
    public int columns;
    public int rows;
    public DisabledSlotData[] disabledSlots;
}

public class GridManager : MonoBehaviour
{
    [Header("Grid Config")]
    public int currentLevel = 1;
    public GameObject tilePrefab;
    public Transform gridParent;
    public float spacing = 4f;

    [Header("Merge Timing")]
    public float initialDelay = 0.2f;
    public float moveWait = 0.4f;
    public float bloomWait = 0.15f;

    public Slot[,] gridArray;
    private int _cols;
    private int _rows;
    private bool _isProcessing = false;
    private bool _pendingMerge = false;

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int levelId)
    {
        TextAsset json = Resources.Load<TextAsset>($"Levels/Level_{levelId}");
        if (json != null)
        {
            LevelData data = JsonUtility.FromJson<LevelData>(json.text);
            GenerateGrid(data);
        }
    }

    void GenerateGrid(LevelData data)
    {
        _cols = data.columns;
        _rows = data.rows;
        gridArray = new Slot[_cols, _rows];

        HashSet<Vector2Int> disabled = new HashSet<Vector2Int>();
        if (data.disabledSlots != null)
            foreach (var ds in data.disabledSlots)
                disabled.Add(new Vector2Int(ds.x, ds.y));

        float offsetX = (_cols - 1f) * spacing / 2f;
        float offsetZ = (_rows - 1f) * spacing / 2f;

        for (int x = 0; x < _cols; x++)
        {
            for (int z = 0; z < _rows; z++)
            {
                if (disabled.Contains(new Vector2Int(x, z))) continue;

                GameObject tile = Instantiate(tilePrefab, gridParent);
                tile.name = $"Tile_{x}_{z}";

                tile.transform.localPosition = new Vector3((x * spacing) - offsetX, 0, (z * spacing) - offsetZ);

                Slot slot = tile.GetComponent<Slot>();
                slot.Initialize(x, z);
                gridArray[x, z] = slot;
            }
        }
    }

    public List<Slot> GetNeighbors(int x, int z)
    {
        List<Slot> result = new List<Slot>();
        int[,] dirs = { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dirs[i, 0];
            int nz = z + dirs[i, 1];

            if (nx >= 0 && nx < _cols && nz >= 0 && nz < _rows)
            {
                Slot s = gridArray[nx, nz];
                if (s != null && !s.isEmpty && s.currentPlate != null && !s.currentPlate.isExploding)
                    result.Add(s);
            }
        }
        return result;
    }

    public void ProcessMergesAt(Slot slot)
    {
        if (_isProcessing)
        {
            _pendingMerge = true;
            return;
        }
        StartCoroutine(ProcessAllMerges());
    }

    private IEnumerator ProcessAllMerges()
    {
        _isProcessing = true;
        _pendingMerge = false;

        if (GameJuice.Instance != null)
            GameJuice.Instance.ResetCombo();

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameState.CheckingCombo);

        yield return new WaitForSeconds(initialDelay);

        bool merged = true;
        int safety = 0;

        while (merged && safety < 200)
        {
            safety++;
            merged = false;

            SweepNullSlices();
            SweepEmptyPlates();

            for (int x = 0; x < _cols && !merged; x++)
            {
                for (int z = 0; z < _rows && !merged; z++)
                {
                    Slot slotA = gridArray[x, z];
                    if (!IsOccupied(slotA)) continue;

                    PlateItem plateA = slotA.currentPlate;
                    List<Slot> neighbors = GetNeighbors(x, z);

                    foreach (Slot slotB in neighbors)
                    {
                        if (!IsOccupied(slotB)) continue;
                        PlateItem plateB = slotB.currentPlate;

                        HashSet<int> sharedTypes = GetSharedTypes(plateA, plateB);
                        foreach (int type in sharedTypes)
                        {
                            int cA = CountType(plateA, type);
                            int cB = CountType(plateB, type);
                            if (cA <= 0 || cB <= 0) continue;

                            PlateItem sender;
                            PlateItem receiver;

                            if (cA < cB)
                            {
                                sender = plateA;
                                receiver = plateB;
                            }
                            else if (cA > cB)
                            {
                                sender = plateB;
                                receiver = plateA;
                            }
                            else
                            {
                                int distinctA = CountDistinctTypes(plateA);
                                int distinctB = CountDistinctTypes(plateB);
                                if (distinctA >= distinctB)
                                {
                                    sender = plateA;
                                    receiver = plateB;
                                }
                                else
                                {
                                    sender = plateB;
                                    receiver = plateA;
                                }
                            }

                            if (receiver.pizzaSlicesOnPlate.Count >= 6) continue;

                            merged = ExecuteMove(sender, receiver, type);
                            if (merged)
                            {
                                yield return new WaitForSeconds(moveWait);
                                SweepNullSlices();
                                SweepEmptyPlates();
                                SweepBlooms();
                                yield return new WaitForSeconds(bloomWait);
                                SweepEmptyPlates();
                            }
                            break;
                        }
                        if (merged) break;
                    }
                }
            }
        }

        SweepNullSlices();
        SweepEmptyPlates();
        _isProcessing = false;

        if (_pendingMerge)
        {
            _pendingMerge = false;
            StartCoroutine(ProcessAllMerges());
        }
        else
        {
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        for (int x = 0; x < _cols; x++)
        {
            for (int z = 0; z < _rows; z++)
            {
                Slot slot = gridArray[x, z];
                if (slot != null && slot.isEmpty)
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.ChangeState(GameState.Playing);
                    return; 
                }
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.GameOver);
        }
    }

    private bool IsOccupied(Slot slot)
    {
        if (slot == null || slot.isEmpty) return false;
        if (slot.currentPlate == null) return false;
        if (slot.currentPlate.isExploding) return false;
        if (slot.currentPlate.pizzaSlicesOnPlate.Count == 0) return false;
        return true;
    }

    private HashSet<int> GetSharedTypes(PlateItem a, PlateItem b)
    {
        HashSet<int> typesA = new HashSet<int>();
        foreach (PizzaItem s in a.pizzaSlicesOnPlate)
            if (s != null) typesA.Add(s.pizzaType);

        HashSet<int> shared = new HashSet<int>();
        foreach (PizzaItem s in b.pizzaSlicesOnPlate)
            if (s != null && typesA.Contains(s.pizzaType))
                shared.Add(s.pizzaType);

        return shared;
    }

    private int CountType(PlateItem plate, int type)
    {
        int c = 0;
        foreach (PizzaItem s in plate.pizzaSlicesOnPlate)
            if (s != null && s.pizzaType == type) c++;
        return c;
    }

    private int CountDistinctTypes(PlateItem plate)
    {
        HashSet<int> types = new HashSet<int>();
        foreach (PizzaItem s in plate.pizzaSlicesOnPlate)
            if (s != null) types.Add(s.pizzaType);
        return types.Count;
    }

    private bool ExecuteMove(PlateItem sender, PlateItem receiver, int type)
    {
        if (sender == null || receiver == null) return false;
        if (sender == receiver) return false;
        if (receiver.pizzaSlicesOnPlate.Count >= 6) return false;

        List<PizzaItem> candidates = new List<PizzaItem>();
        foreach (PizzaItem s in sender.pizzaSlicesOnPlate)
            if (s != null && s.pizzaType == type)
                candidates.Add(s);

        int space = 6 - receiver.pizzaSlicesOnPlate.Count;
        int count = Mathf.Min(space, candidates.Count);
        if (count <= 0) return false;

        for (int i = 0; i < count; i++)
        {
            PizzaItem slice = candidates[i];
            if (slice == null) continue;

            sender.pizzaSlicesOnPlate.Remove(slice);
            receiver.pizzaSlicesOnPlate.Add(slice);
            slice.myPlate = receiver;
            slice.mySlot = receiver.mySlot;

            int rotIdx = receiver.pizzaSlicesOnPlate.Count - 1;
            slice.transform.SetParent(receiver.transform);
            slice.MoveTo(rotIdx);
        }

        sender.RearrangeSlicesAnimated();
        return true;
    }

    private void SweepNullSlices()
    {
        for (int x = 0; x < _cols; x++)
        {
            for (int z = 0; z < _rows; z++)
            {
                Slot slot = gridArray[x, z];
                if (slot == null || slot.currentPlate == null) continue;
                slot.currentPlate.pizzaSlicesOnPlate.RemoveAll(s => s == null);
            }
        }
    }

    private void SweepEmptyPlates()
    {
        for (int x = 0; x < _cols; x++)
        {
            for (int z = 0; z < _rows; z++)
            {
                Slot slot = gridArray[x, z];
                if (slot == null) continue;

                if (slot.currentPlate != null && slot.currentPlate.pizzaSlicesOnPlate.Count == 0)
                {
                    if (!slot.currentPlate.isExploding)
                        Destroy(slot.currentPlate.gameObject);
                    slot.currentPlate = null;
                    slot.isEmpty = true;
                }

                if (slot.currentPlate == null && !slot.isEmpty)
                    slot.isEmpty = true;
            }
        }
    }

    private void SweepBlooms()
    {
        for (int x = 0; x < _cols; x++)
        {
            for (int z = 0; z < _rows; z++)
            {
                Slot slot = gridArray[x, z];
                if (!IsOccupied(slot)) continue;

                PlateItem plate = slot.currentPlate;
                if (plate.pizzaSlicesOnPlate.Count != 6) continue;

                int firstType = plate.pizzaSlicesOnPlate[0].pizzaType;
                bool allSame = true;
                foreach (PizzaItem s in plate.pizzaSlicesOnPlate)
                {
                    if (s.pizzaType != firstType) { allSame = false; break; }
                }

                if (allSame)
                    plate.ExplodePlate();
            }
        }
    }
}