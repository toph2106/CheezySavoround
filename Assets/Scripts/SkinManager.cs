using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý việc áp dụng Skin (Mesh + Material) lên các đĩa khi chúng được sinh ra.
/// </summary>
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Tooltip("Phải có chung reference tới list allItems của ShopManager để lấy Data")]
    public List<ShopItemData> skinDatabase = new List<ShopItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Hàm dùng để áp dụng Skin hiện tại cho 1 cái đĩa (PlateItem).
    /// </summary>
    public void ApplyEquippedSkin(GameObject plateObj)
    {
        if (SaveSystem.Instance == null || plateObj == null) return;

        string equippedID = SaveSystem.Instance.Data.EquippedSkin;
        
        // Tìm dữ liệu skin trong Database
        ShopItemData skinData = skinDatabase.Find(x => x.itemID == equippedID);

        if (skinData != null)
        {
            MeshFilter mf = plateObj.GetComponent<MeshFilter>();
            MeshRenderer mr = plateObj.GetComponent<MeshRenderer>();

            if (mf != null && skinData.plateMesh != null)
            {
                mf.sharedMesh = skinData.plateMesh;
            }

            if (mr != null && skinData.plateMaterial != null)
            {
                mr.material = skinData.plateMaterial;
            }
        }
    }
}
