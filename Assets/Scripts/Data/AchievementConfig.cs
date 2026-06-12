using UnityEngine;
public enum AchievementType
{
    PlatesExploded,   // Tổng đĩa đã nổ
    ComboReached,     // Đạt combo >= X trong 1 lượt
    GoldEarned,       // Tổng vàng kiếm được
    SkinsUnlocked,    // Tổng skin đã mua
    GamesPlayed,      // Tổng số game đã chơi
    PlatesPlaced,     // Tổng đĩa đã đặt lên bàn
    LevelsCleared     // Tổng số ải đã vượt qua
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Cheezy/Achievement Config")]
public class AchievementConfig : ScriptableObject
{
    [Header("Định danh")]
    public string achievementID;

    [Header("Hiển thị")]
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Điều kiện")]
    public AchievementType type;

    public int targetValue = 1;

    [Header("Phần thưởng")]
    public int goldReward = 50;
}
