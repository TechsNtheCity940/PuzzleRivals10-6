using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Loading screen that displays active player information during matchmaking.
/// Shows each player's puzzletag (name) and player card (placeholder or resolved).
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public Transform cardContainer;           // Parent object to instantiate player cards under
    public GameObject playerCardPrefab;       // Prefab for the player card display
    public TextMeshProUGUI statusText;        // Main status message ("SEARCHING FOR PLAYERS...")
    public TextMeshProUGUI subText;           // Sub status ("2 real players, 2 AI bots")
    public Image background;                  // Optional background overlay

    [Header("Card Layout")]
    public Vector2 cardSpacing = new Vector2(0, -40);
    public int cardWidth = 200;
    public Color humanCardColor = new Color(0f, 0.9f, 1f);
    public Color botCardColor = new Color(0.5f, 0.5f, 0.5f);

    private List<GameObject> _activeCards = new List<GameObject>();

    /// <summary>
    /// Creates and displays the loading screen with all player slot information.
    /// </summary>
    public void Show(int totalSlots, List<int> humanSlots, List<int> aiSlots, int localSlot)
    {
        gameObject.SetActive(true);
        ClearCards();

        // Create a card for each slot
        for (int i = 0; i < totalSlots; i++)
        {
            bool isHuman = humanSlots != null && humanSlots.Contains(i);
            bool isLocal = i == localSlot;
            CreatePlayerCard(i, isHuman, isLocal);
        }

        RefreshStatus(humanSlots?.Count ?? 0, aiSlots?.Count ?? 0);
    }

    public void RefreshStatus(int realPlayerCount, int botCount)
    {
        if (statusText != null)
        {
            statusText.text = "SEARCHING FOR PLAYERS...";
            statusText.color = realPlayerCount > 0 ? Color.cyan : Color.yellow;
        }

        if (subText != null)
        {
            subText.text = $"{realPlayerCount} real{(realPlayerCount == 1 ? "" : "s")}  -  {botCount} AI bot{(botCount == 1 ? "" : "s")}";
        }
    }

    public void ShowMatchFound(int realPlayerCount, int botCount)
    {
        if (statusText != null)
        {
            statusText.text = "MATCH FOUND";
            statusText.color = Color.green;
        }

        if (subText != null)
        {
            subText.text = $"{realPlayerCount} real{(realPlayerCount == 1 ? "" : "s")}  -  {botCount} AI bot{(botCount == 1 ? "" : "s")}";
        }
    }

    public void Hide()
    {
        ClearCards();
        gameObject.SetActive(false);
    }

    private void ClearCards()
    {
        foreach (var card in _activeCards)
        {
            if (card != null) Destroy(card);
        }
        _activeCards.Clear();
    }

    private void CreatePlayerCard(int slot, bool isHuman, bool isLocal)
    {
        if (playerCardPrefab == null || cardContainer == null) return;

        var cardObj = Instantiate(playerCardPrefab, cardContainer);
        var card = cardObj.GetComponent<PlayerCard>();

        // Determine display name
        string displayName;
        if (isLocal)
        {
            displayName = "YOU";
        }
        else if (isHuman)
        {
            displayName = $"Player {slot + 1}";
        }
        else
        {
            displayName = "AI Bot";
        }

        // Create placeholder player for card binding
        var player = new Player(slot, displayName, isLocal);
        player.isBot = !isHuman;

        // Set card visuals
        if (card != null)
        {
            var loadout = new CosmeticLoadout(); // Uses defaults
            card.Bind(player, loadout);
        }
        else
        {
            var textObj = cardObj.transform.Find("NameText");
            if (textObj != null && textObj.GetComponent<TextMeshProUGUI>() != null)
            {
                textObj.GetComponent<TextMeshProUGUI>().text = isLocal ? "YOU" : (isHuman ? $"Player {slot + 1}" : "AI Bot");
            }
        }

        // Position the card
        var rt = cardObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(0, -slot * cardSpacing.y);
        }

        _activeCards.Add(cardObj);
    }

    public void AddSlotCard(int slot, bool isHuman, bool isLocal)
    {
        if (playerCardPrefab == null || cardContainer == null) return;

        var cardObj = Instantiate(playerCardPrefab, cardContainer);
        var rt = cardObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(0, -slot * cardSpacing.y);
        }

        _activeCards.Add(cardObj);
    }

    private void Awake()
    {
        if (background == null)
        {
            background = GetComponent<Image>();
        }
    }

    private void OnDestroy()
    {
        ClearCards();
    }
}  
