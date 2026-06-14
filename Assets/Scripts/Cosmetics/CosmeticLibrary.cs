using System.Collections.Generic;
using UnityEngine;

// Indexed catalogue of every cosmetic asset, grouped by category and ordered by
// rarity. Built by the editor populator and saved to
// Assets/Resources/CosmeticLibrary.asset so it loads at runtime via
// CosmeticLibrary.Instance. Emblems are stored rank-ordered (Bronze..Legend).
[CreateAssetMenu(fileName = "CosmeticLibrary", menuName = "PuzzleRivals/Cosmetic Library")]
public class CosmeticLibrary : ScriptableObject
{
    public List<CosmeticItem> frames = new List<CosmeticItem>();
    public List<CosmeticItem> nameplates = new List<CosmeticItem>();
    public List<CosmeticItem> banners = new List<CosmeticItem>();
    public List<CosmeticItem> backgroundCards = new List<CosmeticItem>();
    public List<CosmeticItem> emblems = new List<CosmeticItem>(); // rank-ordered
    public List<CosmeticItem> avatars = new List<CosmeticItem>(); // player-card portrait

    // RankPoints thresholds mapping to emblem rank index (Bronze .. Legend).
    private static readonly int[] RankThresholds = { 0, 400, 1000, 1900, 3000, 4500, 6500, 9000 };

    private static CosmeticLibrary _instance;
    public static CosmeticLibrary Instance
    {
        get
        {
            if (_instance == null) _instance = Resources.Load<CosmeticLibrary>("CosmeticLibrary");
            return _instance;
        }
    }

    public List<CosmeticItem> GetList(CosmeticCategory c)
    {
        switch (c)
        {
            case CosmeticCategory.Frame: return frames;
            case CosmeticCategory.Nameplate: return nameplates;
            case CosmeticCategory.Banner: return banners;
            case CosmeticCategory.BackgroundCard: return backgroundCards;
            case CosmeticCategory.Emblem: return emblems;
            case CosmeticCategory.Avatar: return avatars;
            default: return null;
        }
    }

    // Number of catalogued avatars.
    public int AvatarCount => avatars != null ? avatars.Count : 0;

    // Resolves an avatar sprite by id (clamped/fallback to first so it never blanks).
    public Sprite AvatarForId(int id)
    {
        if (avatars == null || avatars.Count == 0) return null;
        foreach (var it in avatars) if (it.id == id) return it.sprite;
        return avatars[0].sprite;
    }

    public CosmeticItem GetById(CosmeticCategory c, int id)
    {
        var list = GetList(c);
        if (list == null || list.Count == 0) return null;
        foreach (var it in list) if (it.id == id) return it;
        return list[0]; // fallback to first so display never blanks
    }

    public Sprite SpriteById(CosmeticCategory c, int id)
    {
        var it = GetById(c, id);
        return it != null ? it.sprite : null;
    }

    public CosmeticItem GetRandom(CosmeticCategory c, System.Random rng)
    {
        var list = GetList(c);
        if (list == null || list.Count == 0) return null;
        return list[rng.Next(list.Count)];
    }

    // Random pick excluding the opaque "Elite" named pieces (those would cover a
    // card if used as a transparent-border frame). Falls back to full list.
    public CosmeticItem GetRandomNonElite(CosmeticCategory c, System.Random rng)
    {
        var list = GetList(c);
        if (list == null || list.Count == 0) return null;
        var filtered = list.FindAll(x => x.rarity != CosmeticRarity.Elite);
        if (filtered.Count == 0) filtered = list;
        return filtered[rng.Next(filtered.Count)];
    }

    // ---- Loadout resolution ----

    // Local player's equipped cosmetics; emblem reflects their current rank tier.
    public CosmeticLoadout BuildLocalLoadout(PlayerProfileData profile)
    {
        var lo = new CosmeticLoadout();
        if (profile == null) return lo;
        lo.background = SpriteById(CosmeticCategory.BackgroundCard, profile.backgroundCardId);
        lo.frame = SpriteById(CosmeticCategory.Frame, profile.frameId);
        lo.nameplate = SpriteById(CosmeticCategory.Nameplate, profile.nameplateId);
        lo.banner = SpriteById(CosmeticCategory.Banner, profile.bannerId);
        lo.emblem = EmblemForRank(RankIndexForPoints(profile.rankPoints));
        lo.avatar = AvatarForId(profile.avatarId);
        return lo;
    }

    // A random rival's cosmetics (deterministic per seeded rng). Emblem is a
    // random rank tier so rivals look varied.
    public CosmeticLoadout BuildRandomLoadout(System.Random rng)
    {
        var lo = new CosmeticLoadout();
        var bg = GetRandomNonElite(CosmeticCategory.BackgroundCard, rng);
        var fr = GetRandomNonElite(CosmeticCategory.Frame, rng);
        var np = GetRandomNonElite(CosmeticCategory.Nameplate, rng);
        var bn = GetRandomNonElite(CosmeticCategory.Banner, rng);
        lo.background = bg != null ? bg.sprite : null;
        lo.frame = fr != null ? fr.sprite : null;
        lo.nameplate = np != null ? np.sprite : null;
        lo.banner = bn != null ? bn.sprite : null;
        lo.emblem = (emblems != null && emblems.Count > 0) ? EmblemForRank(rng.Next(emblems.Count)) : null;
        return lo;
    }

    public int RankIndexForPoints(int rankPoints)
    {
        int idx = 0;
        for (int i = 0; i < RankThresholds.Length; i++)
            if (rankPoints >= RankThresholds[i]) idx = i;
        return idx;
    }

    public Sprite EmblemForRank(int rankIndex)
    {
        if (emblems == null || emblems.Count == 0) return null;
        int idx = Mathf.Clamp(rankIndex, 0, emblems.Count - 1);
        return emblems[idx].sprite;
    }
}
