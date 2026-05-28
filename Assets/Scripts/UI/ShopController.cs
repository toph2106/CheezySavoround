using UnityEngine;

/// <summary>
/// Điều phối trung tâm: Bật shop nào thì tắt 2 shop còn lại.
/// Gắn script này vào ShopPanel (cái bảng gỗ lớn bên ngoài).
/// OpenShop/CloseShop bật/tắt chính cái ShopPanel này.
/// </summary>
public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    [Header("3 Sub-Panel (kéo CoinS, SkinS, BoostersS vào đây)")]
    [Tooltip("Panel của Shop Coin (CoinS)")]
    public GameObject coinPanel;

    [Tooltip("Panel của Shop Skin (SkinS)")]
    public GameObject skinPanel;

    [Tooltip("Panel của Shop Boosters (BoostersS)")]
    public GameObject boosterPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ==================== MỞ / ĐÓNG CẢ SHOP ====================

    /// <summary>
    /// Gắn vào nút ShopButton ở MenuPanel.
    /// Bật bảng Shop lên và mặc định hiện Tab Coin.
    /// </summary>
    public void OpenShop()
    {
        gameObject.SetActive(true); // Bật chính ShopPanel này lên
        ShowCoin();                 // Mặc định vào Tab Coin
    }

    /// <summary>
    /// Gắn vào nút X đóng Shop.
    /// </summary>
    public void CloseShop()
    {
        gameObject.SetActive(false); // Tắt ShopPanel đi
    }

    // ==================== CHUYỂN TAB ====================

    /// <summary>
    /// Gắn vào nút Tab Coin.
    /// Bật CoinS, tắt SkinS + BoostersS.
    /// </summary>
    public void ShowCoin()
    {
        SetPanels(coinPanel, skinPanel, boosterPanel);

        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenShop();
    }

    /// <summary>
    /// Gắn vào nút Tab Skin.
    /// Bật SkinS, tắt CoinS + BoostersS.
    /// </summary>
    public void ShowSkin()
    {
        SetPanels(skinPanel, coinPanel, boosterPanel);

        if (SkinShopManager.Instance != null)
            SkinShopManager.Instance.OpenShop();
    }

    /// <summary>
    /// Gắn vào nút Tab Boosters.
    /// Bật BoostersS, tắt CoinS + SkinS.
    /// </summary>
    public void ShowBooster()
    {
        SetPanels(boosterPanel, coinPanel, skinPanel);

        if (BoosterShopManager.Instance != null)
            BoosterShopManager.Instance.OpenShop();
    }

    // ==================== HELPER ====================

    private void SetPanels(GameObject active, GameObject hideA, GameObject hideB)
    {
        if (active != null) active.SetActive(true);
        if (hideA != null) hideA.SetActive(false);
        if (hideB != null) hideB.SetActive(false);
    }
}
