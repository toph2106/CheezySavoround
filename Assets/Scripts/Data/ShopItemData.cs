using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Cheezy/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string itemID;
    public string itemName;
    public int price;
    public Sprite itemIcon;

    [Header("Dữ liệu Skin (Đĩa)")]
    public Mesh plateMesh;
    public Material plateMaterial;
}
