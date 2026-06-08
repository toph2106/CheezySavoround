using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đọc config Skin từ JSON (ID + giá).
/// Lấy Mesh/Material từ SkinShopManager (kéo thả trong Inspector).
/// Apply lên tất cả đĩa PlateItem đang sống trong scene.
/// </summary>
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    // Cache tất cả PlateItem đang tồn tại
    private List<PlateItem> _activePlates = new List<PlateItem>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ==================== ĐĂNG KÝ / HỦY ĐĂNG KÝ ĐĨA ====================

    /// <summary>Gọi từ PlateItem.Start() — đăng ký để theo dõi và apply skin ngay.</summary>
    public void RegisterPlate(PlateItem plate)
    {
        if (!_activePlates.Contains(plate))
            _activePlates.Add(plate);

        // Apply skin hiện tại lên đĩa này ngay lập tức
        ApplySkinToPlate(plate.gameObject);
    }

    /// <summary>Gọi từ PlateItem.OnDestroy() — dọn dẹp.</summary>
    public void UnregisterPlate(PlateItem plate)
    {
        _activePlates.Remove(plate);
    }

    // ==================== APPLY SKIN ====================

    /// <summary>
    /// Apply skin đang trang bị lên TẤT CẢ đĩa trong scene.
    /// Gọi sau khi người chơi mua hoặc trang bị skin mới.
    /// </summary>
    public void ApplyEquippedSkinToAll()
    {
        _activePlates.RemoveAll(p => p == null);

        foreach (PlateItem plate in _activePlates)
            ApplySkinToPlate(plate.gameObject);

        Debug.Log($"[SkinManager] Đã apply skin lên {_activePlates.Count} đĩa.");
    }

    /// <summary>Apply skin đang trang bị lên 1 đĩa cụ thể.</summary>
    public void ApplyEquippedSkin(GameObject plateObj)
    {
        ApplySkinToPlate(plateObj);
    }

    // ==================== TIỆN ÍCH (GỌI TỪ BÊN NGOÀI) ====================

    /// <summary>
    /// Trang bị skin theo ID và apply lên tất cả đĩa ngay lập tức.
    /// Gọi từ SkinWardrobeUI hoặc bất kỳ script nào.
    /// </summary>
    public void EquipSkinByID(string skinID)
    {
        if (SaveSystem.Instance == null) return;

        if (!SaveSystem.Instance.HasSkin(skinID))
        {
            Debug.LogWarning($"[SkinManager] Người chơi chưa sở hữu skin: {skinID}");
            return;
        }

        SaveSystem.Instance.EquipSkin(skinID);
        ApplyEquippedSkinToAll();
        SaveSystem.Instance.Save();
        Debug.Log($"[SkinManager] Đã trang bị + apply skin: {skinID}");
    }

    /// <summary>
    /// Trả về danh sách SkinPackage mà người chơi ĐÃ SỞ HỮU.
    /// Dùng để hiển thị UI tủ đồ.
    /// </summary>
    public System.Collections.Generic.List<SkinPackage> GetOwnedSkins()
    {
        var result = new System.Collections.Generic.List<SkinPackage>();
        if (SkinShopManager.Instance == null || SaveSystem.Instance == null) return result;

        foreach (var pkg in SkinShopManager.Instance.packages)
        {
            if (SaveSystem.Instance.HasSkin(pkg.itemID))
                result.Add(pkg);
        }
        return result;
    }

    /// <summary>
    /// Chuyển sang skin tiếp theo trong danh sách đã sở hữu.
    /// Gọi từ 1 nút tạm thời để test nhanh.
    /// </summary>
    public void CycleOwnedSkin()
    {
        var owned = GetOwnedSkins();
        if (owned.Count == 0) return;

        string current = SaveSystem.Instance?.Data.EquippedSkin ?? "";
        int idx = owned.FindIndex(s => s.itemID == current);
        int next = (idx + 1) % owned.Count;

        EquipSkinByID(owned[next].itemID);
        Debug.Log($"[SkinManager] Cycle → {owned[next].itemID}");
    }

    private void ApplySkinToPlate(GameObject plateObj)
    {
        if (plateObj == null || SaveSystem.Instance == null) return;

        if (SkinShopManager.Instance == null)
        {
            Debug.LogWarning("[SkinManager] SkinShopManager.Instance = null!");
            return;
        }

        string equippedID = SaveSystem.Instance.Data.EquippedSkin;
        Debug.Log($"[SkinManager] Đang apply skin '{equippedID}' lên đĩa '{plateObj.name}'");

        SkinPackage skinData = SkinShopManager.Instance.packages.Find(s => s.itemID == equippedID);

        if (skinData == null)
        {
            Debug.Log($"[SkinManager] Skin '{equippedID}' không có trong danh sách shop — giữ nguyên.");
            return;
        }

        MeshFilter mf = plateObj.GetComponent<MeshFilter>();
        MeshRenderer mr = plateObj.GetComponent<MeshRenderer>();

        Debug.Log($"[SkinManager] MeshFilter={mf != null}, MeshRenderer={mr != null}, Mesh={skinData.plateMesh != null}, Material={skinData.plateMaterial != null}");

        if (mf != null && skinData.plateMesh != null)
        {
            mf.sharedMesh = skinData.plateMesh;
            Debug.Log($"[SkinManager] Đổi Mesh → {skinData.plateMesh.name}");
        }

        if (mr != null && skinData.plateMaterial != null)
        {
            mr.material = skinData.plateMaterial;
            Debug.Log($"[SkinManager] Đổi Material → {skinData.plateMaterial.name}");
        }
    }
}
