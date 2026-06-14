using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProfileData
{
    public string playerName = "New Rival";
    public int bannerId;
    public int emblemId;
    public int frameId;
    public int avatarId;
    public int nameplateId;
    public int backgroundCardId;
    public int coins = 100;
    public int gems = 10;
    public int xp = 0;
    public int level = 1;
    
    [Header("Stats")]
    public int totalWins;
    public int totalLosses;
    public int totalPuzzlesSolved;
    public int rankPoints;
    
    [Header("Unlocks")]
    public List<int> unlockedBanners = new List<int> { 0 };
    public List<int> unlockedEmblems = new List<int> { 0 };
    public List<int> unlockedFrames = new List<int> { 0 };
    public List<int> unlockedAvatars = new List<int> { 0 };
    public List<int> unlockedNameplates = new List<int> { 0 };
    public List<int> unlockedBackgroundCards = new List<int> { 0 };
    
    public bool isPremiumSeasonPassActive = false;

    // VIP grants every cosmetic (all categories incl. gem-only Elite) for free.
    public bool isVip = false;

    // Stable local identifier used by admin tooling to target VIP grants.
    public string playerId = "";
}

// Single source of truth for the player's persistent profile: identity
// (playerName), economy (coins/gems), progression (xp/level for the Season Pass,
// rankPoints for competitive rank), stats, and cosmetic unlocks/equips. This is
// the unified profile — the former PlayerProfile (rp/level/username JSON) has
// been folded in here; AwardRp drives ranked progression for both match modes.
public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    public PlayerProfileData currentProfile;

    // Fired whenever the profile changes in a way screens should reflect
    // (rank/stats/currency). UI subscribes to refresh.
    public event System.Action OnProfileChanged;

    // Competitive rank tier names, indexed to match CosmeticLibrary emblems
    // (Bronze .. Legend) so the rank label and the equipped emblem stay aligned.
    public static readonly string[] RankNames =
        { "BRONZE", "SILVER", "GOLD", "PLATINUM", "DIAMOND", "MASTER", "GRANDMASTER", "LEGEND" };

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

    public void SaveProfile()
    {
        string json = JsonUtility.ToJson(currentProfile);
        PlayerPrefs.SetString("PlayerProfile", json);
        PlayerPrefs.Save();
    }

    private void LoadProfile()
    {
        if (PlayerPrefs.HasKey("PlayerProfile"))
        {
            string json = PlayerPrefs.GetString("PlayerProfile");
            currentProfile = JsonUtility.FromJson<PlayerProfileData>(json);
        }
        else
        {
            currentProfile = new PlayerProfileData();
            MigrateLegacyProfile();
        }
    }

    // One-time import of the old PlayerProfile JSON (persistentDataPath/profile.json)
    // so a player's name / rank / coins survive the system unification.
    private void MigrateLegacyProfile()
    {
        try
        {
            string legacyPath = System.IO.Path.Combine(Application.persistentDataPath, "profile.json");
            if (!System.IO.File.Exists(legacyPath)) return;

            var legacy = JsonUtility.FromJson<LegacyProfile>(System.IO.File.ReadAllText(legacyPath));
            if (legacy == null) return;

            if (!string.IsNullOrEmpty(legacy.username)) currentProfile.playerName = legacy.username;
            currentProfile.rankPoints = legacy.rp;
            currentProfile.totalWins = legacy.matchesWon;
            currentProfile.totalLosses = Mathf.Max(0, legacy.matchesPlayed - legacy.matchesWon);
            if (legacy.coins > 0) currentProfile.coins = legacy.coins;
            SaveProfile();
            Debug.Log("ProfileManager: migrated legacy PlayerProfile data.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("ProfileManager: legacy migration skipped (" + e.Message + ").");
        }
    }

    // Awards competitive rank points and records the match result. Used by both
    // RankedMatchManager and BrainVSBrainManager at match end.
    public void AwardRp(int amount, bool won)
    {
        if (currentProfile == null) return;
        currentProfile.rankPoints += amount;
        if (won) currentProfile.totalWins++;
        else currentProfile.totalLosses++;
        SaveProfile();
        OnProfileChanged?.Invoke();
    }

    // Current competitive rank index (0 = Bronze) derived from rankPoints via the
    // CosmeticLibrary thresholds, so rank label and emblem always match.
    public int GetRankIndex()
    {
        var lib = CosmeticLibrary.Instance;
        if (lib == null || currentProfile == null) return 0;
        return lib.RankIndexForPoints(currentProfile.rankPoints);
    }

    public string GetRankName()
    {
        int idx = Mathf.Clamp(GetRankIndex(), 0, RankNames.Length - 1);
        return RankNames[idx];
    }

    // Minimal shape of the former PlayerProfile.ProfileData for migration only.
    [System.Serializable]
    private class LegacyProfile
    {
        public string username;
        public int rp;
        public int matchesPlayed;
        public int matchesWon;
        public int coins;
    }
}
