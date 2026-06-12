using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public UserData Data { get; private set; }

    private const string AES_KEY = "CheezyPizza_2026"; // 16 ký tự = AES-128
    private const string AES_IV  = "Savoround_IV2026"; // 16 ký tự

    private const string SAVE_FILE = "save.dat";

    public event Action OnDataChanged;

    public event Action<int> OnGoldEarned;       // amount
    public event Action<string> OnSkinUnlocked;  // skinID


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void OnApplicationQuit()
    {
        Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Save();
    }

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);

            string path = GetSavePath();
            File.WriteAllText(path, json);

        }
        catch (Exception)
        {
        }
    }
    public void Load()
    {
        string path = GetSavePath();

        if (!File.Exists(path))
        {
            Data = CreateDefaultData();
            Save();
            return;
        }

        try
        {
            string fileContent = File.ReadAllText(path);

            string json;
            if (fileContent.TrimStart().StartsWith("{"))
            {
                json = fileContent;
            }
            else
            {
                json = Decrypt(fileContent);
            }

            Data = JsonUtility.FromJson<UserData>(json);

        }
        catch (Exception)
        {
            Data = CreateDefaultData();
            Save();
        }
    }

    public void DeleteSave()
    {
        string path = GetSavePath();
        if (File.Exists(path))
            File.Delete(path);

        PlayerPrefs.DeleteAll();

        Data = CreateDefaultData();
        OnDataChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Data.Gold += amount;
        Data.TotalGoldEarned += amount;
        OnGoldEarned?.Invoke(amount);
        OnDataChanged?.Invoke();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (Data.Gold < amount)
        {
            return false;
        }
        Data.Gold -= amount;
        OnDataChanged?.Invoke();
        return true;
    }

    public int GetGold() => Data.Gold;

    public void UnlockSkin(string skinID)
    {
        if (string.IsNullOrEmpty(skinID)) return;
        if (!Data.OwnedSkins.Contains(skinID))
        {
            Data.OwnedSkins.Add(skinID);
            OnSkinUnlocked?.Invoke(skinID);
            OnDataChanged?.Invoke();
        }
    }

    public bool HasSkin(string skinID)
    {
        return Data.OwnedSkins.Contains(skinID);
    }

    public bool EquipSkin(string skinID)
    {
        if (!HasSkin(skinID)) return false;
        Data.EquippedSkin = skinID;
        OnDataChanged?.Invoke();
        return true;
    }

    public void UnlockNextLevel()
    {
        Data.HighestUnlockedLevel = Mathf.Max(Data.HighestUnlockedLevel, Data.CurrentLevel + 1);
        OnDataChanged?.Invoke();
    }

    public void UpdateHighScore(int score)
    {
        if (score > Data.HighScore)
        {
            Data.HighScore = score;
            OnDataChanged?.Invoke();
        }
    }

    public void UpdateQuestProgress(string questID, int progressDelta)
    {
        QuestData quest = Data.Quests.Find(q => q.QuestID == questID);
        if (quest == null)
        {
            return;
        }

        if (quest.IsCompleted) return;

        quest.Progress = Mathf.Min(quest.Progress + progressDelta, quest.Target);
        if (quest.Progress >= quest.Target)
        {
            quest.IsCompleted = true;
        }

        OnDataChanged?.Invoke();
    }

    public bool ClaimQuestReward(string questID, int goldReward)
    {
        QuestData quest = Data.Quests.Find(q => q.QuestID == questID);
        if (quest == null || !quest.IsCompleted || quest.IsRewardClaimed) return false;

        quest.IsRewardClaimed = true;
        AddGold(goldReward);
        return true;
    }
    public static int[] DailyGoldRewards = { 50, 75, 100, 150, 200, 300, 500 };
    public bool CanClaimDailyReward()
    {
        if (string.IsNullOrEmpty(Data.LastDailyClaimUTC))
            return true;

        if (!DateTime.TryParse(Data.LastDailyClaimUTC, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
        {
            return true;
        }

        DateTime nowUTC = DateTime.UtcNow;
        DateTime lastClaimDate = lastClaim.Date;
        DateTime todayDate = nowUTC.Date;

        if (todayDate < lastClaimDate)
        {
            return false;
        }
        int daysPassed = (todayDate - lastClaimDate).Days;
        return daysPassed >= 1;
    }

    public int GetDaysSinceLastClaim()
    {
        if (string.IsNullOrEmpty(Data.LastDailyClaimUTC)) return -1;

        if (!DateTime.TryParse(Data.LastDailyClaimUTC, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
            return -1;

        return (DateTime.UtcNow.Date - lastClaim.Date).Days;
    }

    public int ClaimDailyReward()
    {
        if (!CanClaimDailyReward())
        {
            return 0;
        }

        int daysSince = GetDaysSinceLastClaim();
        if (daysSince < 0 || daysSince > 1)
        {
            Data.DailyRewardDay = 0;
            Data.DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };
        }
        else
        {
            Data.DailyRewardDay++;

            if (Data.DailyRewardDay >= 7)
            {
                Data.DailyRewardDay = 0;
                Data.DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };
            }
        }

        int day = Data.DailyRewardDay;
        int goldReward = (day >= 0 && day < DailyGoldRewards.Length) ? DailyGoldRewards[day] : 50;

        Data.DailyRewardClaimed[day] = true;

        Data.LastDailyClaimUTC = DateTime.UtcNow.ToString("o");

        AddGold(goldReward);

        Save();

        return goldReward;
    }

    public void ResetDailyReward()
    {
        Data.DailyRewardDay = -1;
        Data.LastDailyClaimUTC = "";
        Data.DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };
        Save();
        OnDataChanged?.Invoke();
    }

    public void SkipOneDay()
    {
        if (string.IsNullOrEmpty(Data.LastDailyClaimUTC)) return;

        if (DateTime.TryParse(Data.LastDailyClaimUTC, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
        {
            Data.LastDailyClaimUTC = lastClaim.AddDays(-1).ToString("o");
            Save();
            OnDataChanged?.Invoke();
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        Data.SoundEnabled = enabled;
        OnDataChanged?.Invoke();
    }

    public void SetMusicEnabled(bool enabled)
    {
        Data.MusicEnabled = enabled;
        OnDataChanged?.Invoke();
    }

    private UserData CreateDefaultData()
    {
        UserData data = new UserData();
        data.Gold = 0;
        data.HighestUnlockedLevel = 1;
        data.CurrentLevel = 1;
        data.HighScore = 0;
        data.OwnedSkins = new List<string> { "default" };
        data.EquippedSkin = "default";
        data.SoundEnabled = true;
        data.MusicEnabled = true;

        data.DailyRewardDay = -1;
        data.LastDailyClaimUTC = "";
        data.DailyRewardClaimed = new List<bool> { false, false, false, false, false, false, false };

        data.Quests = new List<QuestData>
        {
            new QuestData { QuestID = "quest_play_5",   Progress = 0, Target = 5,  IsCompleted = false, IsRewardClaimed = false },
            new QuestData { QuestID = "quest_combo_3",   Progress = 0, Target = 3,  IsCompleted = false, IsRewardClaimed = false },
            new QuestData { QuestID = "quest_earn_500",  Progress = 0, Target = 500, IsCompleted = false, IsRewardClaimed = false }
        };

        return data;
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE);
    }

    private string Encrypt(string plainText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
        byte[] ivBytes = Encoding.UTF8.GetBytes(AES_IV);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    private string Decrypt(string encryptedText)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
        byte[] ivBytes = Encoding.UTF8.GetBytes(AES_IV);

        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }


    public AchievementData GetAchievement(string id)
    {
        return Data.Achievements.Find(a => a.AchievementID == id);
    }

    public void UpdateAchievement(string id, int progress, bool unlocked)
    {
        AchievementData ach = GetAchievement(id);
        if (ach == null)
        {
            ach = new AchievementData(id);
            Data.Achievements.Add(ach);
        }
        ach.Progress = progress;
        ach.IsUnlocked = unlocked;
        OnDataChanged?.Invoke();
    }

    public bool ClaimAchievementReward(string id, int goldReward)
    {
        AchievementData ach = GetAchievement(id);
        if (ach == null || !ach.IsUnlocked || ach.IsRewardClaimed) return false;

        ach.IsRewardClaimed = true;
        AddGold(goldReward);
        return true;
    }
}
