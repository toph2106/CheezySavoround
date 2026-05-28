using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class SkinPackage
{
    [Tooltip("Mã ID duy nhất (VD: skin_wood, skin_gold)")]
    public string itemID;

    [Tooltip("Ảnh hiển thị Skin (kéo ảnh vào đây)")]
    public Texture packageTexture;

    [Tooltip("Giá mua bằng VÀNG (0 = Miễn phí)")]
    public int goldPrice = 200;

    [Header("Dữ liệu 3D (Tùy chọn)")]
    [Tooltip("Mesh (Hình dáng) đĩa — để trống nếu không đổi")]
    public Mesh plateMesh;

    [Tooltip("Material (Màu sắc) đĩa — để trống nếu không đổi")]
    public Material plateMaterial;
}

/// <summary>
/// Shop Skin — Mua bằng Vàng, đổi giao diện đĩa.
/// Clone từ ShopManager (Coin) nhưng xử lý logic Sở hữu / Trang bị.
/// </summary>
public class SkinShopManager : MonoBehaviour
{
    public static SkinShopManager Instance { get; private set; }

    [Header("Danh sách Skin (Kéo ảnh + Gõ giá)")]
    public List<SkinPackage> packages = new List<SkinPackage>();

    [Header("Shop Panel")]
    public GameObject shopPanel;

    [Header("Khung hiển thị chính")]
    public RawImage itemDisplayImage;
    public TextMeshProUGUI priceText;

    [Header("Nút Buy (Ảnh BUY có sẵn, chỉ cần kéo Button vào)")]
    public Button buyButton;

    [Tooltip("Ảnh hiện khi chưa mua (trạng thái BUY)")]
    public Texture btnTextureBuy;

    [Tooltip("Ảnh hiện khi đã mua nhưng chưa trang bị (trạng thái USE)")]
    public Texture btnTextureUse;

    [Tooltip("Ảnh hiện khi đang trang bị (trạng thái USED)")]
    public Texture btnTextureUsed;

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

    // ==================== CHUYỂN TRANG ====================

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

    // ==================== CẬP NHẬT HIỂN THỊ ====================

    private void UpdateDisplay()
    {
        if (packages.Count == 0) return;

        SkinPackage current = packages[_currentIndex];

        // 1. Đổi ảnh
        if (itemDisplayImage != null && current.packageTexture != null)
            itemDisplayImage.texture = current.packageTexture;

        // 2. Xác định trạng thái
        bool isOwned = SaveSystem.Instance != null && SaveSystem.Instance.HasSkin(current.itemID);
        bool isEquipped = SaveSystem.Instance != null && SaveSystem.Instance.Data.EquippedSkin == current.itemID;

        // Skin miễn phí (giá = 0) tự động unlock
        if (current.goldPrice == 0 && !isOwned && SaveSystem.Instance != null)
        {
            SaveSystem.Instance.UnlockSkin(current.itemID);
            isOwned = true;
        }

        // 3. Cập nhật chữ giá
        if (priceText != null)
        {
            priceText.gameObject.SetActive(!isOwned);
            priceText.text = current.goldPrice.ToString();
        }

        // 4. Cập nhật nút BUY — đổi Ảnh theo trạng thái
        RawImage btnRaw = buyButton != null ? buyButton.GetComponentInChildren<RawImage>() : null;

        if (isEquipped)
        {
            if (buyButton != null) buyButton.interactable = false;
            if (btnRaw != null && btnTextureUsed != null) btnRaw.texture = btnTextureUsed;
        }
        else if (isOwned)
        {
            if (buyButton != null) buyButton.interactable = true;
            if (btnRaw != null && btnTextureUse != null) btnRaw.texture = btnTextureUse;
        }
        else
        {
            bool canAfford = SaveSystem.Instance != null && SaveSystem.Instance.GetGold() >= current.goldPrice;
            if (buyButton != null) buyButton.interactable = canAfford;
            if (btnRaw != null && btnTextureBuy != null) btnRaw.texture = btnTextureBuy;
        }

        // 5. Chấm tròn
        UpdatePageDots();

        // 6. Vàng
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

    // ==================== NÚT BUY ====================

    private void OnBuyButtonClicked()
    {
        if (packages.Count == 0 || SaveSystem.Instance == null) return;

        SkinPackage current = packages[_currentIndex];
        bool isOwned = SaveSystem.Instance.HasSkin(current.itemID);

        if (isOwned)
        {
            // Đã có -> Trang bị
            SaveSystem.Instance.EquipSkin(current.itemID);
            Debug.Log($"[SkinShop] Đã trang bị: {current.itemID}");
        }
        else
        {
            // Chưa có -> Mua
            if (SaveSystem.Instance.SpendGold(current.goldPrice))
            {
                SaveSystem.Instance.UnlockSkin(current.itemID);
                SaveSystem.Instance.EquipSkin(current.itemID);
                Debug.Log($"[SkinShop] Đã MUA Skin: {current.itemID} (Giá: {current.goldPrice})");
            }
            else
            {
                Debug.LogWarning("[SkinShop] Không đủ vàng!");
                UpdateDisplay();
                return;
            }
        }

        // Apply Mesh 3D lên tất cả đĩa đang có trong scene ngay lập tức
        if (SkinManager.Instance != null)
            SkinManager.Instance.ApplyEquippedSkinToAll();

        // Lưu game
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.Save();

        UpdateDisplay();
    }

    // ==================== MỞ / ĐÓNG ====================

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
