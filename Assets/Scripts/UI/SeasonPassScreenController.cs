using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Season Pass screen. Shows the seasonal track (free + premium rewards per
// tier), the player's level/XP progress, and a premium-pass purchase button.
// Premium purchase routes through the existing SeasonPassManager/PayPal path.
public class SeasonPassScreenController : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text levelText;
    public TMP_Text xpText;
    public Image xpFill;
    public TMP_Text premiumStatusText;

    [Header("Track")]
    public SeasonTierCell cellPrefab;
    public RectTransform trackContent;

    [Header("Premium")]
    public Button buyPremiumButton;

    [Header("Nav")]
    public Button backButton;
    public string mainMenuScene = "Main";

    private readonly List<SeasonTierCell> cells = new List<SeasonTierCell>();

    private void Start()
    {
        if (buyPremiumButton != null) buyPremiumButton.onClick.AddListener(OnBuyPremium);
        if (backButton != null) backButton.onClick.AddListener(Back);

        if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.EnsureDefaultConfig();

        BuildTrack();
        RefreshHeader();
    }

    private void RefreshHeader()
    {
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (p == null) return;

        if (levelText != null) levelText.text = "TIER " + p.level;

        // Progress toward the next tier (XpPerTier per tier).
        int per = SeasonPassManager.XpPerTier;
        int withinTier = per > 0 ? p.xp % per : 0;
        if (xpFill != null) xpFill.fillAmount = per > 0 ? (float)withinTier / per : 0f;
        if (xpText != null) xpText.text = $"{withinTier} / {per} XP";

        bool premium = p.isPremiumSeasonPassActive;
        if (premiumStatusText != null)
            premiumStatusText.text = premium ? "PREMIUM ACTIVE" : "FREE TRACK";
        if (buyPremiumButton != null) buyPremiumButton.gameObject.SetActive(!premium);
    }

    private void BuildTrack()
    {
        if (cellPrefab == null || trackContent == null) return;
        foreach (var c in cells) if (c != null) Destroy(c.gameObject);
        cells.Clear();

        var mgr = SeasonPassManager.Instance;
        var lib = CosmeticLibrary.Instance;
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (mgr == null || mgr.config == null || mgr.config.tiers == null || p == null) return;

        foreach (var tier in mgr.config.tiers)
        {
            SeasonTierCell cell = Instantiate(cellPrefab, trackContent);
            cell.gameObject.SetActive(true);

            Sprite freeSprite = RewardSprite(lib, tier.freeReward);
            Sprite premiumSprite = RewardSprite(lib, tier.premiumReward);
            bool reached = p.level >= tier.level;
            bool isCurrent = p.level == tier.level || (p.level < 1 && tier.level == 1);

            cell.Setup(tier.level, freeSprite, premiumSprite, reached, p.isPremiumSeasonPassActive, isCurrent);
            cells.Add(cell);
        }
    }

    private Sprite RewardSprite(CosmeticLibrary lib, CustomizationItem reward)
    {
        if (lib == null || reward == null) return null;
        switch (reward.type)
        {
            case ItemType.Frame: return lib.SpriteById(CosmeticCategory.Frame, reward.id);
            case ItemType.Banner: return lib.SpriteById(CosmeticCategory.Banner, reward.id);
            case ItemType.Emblem: return lib.SpriteById(CosmeticCategory.Emblem, reward.id);
            default: return null;
        }
    }

    private void OnBuyPremium()
    {
        if (SeasonPassManager.Instance != null)
            SeasonPassManager.Instance.PurchasePremiumPass();
        // Reflect immediately if the (possibly synchronous) purchase succeeded.
        RefreshHeader();
        BuildTrack();
    }

    private void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
}
