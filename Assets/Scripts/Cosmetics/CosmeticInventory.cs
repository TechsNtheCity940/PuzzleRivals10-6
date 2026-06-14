using System.Collections.Generic;
using UnityEngine;

// Bridges the CosmeticLibrary catalogue to the player's economy
// (PlayerProfileData unlock lists + equipped ids). Used by the Profile and Store
// screens. Emblems are RANK-EARNED (driven by rankPoints), so they are not
// purchasable here.
public static class CosmeticInventory
{
    // Coin price per rarity. Basic is free (default-owned). Elite is gem-only.
    public const int ELITE_GEM_PRICE = 150;

    public static int CoinPrice(CosmeticRarity rarity)
    {
        switch (rarity)
        {
            case CosmeticRarity.Basic: return 0;
            case CosmeticRarity.Uncommon: return 200;
            case CosmeticRarity.Rare: return 500;
            case CosmeticRarity.Epic: return 1200;
            case CosmeticRarity.Legendary: return 3000;
            case CosmeticRarity.Grandmaster: return 6000;
            default: return 0; // Elite priced in gems, not coins
        }
    }

    public static bool IsGemOnly(CosmeticRarity rarity) => rarity == CosmeticRarity.Elite;

    // ---- Category -> profile field mapping ----

    public static List<int> UnlockedList(PlayerProfileData p, CosmeticCategory c)
    {
        switch (c)
        {
            case CosmeticCategory.Frame: return p.unlockedFrames;
            case CosmeticCategory.Nameplate: return p.unlockedNameplates;
            case CosmeticCategory.Banner: return p.unlockedBanners;
            case CosmeticCategory.BackgroundCard: return p.unlockedBackgroundCards;
            case CosmeticCategory.Emblem: return p.unlockedEmblems;
            case CosmeticCategory.Avatar: return p.unlockedAvatars;
            default: return null;
        }
    }

    public static int GetEquippedId(PlayerProfileData p, CosmeticCategory c)
    {
        switch (c)
        {
            case CosmeticCategory.Frame: return p.frameId;
            case CosmeticCategory.Nameplate: return p.nameplateId;
            case CosmeticCategory.Banner: return p.bannerId;
            case CosmeticCategory.BackgroundCard: return p.backgroundCardId;
            case CosmeticCategory.Emblem: return p.emblemId;
            case CosmeticCategory.Avatar: return p.avatarId;
            default: return 0;
        }
    }

    public static void SetEquippedId(PlayerProfileData p, CosmeticCategory c, int id)
    {
        switch (c)
        {
            case CosmeticCategory.Frame: p.frameId = id; break;
            case CosmeticCategory.Nameplate: p.nameplateId = id; break;
            case CosmeticCategory.Banner: p.bannerId = id; break;
            case CosmeticCategory.BackgroundCard: p.backgroundCardId = id; break;
            case CosmeticCategory.Emblem: p.emblemId = id; break;
            case CosmeticCategory.Avatar: p.avatarId = id; break;
        }
    }

    // ---- Queries ----

    public static bool IsVip()
    {
        var p = Profile();
        return p != null && p.isVip;
    }

    public static bool IsOwned(CosmeticCategory c, CosmeticItem item)
    {
        var p = Profile();
        if (p == null || item == null) return false;
        if (p.isVip) return true; // VIP owns everything (all categories, incl. Elite)
        if (item.rarity == CosmeticRarity.Basic) return true; // basics always owned
        var list = UnlockedList(p, c);
        return list != null && list.Contains(item.id);
    }

    public static bool IsEquipped(CosmeticCategory c, CosmeticItem item)
    {
        var p = Profile();
        return p != null && item != null && GetEquippedId(p, c) == item.id;
    }

    // ---- Actions ----

    // Attempts to buy the item with coins (or gems for Elite). Returns true on
    // success and persists the profile.
    public static bool TryPurchase(CosmeticCategory c, CosmeticItem item, out string message)
    {
        message = "";
        var p = Profile();
        if (p == null || item == null) { message = "No profile."; return false; }
        if (IsOwned(c, item)) { message = "Already owned."; return false; }

        if (IsGemOnly(item.rarity))
        {
            if (p.gems < ELITE_GEM_PRICE) { message = "Not enough gems."; return false; }
            p.gems -= ELITE_GEM_PRICE;
        }
        else
        {
            int price = CoinPrice(item.rarity);
            if (p.coins < price) { message = "Not enough coins."; return false; }
            p.coins -= price;
        }

        var list = UnlockedList(p, c);
        if (list != null && !list.Contains(item.id)) list.Add(item.id);
        Save();
        message = "Purchased " + item.key;
        return true;
    }

    // Equips an owned item (no-op if not owned). Returns true if equipped.
    public static bool TryEquip(CosmeticCategory c, CosmeticItem item)
    {
        var p = Profile();
        if (p == null || item == null) return false;
        if (!IsOwned(c, item)) return false;
        SetEquippedId(p, c, item.id);
        Save();
        return true;
    }

    // ---- Helpers ----

    private static PlayerProfileData Profile()
        => ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;

    private static void Save()
    {
        if (ProfileManager.Instance != null) ProfileManager.Instance.SaveProfile();
    }
}
