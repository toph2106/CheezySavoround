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
        SpawnNewPlates();
    }

    public void SpawnNewPlates()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || platePrefabs == null || platePrefabs.Length == 0) return;

        _activePlatesInTray = spawnPoints.Length;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int rndIndex = Random.Range(0, platePrefabs.Length);
            GameObject obj = Instantiate(platePrefabs[rndIndex], spawnPoints[i].position, Quaternion.identity);
            
            // Gắn đĩa làm con của điểm sinh (để quản lý gọn gàng trong Hierarchy)
            obj.transform.SetParent(spawnPoints[i]); 
        }
    }

    public void OnPlatePlaced()
    {
        _activePlatesInTray--;
        if (_activePlatesInTray <= 0)
        {
            // Nghỉ 0.5s để đĩa vừa đặt xuống có thời gian bay/merge trước khi đẻ đĩa mới
            Invoke(nameof(SpawnNewPlates), 0.5f);
        }
    }
}
