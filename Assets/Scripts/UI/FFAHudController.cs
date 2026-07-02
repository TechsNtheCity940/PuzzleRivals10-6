using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Drives the live FFA match HUD: one PlayerCard per player plus a top fuse bar
// showing who currently holds the bomb and how much time is left. Subscribes to
// RankedMatchManager state events; reads the live fuse each frame.
public class FFAHudController : MonoBehaviour
{
    [Header("Card Spawning")]
    public PlayerCard cardPrefab;
    public RectTransform cardContainer;

    [Header("Fuse / Bomb Banner")]
    public Image fuseFill;          // horizontal fill (Image type = Filled)
    public TMP_Text holderText;     // "GLITCH HAS THE BOMB"
    public TMP_Text fuseText;       // countdown seconds
    public Image timerIcon;         // ticking stopwatch icon above the fuse bar

    [Header("Result Banner")]
    public GameObject resultBanner; // legacy banner (superseded by leaderboard)
    public TMP_Text resultText;

    [Header("Bomb Animation")]
    public Sprite[] bombHeldFrames; // sliced bomb_held_08f sprite sheet (8 frames)
    public Sprite bombLitSprite;    // bomb_lit.png
    public Sprite explosionSprite;  // explosion.png

    [Header("Card Corners")]
    public float cornerInset = 60f;  // margin (in canvas ref units) from the screen edges
    public float cardScale = 1.8f;   // enlarge the small prefab so name/rank are readable

    [Header("Leaderboard / Restart")]
    public MatchLeaderboard leaderboardPrefab;
    public float restartDelay = 5f;
    public string matchSceneName = "Match";

    private RankedMatchManager match;
    private MatchLeaderboard leaderboard;
    private bool restarting;
    private readonly List<PlayerCard> cards = new List<PlayerCard>();
    private bool built;

    private HashSet<int> eliminatedPlayerIds = new HashSet<int>();

    private IEnumerator Start()
    {
        if (resultBanner != null) resultBanner.SetActive(false);

        // Wait for the match manager to come online and build its players.
        float timeout = 10f;
        while ((RankedMatchManager.Instance == null ||
                RankedMatchManager.Instance.allPlayers.Count == 0) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        match = RankedMatchManager.Instance;
        if (match == null)
        {
            Debug.LogWarning("FFAHudController: no RankedMatchManager found.");
            yield break;
        }

        match.OnMatchStateChanged += RefreshCards;
        match.OnMatchEnded += ShowResult;

        BuildCards();
        RefreshCards();
    }

    private void OnDestroy()
    {
        if (match != null)
        {
            match.OnMatchStateChanged -= RefreshCards;
            match.OnMatchEnded -= ShowResult;
        }
    }

    private void BuildCards()
    {
        if (built || cardPrefab == null || cardContainer == null) return;

        foreach (Transform child in cardContainer) Destroy(child.gameObject);
        cards.Clear();
        eliminatedPlayerIds.Clear();

        // Rivals get deterministic random cosmetics; local uses equipped + rank.
        var rivalRng = new System.Random(match.seed);
        var lib = CosmeticLibrary.Instance;

        // Local player first so "YOU" is always in the top-left corner; the rest
        // keep their original order. OrderByDescending is a stable sort.
        var ordered = match.allPlayers.OrderByDescending(p => p.isLocal).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            Player player = ordered[i];
            PlayerCard card = Instantiate(cardPrefab, cardContainer);
            card.gameObject.SetActive(true);

            CosmeticLoadout loadout = default;
            if (lib != null)
            {
                if (player.isLocal && ProfileManager.Instance != null)
                    loadout = lib.BuildLocalLoadout(ProfileManager.Instance.currentProfile);
                else
                    loadout = lib.BuildRandomLoadout(rivalRng);
            }

            card.Bind(player, loadout);
            // Anchor each card to the corner that matches its player's platform
            // character (driven by playerId), NOT its order in the list. This puts
            // the local player's card in the corner next to their own character.
            PositionCardInCorner(card, player.playerId);
            cards.Add(card);

            if (player.isEliminated)
            {
                eliminatedPlayerIds.Add(player.playerId);
            }
        }
        built = true;
    }

    // Anchors a card into the screen corner matching its player's slot, mirroring
    // the platform-character layout (Character_0..3):
    //   id 0 = top-left, id 1 = top-right, id 2 = bottom-left, id 3 = bottom-right.
    // CardContainer is a full-screen stretch rect, so these corners are the actual
    // screen corners. cornerInset keeps the card off the very edge.
    private void PositionCardInCorner(PlayerCard card, int playerId)
    {
        RectTransform rt = card.GetComponent<RectTransform>();
        if (rt == null) return;

        bool left = (playerId % 2) == 0;
        bool top = playerId < 2;

        Vector2 corner = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
        rt.anchorMin = corner;
        rt.anchorMax = corner;
        rt.pivot = corner;
        rt.localScale = Vector3.one * cardScale;
        rt.anchoredPosition = new Vector2(left ? cornerInset : -cornerInset,
                                          top ? -cornerInset : cornerInset);
    }

    private void RefreshCards()
    {
        if (!built) BuildCards();

        Player holder = match.currentBombHolder;
        foreach (var card in cards)
        {
            card.Refresh();
            bool isHolder = !match.isMatchComplete && card.BoundPlayer != null &&
                            card.BoundPlayer == holder && card.BoundPlayer.hasBomb;
            card.SetBombActive(isHolder, bombLitSprite);

            // Handle newly eliminated players
            if (card.BoundPlayer != null && card.BoundPlayer.isEliminated && !eliminatedPlayerIds.Contains(card.BoundPlayer.playerId))
            {
                int id = card.BoundPlayer.playerId;
                eliminatedPlayerIds.Add(id);

                // 1. Play Explosion animation on Card
                card.ExplodeCard(explosionSprite);

                // 2. Play Platform Characters elimination sequence
                if (FFAArenaCharacterController.Instance != null)
                {
                    FFAArenaCharacterController.Instance.PlayEliminationSequence(id, null);
                }
            }
        }

        // Update active bomb holder indicator on the platform
        if (FFAArenaCharacterController.Instance != null)
        {
            FFAArenaCharacterController.Instance.UpdateBombHolder(holder);
        }
    }

    private void Update()
    {
        if (match == null) return;

        Player holder = match.currentBombHolder;
        if (holderText != null)
            holderText.text = (holder != null && !match.isMatchComplete)
                ? $"{holder.playerName} HAS THE BOMB"
                : (match.isMatchComplete ? "" : "");

        float norm = match.CurrentFuseNormalized;
        if (fuseFill != null) fuseFill.fillAmount = norm;
        if (fuseText != null)
            fuseText.text = match.isMatchComplete ? "" : Mathf.CeilToInt(match.CurrentFuseRemaining) + "s";

        // Tint the fuse red as it runs low.
        if (fuseFill != null)
            fuseFill.color = Color.Lerp(new Color(1f, 0.25f, 0.2f), new Color(0.2f, 0.9f, 1f), norm);

        // Animate the stopwatch icon above the timing bar
        if (timerIcon != null)
        {
            if (match.isMatchComplete)
            {
                timerIcon.gameObject.SetActive(false);
            }
            else
            {
                timerIcon.gameObject.SetActive(true);

                // Speed up ticking frequency from 4Hz (when full) to 16Hz (when critical)
                float tickSpeed = Mathf.Lerp(16f, 4f, norm);
                float angle = Mathf.Sin(Time.time * tickSpeed) * 15f; // Oscillate ±15 degrees
                timerIcon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

                // Pulse scale (expand larger as time runs low)
                float basePulse = Mathf.PingPong(Time.time * (tickSpeed * 0.5f), 0.15f);
                float pulseScale = 1f + basePulse * (1f - norm);
                timerIcon.rectTransform.localScale = new Vector3(pulseScale, pulseScale, 1f);
            }
        }
    }

    private void ShowResult(Player winner)
    {
        // Stop any bomb animations and hide the legacy banner; the leaderboard
        // is now the single end-of-match screen.
        foreach (var card in cards) card.SetBombActive(false, null);
        if (resultBanner != null) resultBanner.SetActive(false);

        ShowLeaderboard();

        if (!restarting)
        {
            restarting = true;
            StartCoroutine(RestartAfterDelay());
        }
    }

    private void ShowLeaderboard()
    {
        if (leaderboardPrefab == null || match == null) return;

        var lib = CosmeticLibrary.Instance;
        var results = match.GetRankedResults();
        var entries = new List<LeaderboardEntry>();
        for (int i = 0; i < results.Count; i++)
        {
            Player p = results[i];
            entries.Add(new LeaderboardEntry
            {
                playerName = p.isLocal ? p.playerName + " (YOU)" : p.playerName,
                scoreLabel = "SOLVED: " + p.puzzlesSolved,
                placeLabel = PlaceLabel(i),
                // Top 3 get gold/silver/bronze (emblem ids 2/1/0).
                emblem = (lib != null && i < 3) ? lib.EmblemForRank(2 - i) : null
            });
        }

        if (leaderboard == null)
        {
            Canvas canvas = cardContainer != null
                ? cardContainer.GetComponentInParent<Canvas>()
                : FindAnyObjectByType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;
            leaderboard = Instantiate(leaderboardPrefab, parent);
            RectTransform rt = leaderboard.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
        leaderboard.Show("RESULTS", entries);
    }

    private static string PlaceLabel(int index)
    {
        switch (index)
        {
            case 0: return "1ST";
            case 1: return "2ND";
            case 2: return "3RD";
            default: return (index + 1) + "TH";
        }
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(matchSceneName);
    }
}
