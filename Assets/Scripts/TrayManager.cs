using UnityEngine;

public class TrayManager : MonoBehaviour
{
    public static TrayManager Instance { get; private set; }

    [Header("Spawning Setup")]
    public Transform[] spawnPoints;
    public GameObject[] platePrefabs;

    private int _activePlatesInTray = 0;

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
    }

    public void SpawnNewPlates()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || platePrefabs == null || platePrefabs.Length == 0) return;

        _activePlatesInTray = spawnPoints.Length;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int rndIndex = Random.Range(0, platePrefabs.Length);
            GameObject obj = Instantiate(platePrefabs[rndIndex], spawnPoints[i].position, Quaternion.identity);
            
            if (SkinManager.Instance != null)
            {
                SkinManager.Instance.ApplyEquippedSkin(obj);
            }

            obj.transform.SetParent(spawnPoints[i]); 
        }
    }

    public void OnPlatePlaced()
    {
        _activePlatesInTray--;
        if (_activePlatesInTray <= 0)
        {
            Invoke(nameof(SpawnNewPlates), 0.5f);
        }
    }
    public void ClearTray()
    {
        CancelInvoke(nameof(SpawnNewPlates));
        _activePlatesInTray = 0;

        if (spawnPoints == null) return;
        foreach (var sp in spawnPoints)
        {
            if (sp == null) continue;
            for (int i = sp.childCount - 1; i >= 0; i--)
            {
                Destroy(sp.GetChild(i).gameObject);
            }
        }
    }
}
