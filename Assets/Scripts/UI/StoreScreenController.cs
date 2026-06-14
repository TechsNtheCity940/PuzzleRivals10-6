using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Store screen. Sells cosmetics from the CosmeticLibrary by rarity-based price
// (coins; Elite pieces cost gems). Emblems are rank-earned and not sold here.
// Tapping an affordable, unowned tile purchases it; owned tiles are marked.
public class StoreScreenController : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text coinsText;
    public TMP_Text gemsText;
    public TMP_Text messageText;

    [Header("Category Tabs")]
    public Button frameTab;
    public Button nameplateTab;
    public Button bannerTab;
    public Button backgroundTab;

    [Header("Grid")]
    public CosmeticTile tilePrefab;
    public RectTransform gridContainer;

    [Header("Nav")]
    public Button backButton;
    public string mainMenuScene = "Main";

    private CosmeticCategory currentCategory = CosmeticCategory.Frame;
    private readonly List<CosmeticTile> tiles = new List<CosmeticTile>();
    private float messageTimer;

    private void Start()
    {
        if (frameTab != null) frameTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Frame));
        if (nameplateTab != null) nameplateTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Nameplate));
        if (bannerTab != null) bannerTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.Banner));
        if (backgroundTab != null) backgroundTab.onClick.AddListener(() => SelectCategory(CosmeticCategory.BackgroundCard));
        if (backButton != null) backButton.onClick.AddListener(Back);

        if (messageText != null) messageText.text = "";
        RefreshHeader();
        SelectCategory(CosmeticCategory.Frame);
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && messageText != null) messageText.text = "";
        }
    }

    private void RefreshHeader()
    {
        var p = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (p == null) return;
        if (coinsText != null) coinsText.text = p.coins.ToString();
        if (gemsText != null) gemsText.text = p.gems.ToString();
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
    }

    private void SetTabActive(Button tab, bool active)
    {
        if (tab == null) return;
        var img = tab.GetComponent<Image>();
        if (img != null) img.color = active ? new Color(0.5f, 0.35f, 0.1f) : new Color(0.18f, 0.18f, 0.22f);
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
        var item = tile.Item;
        bool owned = CosmeticInventory.IsOwned(tile.Category, item);
        if (owned)
        {
            tile.SetState("OWNED", new Color(0.1f, 0.5f, 0.3f), false);
            tile.SetInteractable(false);
        }
        else if (CosmeticInventory.IsGemOnly(item.rarity))
        {
            tile.SetState($"{CosmeticInventory.ELITE_GEM_PRICE} GEM", new Color(0.5f, 0.2f, 0.55f), false);
            tile.SetInteractable(true);
        }
        else
        {
            tile.SetState($"{CosmeticInventory.CoinPrice(item.rarity)}", new Color(0.45f, 0.4f, 0.1f), false);
            tile.SetInteractable(true);
        }
    }

    private void OnTileClicked(CosmeticTile tile)
    {
        if (CosmeticInventory.TryPurchase(tile.Category, tile.Item, out string msg))
        {
            ShowMessage(msg, new Color(0.4f, 1f, 0.5f));
            RefreshHeader();
            foreach (var t in tiles) UpdateTileState(t);
        }
        else
        {
            ShowMessage(msg, new Color(1f, 0.5f, 0.4f));
        }
    }

    private void ShowMessage(string msg, Color color)
    {
        if (messageText == null) return;
        messageText.text = msg;
        messageText.color = color;
        messageTimer = 2.5f;
    }

    private void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
}
