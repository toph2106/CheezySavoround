using UnityEngine;

/// <summary>
/// Loại sự kiện mà achievement lắng nghe.
/// Thêm enum mới ở đây khi cần loại achievement mới.
/// </summary>
public enum AchievementType
{
    PlatesExploded,   // Tổng đĩa đã nổ
    ComboReached,     // Đạt combo >= X trong 1 lượt
    GoldEarned,       // Tổng vàng kiếm được (all-time)
    SkinsUnlocked,    // Tổng skin đã mua
    GamesPlayed,      // Tổng số game đã chơi
    PlatesPlaced      // Tổng đĩa đã đặt lên bàn
}

/// <summary>
/// ScriptableObject define 1 achievement.
/// Tạo asset: Right-click > Create > Cheezy > Achievement Config.
/// Dễ thêm / sửa / xóa achievement mà KHÔNG cần sửa code.
/// </summary>
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
