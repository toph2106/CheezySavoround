#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script tạo 10 Achievement Config assets mặc định.
/// Chạy 1 lần: Menu bar → Cheezy → Create Default Achievements
/// </summary>
public class AchievementConfigCreator
{
    [MenuItem("Cheezy/Create Default Achievements")]
    public static void CreateDefaultAchievements()
    {
        string folder = "Assets/ScriptableObjects/Achievements";

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Achievements");

        CreateAchievement(folder, "first_explode", "Pizza Master", "Nổ đĩa pizza đầu tiên",
            AchievementType.PlatesExploded, 1, 50);

        CreateAchievement(folder, "explode_10", "Boom Boom", "Nổ tổng cộng 10 đĩa pizza",
            AchievementType.PlatesExploded, 10, 100);

        CreateAchievement(folder, "explode_50", "Explosion Expert", "Nổ tổng cộng 50 đĩa pizza",
            AchievementType.PlatesExploded, 50, 200);

        CreateAchievement(folder, "combo_3", "Combo Starter", "Đạt combo 3 trong 1 lượt",
            AchievementType.ComboReached, 3, 75);

        CreateAchievement(folder, "combo_5", "Combo King", "Đạt combo 5 trong 1 lượt",
            AchievementType.ComboReached, 5, 150);

        CreateAchievement(folder, "earn_500", "Gold Digger", "Kiếm tổng cộng 500 vàng",
            AchievementType.GoldEarned, 500, 100);

        CreateAchievement(folder, "earn_2000", "Rich Chef", "Kiếm tổng cộng 2000 vàng",
            AchievementType.GoldEarned, 2000, 200);

        CreateAchievement(folder, "buy_skin", "Fashionista", "Mua skin đầu tiên",
            AchievementType.SkinsUnlocked, 1, 50);

        CreateAchievement(folder, "play_10", "Addicted", "Chơi 10 lượt game",
            AchievementType.GamesPlayed, 10, 100);

        CreateAchievement(folder, "place_100", "Plate Placer", "Đặt tổng cộng 100 đĩa lên bàn",
            AchievementType.PlatesPlaced, 100, 150);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AchievementConfigCreator] ✅ Đã tạo 10 Achievement configs tại: " + folder);
        EditorUtility.DisplayDialog("Thành công!", $"Đã tạo 10 Achievement configs tại:\n{folder}", "OK");
    }

    private static void CreateAchievement(string folder, string id, string name, string desc,
        AchievementType type, int target, int reward)
    {
        string path = $"{folder}/{id}.asset";

        // Nếu đã tồn tại → skip
        if (AssetDatabase.LoadAssetAtPath<AchievementConfig>(path) != null)
        {
            Debug.Log($"[AchievementConfigCreator] Đã tồn tại: {path} → Bỏ qua");
            return;
        }

        AchievementConfig config = ScriptableObject.CreateInstance<AchievementConfig>();
        config.achievementID = id;
        config.displayName = name;
        config.description = desc;
        config.type = type;
        config.targetValue = target;
        config.goldReward = reward;

        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"[AchievementConfigCreator] Tạo: {path}");
    }
}
#endif
