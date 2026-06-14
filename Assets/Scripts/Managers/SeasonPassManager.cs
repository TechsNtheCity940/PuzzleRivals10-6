using UnityEngine;
using System.Collections.Generic;

public class SeasonPassManager : MonoBehaviour
{
    public static SeasonPassManager Instance;
    public SeasonPassConfig config;

    public const int TierCount = 15;
    public const int XpPerTier = 500;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureDefaultConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Builds a default season track from the CosmeticLibrary if none was authored.
    // Free rewards = frames; premium rewards = banners (both map cleanly to the
    // PlayerProfileData unlock lists via their library ids).
    public void EnsureDefaultConfig()
    {
        if (config != null && config.tiers != null && config.tiers.Count > 0) return;

        config = new SeasonPassConfig { premiumPassPriceUSD = 9.99f, tiers = new List<SeasonPassTier>() };
        var lib = CosmeticLibrary.Instance;
        int frameCount = lib != null && lib.frames != null ? lib.frames.Count : 0;
        int bannerCount = lib != null && lib.banners != null ? lib.banners.Count : 0;

        for (int i = 1; i <= TierCount; i++)
        {
            var tier = new SeasonPassTier { level = i, xpRequired = i * XpPerTier };
            if (frameCount > 0)
            {
                var f = lib.frames[(i - 1) % frameCount];
                tier.freeReward = new CustomizationItem { id = f.id, itemName = f.key, type = ItemType.Frame };
            }
            if (bannerCount > 0)
            {
                var b = lib.banners[(i - 1) % bannerCount];
                tier.premiumReward = new CustomizationItem { id = b.id, itemName = b.key, type = ItemType.Banner };
            }
            config.tiers.Add(tier);
        }
    }

    public void AddXP(int amount)
    {
        if (ProfileManager.Instance == null) return;
        
        var profile = ProfileManager.Instance.currentProfile;
        profile.xp += amount;
        
        CheckLevelUp(profile);
        ProfileManager.Instance.SaveProfile();
    }

    private void CheckLevelUp(PlayerProfileData profile)
    {
        // Simple logic: find highest reached tier
        SeasonPassTier highestReached = null;
        foreach (var tier in config.tiers)
        {
            if (profile.xp >= tier.xpRequired)
            {
                if (highestReached == null || tier.level > highestReached.level)
                {
                    highestReached = tier;
                }
            }
        }

        if (highestReached != null && highestReached.level > profile.level)
        {
            // Grant all rewards from current level + 1 to highestReached
            for (int i = profile.level + 1; i <= highestReached.level; i++)
            {
                var tier = config.tiers.Find(t => t.level == i);
                if (tier != null) GrantRewards(tier, profile);
            }
            profile.level = highestReached.level;
        }
    }

    private void GrantRewards(SeasonPassTier tier, PlayerProfileData profile)
    {
        if (tier.freeReward != null) UnlockItem(tier.freeReward, profile);
        if (profile.isPremiumSeasonPassActive && tier.premiumReward != null) UnlockItem(tier.premiumReward, profile);
        
        Debug.Log($"Granted rewards for level {tier.level}");
    }

    private void UnlockItem(CustomizationItem item, PlayerProfileData profile)
    {
        switch (item.type)
        {
            case ItemType.Banner: if (!profile.unlockedBanners.Contains(item.id)) profile.unlockedBanners.Add(item.id); break;
            case ItemType.Frame: if (!profile.unlockedFrames.Contains(item.id)) profile.unlockedFrames.Add(item.id); break;
            case ItemType.Avatar: if (!profile.unlockedAvatars.Contains(item.id)) profile.unlockedAvatars.Add(item.id); break;
            case ItemType.Emblem: if (!profile.unlockedEmblems.Contains(item.id)) profile.unlockedEmblems.Add(item.id); break;
        }
    }

    public void PurchasePremiumPass()
    {
        if (PayPalService.Instance == null)
        {
            Debug.LogError("PayPalService not found!");
            return;
        }

        PayPalService.Instance.InitiatePayment(config.premiumPassPriceUSD, "Season Pass", (success) => {
            if (success)
            {
                ProfileManager.Instance.currentProfile.isPremiumSeasonPassActive = true;
                RetroactiveGrant();
                ProfileManager.Instance.SaveProfile();
                Debug.Log("Premium Season Pass Activated!");
            }
        });
    }

    private void RetroactiveGrant()
    {
        var profile = ProfileManager.Instance.currentProfile;
        for (int i = 1; i <= profile.level; i++)
        {
            var tier = config.tiers.Find(t => t.level == i);
            if (tier != null && tier.premiumReward != null)
            {
                UnlockItem(tier.premiumReward, profile);
            }
        }
    }
}
