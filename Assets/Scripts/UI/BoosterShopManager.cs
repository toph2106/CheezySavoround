using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class BoosterPackage
{
    public string boosterID;
    public Texture packageTexture;
    public int goldPrice = 150;
    public int amount = 1;
}

public class BoosterShopManager : MonoBehaviour
{
    public static BoosterShopManager Instance { get; private set; }

    [Header("Danh sách Booster (Kéo ảnh + Gõ giá)")]
    public List<BoosterPackage> packages = new List<BoosterPackage>();

    [Header("Shop Panel")]
    public GameObject shopPanel;

    [Header("Khung hiển thị chính")]
    public RawImage itemDisplayImage;
    public TextMeshProUGUI priceText;

    [Header("Nút Buy")]
    public Button buyButton;

    [Header("Nút chuyển trang")]
    public Button nextButton;
    public Button prevButton;

    [Header("Chấm tròn (Page Dots)")]
    public List<RawImage> pageDots = new List<RawImage>();
    public Texture dotActiveTexture;
    public Texture dotInactiveTexture;

    [Header("Hiển thị Vàng (Tùy chọn)")]
    public TextMeshProUGUI currentGoldText;

    private int _currentIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged += OnDataChanged;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextItem);
        }
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(PreviousItem);
        }

        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.OnDataChanged -= OnDataChanged;
    }


    public void NextItem()
    {
        if (packages.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % packages.Count;
        UpdateDisplay();
    }

    public void PreviousItem()
    {
        if (packages.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + packages.Count) % packages.Count;
        UpdateDisplay();
    }


    private void UpdateDisplay()
    {
        if (packages.Count == 0) return;

        BoosterPackage current = packages[_currentIndex];

        if (itemDisplayImage != null && current.packageTexture != null)
            itemDisplayImage.texture = current.packageTexture;

        if (priceText != null)
            priceText.text = current.goldPrice.ToString();

        if (buyButton != null)
            buyButton.interactable = SaveSystem.Instance != null
                && SaveSystem.Instance.GetGold() >= current.goldPrice;

        UpdatePageDots();

        UpdateGoldText();
    }

    private void UpdatePageDots()
    {
        for (int i = 0; i < pageDots.Count; i++)
        {
            if (pageDots[i] == null) continue;
            pageDots[i].texture = (i == _currentIndex) ? dotActiveTexture : dotInactiveTexture;
        }
    }

    private void UpdateGoldText()
    {
        if (currentGoldText != null && SaveSystem.Instance != null)
            currentGoldText.text = SaveSystem.Instance.GetGold().ToString();
    }


    private void OnBuyButtonClicked()
    {
        if (packages.Count == 0 || SaveSystem.Instance == null) return;

        BoosterPackage current = packages[_currentIndex];

        if (SaveSystem.Instance.SpendGold(current.goldPrice))
        {
            GameplayHUD.AddBooster(current.boosterID, current.amount);
            Debug.Log($"[Shop] Đã mua {current.amount}x {current.boosterID} với giá {current.goldPrice} vàng");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayChaChing();
            }

            var hud = FindFirstObjectByType<GameplayHUD>();
            if (hud != null) hud.RefreshAll();
        }
        else
        {
            Debug.Log($"[Shop] Không đủ vàng để mua {current.boosterID} (cần {current.goldPrice}, có {SaveSystem.Instance.GetGold()})");
        }

        UpdateDisplay();
    }


    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
        _currentIndex = 0;
        UpdateDisplay();
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    private void OnDataChanged() => UpdateDisplay();
}
