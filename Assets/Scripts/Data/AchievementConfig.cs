using UnityEngine;
public enum AchievementType
{
    PlatesExploded,   // Tổng đĩa đã nổ
    ComboReached,     // Đạt combo >= X trong 1 lượt
    GoldEarned,       // Tổng vàng kiếm được
    SkinsUnlocked,    // Tổng skin đã mua
    GamesPlayed,      // Tổng số game đã chơi
    PlatesPlaced      // Tổng đĩa đã đặt lên bàn
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Cheezy/Achievement Config")]
public class AchievementConfig : ScriptableObject
{
    [Header("Định danh")]
    [Tooltip("ID duy nhất, dùng để lưu progress (VD: first_explode)")]
    public string achievementID;

    [Header("Hiển thị")]
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Điều kiện")]
    [Tooltip("Loại sự kiện achievement lắng nghe")]
    public AchievementType type;

    [Tooltip("Giá trị cần đạt để unlock (VD: 10 = nổ 10 đĩa)")]
    public int targetValue = 1;

    [Header("Phần thưởng")]
    [Tooltip("Số vàng nhận khi claim")]
    public int goldReward = 50;
}
