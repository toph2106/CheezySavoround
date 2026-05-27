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

    // ==================== CÀI ĐẶT ====================
    public bool SoundEnabled = true;

    public bool MusicEnabled = true;
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
