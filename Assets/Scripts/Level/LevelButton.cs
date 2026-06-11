using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public Image backgroundImage;
    public Button myButton;

    [Header("Colors & Visuals")]
    public Color passedColor = new Color(1f, 1f, 1f, 1f);
    public Color currentColor = new Color(1f, 0.8f, 0f, 1f);
    public Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    public GameObject lockIcon;

    private int _levelNumber;

    public void Setup(int level, int highestLevel)
    {
        _levelNumber = level;

        if (levelText != null)
            levelText.text = level.ToString();

        if (myButton != null)
            myButton.onClick.RemoveAllListeners();

        if (level < highestLevel)
        {
            backgroundImage.color = passedColor;
            myButton.interactable = true;
            if (lockIcon != null) lockIcon.SetActive(false);
            myButton.onClick.AddListener(OnLevelClicked);
        }
        else if (level == highestLevel)
        {
            backgroundImage.color = currentColor;
            myButton.interactable = true;
            if (lockIcon != null) lockIcon.SetActive(false);
            myButton.onClick.AddListener(OnLevelClicked);
        }
        else
        {
            backgroundImage.color = lockedColor;
            myButton.interactable = false; 
            if (lockIcon != null) lockIcon.SetActive(true);
        }
    }

    private void OnLevelClicked()
    {
        if (GameManager.Instance != null)
        {
            LevelSelectManager.Instance.CloseMenu();
            GameManager.Instance.ReplaySpecificLevel(_levelNumber);
        }
    }
}