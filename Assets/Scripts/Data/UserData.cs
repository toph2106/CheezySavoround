using System;
using System.Collections.Generic;
[Serializable]
public class UserData
{
    public int Gold = 0;

    public int HighestUnlockedLevel = 1;

    public int CurrentLevel = 1;

    public int HighScore = 0;

    public List<string> OwnedSkins = new List<string>();

    public string EquippedSkin = "default";

    public List<QuestData> Quests = new List<QuestData>();

    public int DailyRewardDay = -1;

    public string LastDailyClaimUTC = "";

    public List<bool> DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };

    public bool SoundEnabled = true;

    public bool MusicEnabled = true;

    public List<AchievementData> Achievements = new List<AchievementData>();

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
