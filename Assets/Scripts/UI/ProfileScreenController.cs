using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Profile / dashboard screen. Shows the local player's live card preview
// (equipped cosmetics + rank emblem), header stats, and equip grids for each
// cosmetic category (Frame, Nameplate, Banner, Background). Tapping an owned
// item equips it and updates the preview. Emblem is rank-earned (read-only).
public class ProfileScreenController : MonoBehaviour
{
    [Header("Header")]
    public Image bannerShowcase;   // wide banner behind the header (equipped banner)
    public TMP_Text nameText;      // Keep for compatibility or set inactive
    public TMP_InputField nameInputField; // Input field to edit the player's name
    public TMP_Text rankText;
    public TMP_Text statsText;
    public TMP_Text coinsText;
    public TMP_Text gemsText;

    [Header("Preview Card")]
    public PlayerCard previewCard;     // an instance placed in the scene

    [Header("Category Tabs")]
    public Button frameTab;
    public Button nameplateTab;
    public Button bannerTab;
    public Button backgroundTab;
    public Button avatarTab; // Added Avatar Tab Button

    [Header("Grid")]
    public CosmeticTile tilePrefab;
    public RectTransform gridContainer;

    [Header("Nav")]
    public Button backButton;
    public string mainMenuScene = "Main";

    private CosmeticCategory currentCategory = CosmeticCategory.Frame;
    private readonly List<CosmeticTile> tiles = new List<CosmeticTile>();

    private void Start()
    {
        if (frameTab != null) frameTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Frame));
        if (nameplateTab != null) nameplateTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Nameplate));
        if (bannerTab != null) bannerTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Banner));
        if (backgroundTab != null) backgroundTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.BackgroundCard));
        if (avatarTab != null) avatarTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Avatar)); // Listen for avatar tab
        if (backButton != null) backButton.onClick.AddListener(Back);

        // Setup Username Editing
        if (nameInputField != null)
        {
            var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
            if (p != null)
            {
                nameInputField.text = p.playerName;
            }
            nameInputField.onEndEdit.AddListener(OnNameChanged);
        }

        RefreshHeader();
        RefreshPreview();
        SelectCategory(CosmeticCategory.Frame);
    }

    private void OnNameChanged(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (p != null)
        {
            p.playerName = newName.Trim();
            ProfileManager.Instance.SaveProfile();
            RefreshHeader();
            RefreshPreview();
        }
    }

    private void RefreshHeader()
    {
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (p == null) return;

        if (nameText != null) nameText.text = p.playerName;
        if (nameInputField != null && nameInputField.text != p.playerName) nameInputField.text = p.playerName;
        if (coinsText != null) coinsText.text = p.coins.ToString();
        if (gemsText != null) gemsText.text = p.gems.ToString();
        if (statsText != null) statsText.text = $"WINS {p.totalWins}   LOSSES {p.totalLosses}   SOLVED {p.totalPuzzlesSolved}";

        if (rankText != null && ProfileManager.Instance != null)
            rankText.text = $"{ProfileManager.Instance.GetRankName()}   ({p.rankPoints} RP)";

        RefreshBanner();
    }

    // Shows the player's equipped banner in the header showcase.
    private void RefreshBanner()
    {
        var lib = CosmeticLibrary.Instance;
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (bannerShowcase == null || lib == null || p == null) return;

        Sprite banner = lib.SpriteById(CosmeticCategory.Banner, p.bannerId);
        if (banner != null)
        {
            bannerShowcase.sprite = banner;
            bannerShowcase.color = Color.white;
            bannerShowcase.enabled = true;
        }
    }

    private void RefreshPreview()
    {
        var lib = CosmeticLibrary.Instance;
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (previewCard == null || lib == null || p == null) return;

        var lo = lib.BuildLocalLoadout(p);
        previewCard.ApplyLoadout(lo);
        if (previewCard.nameText != null) previewCard.nameText.text = p.playerName;
        if (previewCard.solvedText != null) previewCard.solvedText.text = "";
    }

    private void SelectCategory(CosmeticCategory category)
    {
        currentCategory = category;
        BuildGrid();
        HighlightTab();
    }

    private void HighlightTab()
    {
        SetTabActive(frameTab, currentCategory == CosmeticCategory.Frame);
        SetTabActive(nameplateTab, currentCategory == CosmeticCategory.Nameplate);
        SetTabActive(bannerTab, currentCategory == CosmeticCategory.Banner);
        SetTabActive(backgroundTab, currentCategory == CosmeticCategory.BackgroundCard);
        SetTabActive(avatarTab, currentCategory == CosmeticCategory.Avatar);
    }

    private void SetTabActive(Button tab, bool active)
    {
        if (tab == null) return;
        var img = tab.GetComponent<Image>();
        if (img != null) img.color = active ? new Color(0.1f, 0.5f, 0.6f) : new Color(0.18f, 0.18f, 0.22f);
    }

    private void BuildGrid()
    {
        if (tilePrefab == null || gridContainer == null) return;
        foreach (var t in tiles) if (t != null) Destroy(t.gameObject);
        tiles.Clear();

        var lib = CosmeticLibrary.Instance;
        if (lib == null) return;
        var list = lib.GetList(currentCategory);
        if (list == null) return;

        foreach (var item in list)
        {
            // Profile shows only OWNED items (equip-only screen).
            if (!CosmeticInventory.IsOwned(currentCategory, item)) continue;

            CosmeticTile tile = Instantiate(tilePrefab, gridContainer);
            tile.gameObject.SetActive(true);
            tile.Setup(currentCategory, item);
            UpdateTileState(tile);
            tile.OnClicked += OnTileClicked;
            tiles.Add(tile);
        }
    }

    private void UpdateTileState(CosmeticTile tile)
    {
        bool equipped = CosmeticInventory.IsEquipped(tile.Category, tile.Item);
        tile.SetState(equipped ? "EQUIPPED" : "OWNED",
            equipped ? new Color(0.1f, 0.6f, 0.3f) : new Color(0.25f, 0.25f, 0.3f),
            equipped);
    }

    private void OnTileClicked(CosmeticTile tile)
    {
        if (CosmeticInventory.TryEquip(tile.Category, tile.Item))
        {
            foreach (var t in tiles) UpdateTileState(t);
            RefreshPreview();
            if (tile.Category == CosmeticCategory.Banner) RefreshBanner();
        }
    }

    private void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
}
