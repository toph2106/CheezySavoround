using UnityEngine;
using UnityEditor;
using System.IO;

public class AchievementGenerator : EditorWindow
{
    [MenuItem("Cheezy/Create Achievements")]
    public static void GenerateAchievements()
    {
        string folderPath = "Assets/ScriptableObjects/Achievements";
        
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Achievements");
        }

        CreateAch("ach_lvl_10", "Tân Binh Giao Bánh", "Vượt qua 10 ải đầu tiên.", AchievementType.LevelsCleared, 10, 100, folderPath);
        CreateAch("ach_lvl_20", "Đầu Bếp Trưởng", "Vượt qua 20 ải.", AchievementType.LevelsCleared, 20, 250, folderPath);
        CreateAch("ach_lvl_30", "Siêu Cấp Bếp Trưởng", "Vượt qua 30 ải khó nhằn.", AchievementType.LevelsCleared, 30, 500, folderPath);

        CreateAch("ach_gold_1000", "Lợn Đất", "Kiếm được tổng cộng 1,000 Vàng.", AchievementType.GoldEarned, 1000, 50, folderPath);
        CreateAch("ach_gold_5000", "Trọc Phú", "Kiếm được tổng cộng 5,000 Vàng.", AchievementType.GoldEarned, 5000, 200, folderPath);
        CreateAch("ach_gold_20000", "Tỷ Phú Pizza", "Kiếm được tổng cộng 20,000 Vàng.", AchievementType.GoldEarned, 20000, 500, folderPath);

        CreateAch("ach_boom_50", "Máy Cắt Pizza", "Ghép nổ 50 đĩa Pizza.", AchievementType.PlatesExploded, 50, 50, folderPath);
        CreateAch("ach_boom_250", "Kẻ Hủy Diệt", "Ghép nổ 250 đĩa Pizza.", AchievementType.PlatesExploded, 250, 150, folderPath);
        CreateAch("ach_boom_1000", "Vua Bom Nổ", "Ghép nổ 1000 đĩa Pizza.", AchievementType.PlatesExploded, 1000, 400, folderPath);

        CreateAch("ach_place_100", "Đôi Tay Bận Rộn", "Đặt 100 đĩa lên bàn.", AchievementType.PlatesPlaced, 100, 50, folderPath);
        CreateAch("ach_place_500", "Bồi Bàn Siêu Tốc", "Đặt 500 đĩa lên bàn.", AchievementType.PlatesPlaced, 500, 150, folderPath);
        CreateAch("ach_place_1000", "Máy Rót Đĩa", "Đặt 1000 đĩa lên bàn.", AchievementType.PlatesPlaced, 1000, 300, folderPath);

        CreateAch("ach_combo_3", "Dễ Như Ăn Kẹo", "Đạt Combo x3 trong 1 lượt.", AchievementType.ComboReached, 3, 100, folderPath);
        CreateAch("ach_combo_5", "Bàn Tay Ma Thuật", "Đạt Combo x5 trong 1 lượt.", AchievementType.ComboReached, 5, 200, folderPath);
        CreateAch("ach_combo_10", "Thần Đồng Ghép Đĩa", "Đạt Combo x10 trong 1 lượt.", AchievementType.ComboReached, 10, 500, folderPath);

        CreateAch("ach_play_5", "Khởi Động", "Chơi 5 ván game.", AchievementType.GamesPlayed, 5, 50, folderPath);
        CreateAch("ach_play_20", "Khách Quen", "Chơi 20 ván game.", AchievementType.GamesPlayed, 20, 100, folderPath);
        CreateAch("ach_play_100", "Nghiện Pizza", "Chơi 100 ván game.", AchievementType.GamesPlayed, 100, 300, folderPath);

        CreateAch("ach_skin_2", "Đổi Gió", "Mở khóa 2 Skin mới.", AchievementType.SkinsUnlocked, 2, 100, folderPath);
        CreateAch("ach_skin_5", "Dân Chơi Hàng Hiệu", "Mở khóa 5 Skin.", AchievementType.SkinsUnlocked, 5, 300, folderPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateAch(string id, string name, string desc, AchievementType type, int target, int reward, string folderPath)
    {
        string assetPath = $"{folderPath}/{id}.asset";
        
        if (AssetDatabase.LoadAssetAtPath<AchievementConfig>(assetPath) != null)
        {
            return;
        }

        AchievementConfig config = ScriptableObject.CreateInstance<AchievementConfig>();
        config.achievementID = id;
        config.displayName = name;
        config.description = desc;
        config.type = type;
        config.targetValue = target;
        config.goldReward = reward;

        AssetDatabase.CreateAsset(config, assetPath);
    }
}
