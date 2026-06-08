using System;
using System.Collections.Generic;
[Serializable]
public class UserData
{
    // ==================== TIỀN TỆ ====================
    public int Gold = 0;

    // ==================== TIẾN ĐỘ GAME ====================
    public int HighestUnlockedLevel = 1;

    public int CurrentLevel = 1;

    public int HighScore = 0;

    // ==================== SKIN ====================
    public List<string> OwnedSkins = new List<string>();

    public string EquippedSkin = "default";

    // ==================== NHIỆM VỤ ====================
    public List<QuestData> Quests = new List<QuestData>();

    // ==================== DAILY REWARD ====================
    public int DailyRewardDay = -1;

    public string LastDailyClaimUTC = "";

    public List<bool> DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };

    // ==================== CÀI ĐẶT ====================
    public bool SoundEnabled = true;

    public bool MusicEnabled = true;

    // ==================== ACHIEVEMENT ====================
    public List<AchievementData> Achievements = new List<AchievementData>();

    // Thống kê tích lũy (all-time) — dùng cho achievement tracking
    public int TotalGoldEarned = 0;
    public int TotalPlatesExploded = 0;
    public int TotalPlatesPlaced = 0;
    public int TotalGamesPlayed = 0;
    public int HighestCombo = 0;
}
[Serializable]
public class QuestData
{
    public string QuestID;

    public int Progress;

    public int Target;

    public bool IsCompleted;

    public bool IsRewardClaimed;
}
