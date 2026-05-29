using UnityEngine;

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

    public void OpenShop()
    {
        gameObject.SetActive(true);
        ShowCoin();                
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }

    public void ShowCoin()
    {
        SetPanels(coinPanel, skinPanel, boosterPanel);

        if (ShopManager.Instance != null)
            ShopManager.Instance.OpenShop();
    }


    public void ShowSkin()
    {
        SetPanels(skinPanel, coinPanel, boosterPanel);

        if (SkinShopManager.Instance != null)
            SkinShopManager.Instance.OpenShop();
    }

    public void ShowBooster()
    {
        SetPanels(boosterPanel, coinPanel, skinPanel);

        if (BoosterShopManager.Instance != null)
            BoosterShopManager.Instance.OpenShop();
    }

    private void SetPanels(GameObject active, GameObject hideA, GameObject hideB)
    {
        if (active != null) active.SetActive(true);
        if (hideA != null) hideA.SetActive(false);
        if (hideB != null) hideB.SetActive(false);
    }
}
