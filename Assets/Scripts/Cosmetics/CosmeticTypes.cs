using System;
using UnityEngine;

// Cosmetic categories present in the art library (Assets/Art/<Category>).
public enum CosmeticCategory { Frame, Nameplate, Banner, BackgroundCard, Emblem, Avatar }

// Rarity tiers parsed from the asset filenames (e.g. "epic_01"). "Elite" is used
// for the special named season pieces (e.g. AuroraEliteFrameS1).
public enum CosmeticRarity { Basic, Uncommon, Rare, Epic, Legendary, Grandmaster, Elite }

// A single catalogued cosmetic. id is unique within its category and stable
// (assigned by rarity then name), so it can be stored as an equipped/unlocked id
// in PlayerProfileData.
[Serializable]
public class CosmeticItem
{
    public int id;
    public string key;            // filename without extension, e.g. "epic_01"
    public CosmeticCategory category;
    public CosmeticRarity rarity;
    public Sprite sprite;
}

// A resolved set of sprites to display on a player card (banner is for the
// profile/dashboard, not the small FFA card).
public struct CosmeticLoadout
{
    public Sprite background;
    public Sprite frame;
    public Sprite nameplate;
    public Sprite emblem;
    public Sprite banner;
    public Sprite avatar;
}
