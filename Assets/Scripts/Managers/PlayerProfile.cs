using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }

    public event Action OnProfileChanged;
    public event Action<int> OnLevelUp;
    public event Action<string, string> OnItemUnlocked;
    public event Action OnUsernameSet;

    [Serializable]
    public class ItemDefinition
    {
        public string id;
        public string label;
        public string source;
        public int level;
        public Color color;
        public Color[] colors;
        public string type;
    }

    [Serializable]
    public class RankTier
    {
        public int minLevel;
        public string label;
        public Color color;
    }

    [Serializable]
    public class ProfileData
    {
        public string username = "";
        public int rp = 0;
        public int matchesPlayed = 0;
        public int matchesWon = 0;
        public int seasonId = 1;
        public bool firstRun = true;
        public int coins = 0;

        public List<string> unlockedFrames = new List<string>();
        public string equippedFrame = "frame_basic";
    }

    public ProfileData data = new ProfileData();

    public static readonly List<ItemDefinition> FRAMES = new List<ItemDefinition> {
        new ItemDefinition { id = "frame_basic",   label = "Cyan Edge",     source = "default", level = 1,  color = new Color(0.0f, 0.90f, 1.0f) },
        new ItemDefinition { id = "frame_lime",    label = "Lime Spark",    source = "level",   level = 3,  color = new Color(0.61f, 1.0f, 0.18f) },
        new ItemDefinition { id = "frame_amber",   label = "Amber Halo",    source = "level",   level = 6,  color = new Color(1.0f, 0.54f, 0.12f) },
        new ItemDefinition { id = "frame_magenta", label = "Magenta Pulse", source = "level",   level = 10, color = new Color(1.0f, 0.18f, 0.78f) },
        new ItemDefinition { id = "frame_violet",  label = "Violet Aura",   source = "season",  level = 1,  color = new Color(0.78f, 0.35f, 1.0f) },
        new ItemDefinition { id = "frame_gold",    label = "Gold Crown",    source = "level",   level = 18, color = new Color(1.0f, 0.92f, 0.18f) },
        new ItemDefinition { id = "frame_legend",  label = "Legend",        source = "level",   level = 30, color = new Color(1.0f, 1.0f, 1.0f) }
    };

    public static readonly List<RankTier> RANK_TIERS = new List<RankTier> {
        new RankTier { minLevel = 1,  label = "NOVICE",      color = new Color(0.6f, 0.7f, 0.8f) },
        new RankTier { minLevel = 4,  label = "APPRENTICE",  color = new Color(0.0f, 0.90f, 1.0f) },
        new RankTier { minLevel = 8,  label = "ADEPT",       color = new Color(0.61f, 1.0f, 0.18f) },
        new RankTier { minLevel = 14, label = "ELITE",       color = new Color(1.0f, 0.54f, 0.12f) },
        new RankTier { minLevel = 22, label = "MASTER",      color = new Color(1.0f, 0.18f, 0.78f) },
        new RankTier { minLevel = 32, label = "GRANDMASTER", color = new Color(1.0f, 0.92f, 0.18f) }
    };

    private string SavePath => Path.Combine(Application.persistentDataPath, "profile.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProfile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadProfile()
    {
        if (File.Exists(SavePath))
        {
            try {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<ProfileData>(json);
                data.firstRun = string.IsNullOrEmpty(data.username);
            } catch {
                data = new ProfileData();
                data.firstRun = true;
            }
        }
        else
        {
            data = new ProfileData();
            data.firstRun = true;
            GrantDefaults();
        }
        
        AutoUnlockForLevel(GetLevel());
    }

    public void SaveProfile()
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(SavePath, json);
    }

    public int GetLevel() => RpToLevel(data.rp);

    public int RpToLevel(int amount)
    {
        int n = 1;
        int cum = 0;
        while (true)
        {
            int needed = (int)(50.0f * Mathf.Pow(n, 1.35f));
            if (cum + needed > amount) return n;
            cum += needed;
            n++;
            if (n > 99) return 99;
        }
    }

    public int RpForLevel(int lvl)
    {
        int cum = 0;
        for (int i = 1; i < lvl; i++)
        {
            cum += (int)(50.0f * Mathf.Pow(i, 1.35f));
        }
        return cum;
    }

    public float GetLevelProgress()
    {
        int lvl = GetLevel();
        int currentFloor = RpForLevel(lvl);
        int nextFloor = RpForLevel(lvl + 1);
        if (nextFloor - currentFloor <= 0) return 1.0f;
        return Mathf.Clamp01((float)(data.rp - currentFloor) / (float)(nextFloor - currentFloor));
    }

    public void AwardRp(int amount, bool won)
    {
        int prevLevel = GetLevel();
        data.matchesPlayed++;
        if (won) data.matchesWon++;
        data.rp += amount;
        
        AutoUnlockForLevel(GetLevel());
        SaveProfile();
        
        OnProfileChanged?.Invoke();
        if (GetLevel() > prevLevel)
        {
            OnLevelUp?.Invoke(GetLevel());
        }
    }

    private void GrantDefaults()
    {
        foreach (var f in FRAMES) if (f.source == "default" && !data.unlockedFrames.Contains(f.id)) data.unlockedFrames.Add(f.id);
    }

    private void AutoUnlockForLevel(int lvl)
    {
        foreach (var f in FRAMES)
        {
            if ((f.source == "default" || f.source == "level") && f.level <= lvl && !data.unlockedFrames.Contains(f.id))
            {
                data.unlockedFrames.Add(f.id);
                OnItemUnlocked?.Invoke(f.id, "frame");
            }
        }
    }

    public void SetUsername(string name)
    {
        string clean = name.Trim();
        if (clean.Length > 14) clean = clean.Substring(0, 14);
        if (string.IsNullOrEmpty(clean)) clean = "RIVAL";
        data.username = clean;
        data.firstRun = false;
        SaveProfile();
        OnProfileChanged?.Invoke();
        OnUsernameSet?.Invoke();
    }

    public RankTier GetRankTier()
    {
        int lvl = GetLevel();
        RankTier current = RANK_TIERS[0];
        foreach (var t in RANK_TIERS)
        {
            if (lvl >= t.minLevel) current = t;
        }
        return current;
    }
}
