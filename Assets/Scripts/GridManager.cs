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
    public float spacing = 1.1f;

    public Slot[,] gridArray;

    void Start()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int levelId)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/Level_{levelId}");

        if (jsonFile != null)
        {
            LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
            GenerateGrid(data);
        }
    }

    void GenerateGrid(LevelData data)
    {
        gridArray = new Slot[data.columns, data.rows];
        HashSet<Vector2Int> disabledSet = new HashSet<Vector2Int>();

        if (data.disabledSlots != null)
        {
            foreach (var ds in data.disabledSlots)
            {
                disabledSet.Add(new Vector2Int(ds.x, ds.y));
            }
        }

        for (int x = 0; x < data.columns; x++)
        {
            for (int z = 0; z < data.rows; z++)
            {
                if (disabledSet.Contains(new Vector2Int(x, z))) continue;

                Vector3 spawnPos = new Vector3(x * spacing, 0, z * spacing);
                GameObject tileObj = Instantiate(tilePrefab, spawnPos, Quaternion.identity, gridParent);
                tileObj.name = $"Tile_{x}_{z}";

                Slot slotComponent = tileObj.GetComponent<Slot>();
                slotComponent.Initialize(x, z);
                gridArray[x, z] = slotComponent;
            }
        }
    }
}