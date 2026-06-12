using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;

public class AtlasGenerator : EditorWindow
{
    [MenuItem("Cheezy/Tối ưu/Tự động đóng gói Atlas (Giảm Batches)")]
    public static void GenerateAtlas()
    {
        string atlasPath = "Assets/GameSpriteAtlas.spriteatlas";
        
        if (AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath) != null)
        {
            return;
        }

        SpriteAtlas atlas = new SpriteAtlas();
        
        SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = false,
            padding = 2
        };
        atlas.SetPackingSettings(packingSettings);

        SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings()
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(textureSettings);

        AssetDatabase.CreateAsset(atlas, atlasPath);

        string[] searchFolders = new string[] { 
            "Assets/Textures", 
            "Assets/UI", 
            "Assets/Resources"
        };

        foreach (string folder in searchFolders)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                Object folderObj = AssetDatabase.LoadAssetAtPath<Object>(folder);
                if (folderObj != null)
                {
                    SpriteAtlasExtensions.Add(atlas, new Object[] { folderObj });
                }
            }
        }
        AssetDatabase.SaveAssets();
        SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
    }
}
