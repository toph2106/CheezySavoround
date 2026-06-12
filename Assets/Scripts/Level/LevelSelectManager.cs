using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    [Header("References")]
    public GameObject levelSelectPanel;
    public GameObject levelButtonPrefab;
    public Transform contentParent;

    [Header("Settings")]
    public int totalLevels = 30;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        CloseLevel();
    }

    public void OpenMenu()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);

        GenerateLevelButtons();
    }

    public void CloseMenu()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    private void GenerateLevelButtons()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int highestLevel = 1;
        if (SaveSystem.Instance != null && SaveSystem.Instance.Data != null)
        {
            highestLevel = SaveSystem.Instance.Data.CurrentLevel;
        }

        for (int i = 1; i <= totalLevels; i++)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, contentParent);
            LevelButton btnScript = btnObj.GetComponent<LevelButton>();

            if (btnScript != null)
            {
                btnScript.Setup(i, highestLevel);
            }
        }
    }
    public void CloseLevel()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

}