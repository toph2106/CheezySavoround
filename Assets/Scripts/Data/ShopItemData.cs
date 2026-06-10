using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Cheezy/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    [Tooltip("Mã ID duy nhất để lưu vào file JSON (VD: plate_wood, plate_gold)")]
    public string itemID;
    
    [Tooltip("Tên hiển thị trên UI")]
    public string itemName;
    
    [Tooltip("Giá mua bằng Vàng")]
    public int price;
    
    [Tooltip("Hình ảnh hiển thị trong Shop")]
    public Sprite itemIcon;

    [Header("Dữ liệu Skin (Đĩa)")]
    [Tooltip("Mesh (Hình dáng) của cái đĩa")]
    public Mesh plateMesh;
    
    [Tooltip("Material (Màu sắc/Bề mặt) của cái đĩa")]
    public Material plateMaterial;
}
