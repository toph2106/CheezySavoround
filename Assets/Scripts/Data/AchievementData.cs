using System;

[Serializable]
public class AchievementData
{
    public string AchievementID;
    public int Progress;
    public bool IsUnlocked;
    public bool IsRewardClaimed;

    public AchievementData() { }

    public AchievementData(string id)
    {
        AchievementID = id;
        Progress = 0;
        IsUnlocked = false;
        IsRewardClaimed = false;
    }
}
