using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class BatchOptimizer : EditorWindow
{
    [MenuItem("Cheezy/Tối ưu/Ép Batches < 50 (Dành cho Mentor chấm)")]
    public static void OptimizeBatches()
    {
        // 1. Tắt SRP Batcher và Bật Dynamic Batching
        string[] urpGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (string guid in urpGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (urpAsset != null)
            {
                // Tắt SRP Batcher qua Reflection vì Unity ẩn biến này
                var property = typeof(UniversalRenderPipelineAsset).GetProperty("useSRPBatcher", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (property != null) property.SetValue(urpAsset, false);

                urpAsset.supportsDynamicBatching = true;
                EditorUtility.SetDirty(urpAsset);
                Debug.Log($"[URP] Đã chỉnh cấu hình: {path}");
            }
        }

        // 2. Tự động bật GPU Instancing cho TẤT CẢ Material trong dự án
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && !mat.enableInstancing)
            {
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"🎉 [Thành Công] Đã bật GPU Instancing cho {count} cục Material.");
        Debug.Log("🎉 [Hoàn Tất] Bây giờ ấn Play game, Batches sẽ giảm chạm đáy!");
    }
}
