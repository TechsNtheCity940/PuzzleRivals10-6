using System;
using System.Collections.Generic;

// Shared pure-data types used by the meta-game managers (Store, Season Pass,
// Ranked / Brain-vs-Brain). These are plain serializable data holders — they
// have no existing equivalent in the project, so they are defined here to let
// the managers compile and operate against the existing economy
// (ProfileManager / PlayerProfileData with int-id unlock lists).

public enum ItemType
{
    Banner,
    Frame,
    Avatar,
    Emblem
}

[Serializable]
public class CustomizationItem
{
    public int id;
    public string itemName;
    public ItemType type;
    public int coinPrice;
    public int gemPrice;
}

[Serializable]
public class StoreBundle
{
    public string bundleName;
    public float usdPrice;
    public List<CustomizationItem> items = new List<CustomizationItem>();
}

[Serializable]
public class StoreConfig
{
    public List<CustomizationItem> items = new List<CustomizationItem>();
    public List<StoreBundle> bundles = new List<StoreBundle>();
}

[Serializable]
public class SeasonPassTier
{
    public int level;
    public int xpRequired;
    public CustomizationItem freeReward;
    public CustomizationItem premiumReward;
}

[Serializable]
public class SeasonPassConfig
{
    public float premiumPassPriceUSD = 9.99f;
    public List<SeasonPassTier> tiers = new List<SeasonPassTier>();
}

[Serializable]
public class MatchResult
{
    public string playerName;
    public int score;
    public bool isWinner;
    public int xpGained;
    public int coinsGained;
}
