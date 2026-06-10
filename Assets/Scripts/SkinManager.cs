using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    private List<PlateItem> _activePlates = new List<PlateItem>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public void RegisterPlate(PlateItem plate)
    {
        if (!_activePlates.Contains(plate))
            _activePlates.Add(plate);

        ApplySkinToPlate(plate.gameObject);
    }

    public void UnregisterPlate(PlateItem plate)
    {
        _activePlates.Remove(plate);
    }


    public void ApplyEquippedSkinToAll()
    {
        _activePlates.RemoveAll(p => p == null);

        foreach (PlateItem plate in _activePlates)
            ApplySkinToPlate(plate.gameObject);

    }

    public void ApplyEquippedSkin(GameObject plateObj)
    {
        ApplySkinToPlate(plateObj);
    }

    public void EquipSkinByID(string skinID)
    {
        if (SaveSystem.Instance == null) return;

        if (!SaveSystem.Instance.HasSkin(skinID))
        {
            return;
        }

        SaveSystem.Instance.EquipSkin(skinID);
        ApplyEquippedSkinToAll();
        SaveSystem.Instance.Save();
    }

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

    public void CycleOwnedSkin()
    {
        var owned = GetOwnedSkins();
        if (owned.Count == 0) return;

        string current = SaveSystem.Instance?.Data.EquippedSkin ?? "";
        int idx = owned.FindIndex(s => s.itemID == current);
        int next = (idx + 1) % owned.Count;

        EquipSkinByID(owned[next].itemID);
    }

    private void ApplySkinToPlate(GameObject plateObj)
    {
        if (plateObj == null || SaveSystem.Instance == null) return;

        if (SkinShopManager.Instance == null)
        {
            return;
        }

        string equippedID = SaveSystem.Instance.Data.EquippedSkin;

        SkinPackage skinData = SkinShopManager.Instance.packages.Find(s => s.itemID == equippedID);

        if (skinData == null)
        {
            return;
        }

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
