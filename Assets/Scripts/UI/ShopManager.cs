using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class CoinPackage
{
    [Tooltip("Ảnh hiển thị gói coin (kéo ảnh vào đây)")]
    public Texture packageTexture;

    [Tooltip("Giá hiển thị (VD: 1.99$)")]
    public string displayPrice = "1.99$";

    [Tooltip("Số vàng nhận được khi mua gói này")]
    public int goldAmount = 100;
}
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    // ==================== DANH SÁCH GÓI COIN ====================
    [Header("Danh sách gói Coin (Kéo ảnh + Gõ giá)")]
    public List<CoinPackage> packages = new List<CoinPackage>();

    // ==================== SHOP PANEL ====================
    [Header("Shop Panel")]
    public GameObject shopPanel;

    // ==================== KHUNG HIỂN THỊ ====================
    [Header("Khung hiển thị chính")]
    [Tooltip("RawImage ở giữa (đang hiện hình đồng xu)")]
    public RawImage itemDisplayImage;

    [Tooltip("Chữ giá tiền (Text TMP — cái 1.99$)")]
    public TextMeshProUGUI priceText;

    // ==================== NÚT BUY (Chỉ cần Button, không cần Text) ====================
    [Header("Nút Buy (Ảnh BUY có sẵn, chỉ cần kéo Button vào)")]
    public Button buyButton;

    // ==================== NÚT CHUYỂN TRANG ====================
    [Header("Nút chuyển trang (Kéo NextR và NextL vào đây)")]
    [Tooltip("Nút mũi tên Phải")]
    public Button nextButton;

    [Tooltip("Nút mũi tên Trái")]
    public Button prevButton;

    // ==================== CHẤM TRÒN (PAGE DOTS) ====================
    [Header("Chấm tròn (Page Dots)")]
    [Tooltip("Kéo các RawImage chấm tròn vào đây, từ trái sang phải")]
    public List<RawImage> pageDots = new List<RawImage>();

    [Tooltip("Ảnh chấm khi ĐANG CHỌN (tím)")]
    public Texture dotActiveTexture;

    [Tooltip("Ảnh chấm khi KHÔNG CHỌN (gỗ/xám)")]
    public Texture dotInactiveTexture;

    // ==================== HIỂN THỊ VÀNG ====================
    [Header("Hiển thị Vàng (Tùy chọn)")]
    public TextMeshProUGUI currentGoldText;

    // ==================== BIẾN NỘI BỘ ====================
    private int _currentIndex = 0;

    // ==================== KHỞI TẠO ====================
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
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnDataChanged += OnDataChanged;
        }

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
        {
            SaveSystem.Instance.OnDataChanged -= OnDataChanged;
        }
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

        CoinPackage current = packages[_currentIndex];

        if (itemDisplayImage != null && current.packageTexture != null)
        {
            itemDisplayImage.texture = current.packageTexture;
        }

        if (priceText != null)
        {
            priceText.text = current.displayPrice;
        }

        UpdatePageDots();

        UpdateGoldText();
    }

    private void UpdatePageDots()
    {
        for (int i = 0; i < pageDots.Count; i++)
        {
            if (pageDots[i] == null) continue;

            if (i == _currentIndex)
            {
                if (dotActiveTexture != null) pageDots[i].texture = dotActiveTexture;
            }
            else
            {
                if (dotInactiveTexture != null) pageDots[i].texture = dotInactiveTexture;
            }
        }
    }

    private void UpdateGoldText()
    {
        if (currentGoldText != null && SaveSystem.Instance != null)
        {
            currentGoldText.text = SaveSystem.Instance.GetGold().ToString();
        }
    }


    private void OnBuyButtonClicked()
    {
        if (packages.Count == 0 || SaveSystem.Instance == null) return;

        CoinPackage current = packages[_currentIndex];

        SaveSystem.Instance.AddGold(current.goldAmount);
        Debug.Log($"[Shop] Đã mua gói {current.displayPrice} → +{current.goldAmount} vàng!");

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

    private void OnDataChanged()
    {
        UpdateDisplay();
    }
}
