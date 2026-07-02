using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// 4-player Ranked Free-For-All, overlay based.
//
// Flow: a random player gets the "bomb" (the puzzle's fuse timer) and a random
// puzzle type+variation shown as a centre-screen overlay so everyone watches.
// Solve -> the bomb passes to another random alive player with a different
// random puzzle. The fuse shortens and puzzles get harder as solves accumulate.
// If the fuse runs out (puzzle OnTimedOut) the holder is eliminated. Last
// player standing wins. The local player plays manually; bots AutoSolveChunk.
public class RankedMatchManager : MonoBehaviour
{
    public static RankedMatchManager Instance;

    [Header("Match Settings")]
    public int seed;
    public int playerCount = 4;
    public float baseFuseTime = 22f;
    public float minFuseTime = 8f;
    public float speedIncreasePerSolve = 0.05f;

    [Header("Bot Tuning")]
    public float botSolveInterval = 0.9f;

    [Header("Overlay Prefabs (one per puzzle type)")]
    public List<GameObject> puzzleOverlayPrefabs = new List<GameObject>();
    public Canvas matchCanvas;

    [Header("Matchmaking")]
    public int realPlayers = 1;          // humans in the lobby (incl. local)
    public int botCount = 3;             // AI bots filling the rest
    public List<int> humanSlots;         // slot indices that are real humans

    [Header("State")]
    public List<Player> activePlayers = new List<Player>();
    public List<Player> allPlayers = new List<Player>();
    public int currentRoundIndex = 1;
    public Player currentBombHolder;
    public bool isMatchComplete = false;
    public Player winner;

    // ---- HUD-facing events ----
    // Fired whenever card-relevant state changes (players built, turn change,
    // solve, elimination, match end). The HUD rebuilds/refreshes on this.
    public event System.Action OnMatchStateChanged;
    // Fired when the bomb passes to a new holder (new puzzle started).
    public event System.Action<Player> OnTurnChanged;
    // Fired once when the match ends, with the winner (may be null).
    public event System.Action<Player> OnMatchEnded;

    // ---- HUD-facing read accessors ----
    public BasePuzzle CurrentPuzzle => currentPuzzle;
    public float CurrentFuseNormalized => currentPuzzle != null ? currentPuzzle.NormalizedTimeRemaining : 0f;
    public float CurrentFuseRemaining => currentPuzzle != null ? currentPuzzle.TimeRemaining : 0f;

    private System.Random rng;
    private BasePuzzle currentPuzzle;
    private GameObject currentOverlay;
    private int totalSolves;
    private float currentSpeedMultiplier = 1f;
    private Coroutine botRoutine;
    // Order players are eliminated (first out = index 0). Used to rank results.
    private readonly List<Player> eliminationOrder = new List<Player>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void InitializeMatch(int matchSeed)
    {
        seed = matchSeed;
        rng = new System.Random(seed);
        isMatchComplete = false;
        currentRoundIndex = 1;
        totalSolves = 0;
        currentSpeedMultiplier = 1f;
        eliminationOrder.Clear();

        BuildPlayers();
        activePlayers = new List<Player>(allPlayers);
        OnMatchStateChanged?.Invoke();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayArenaMusic();

        Debug.Log($"Ranked FFA started: {activePlayers.Count} players, seed {seed}.");
        NextTurn(PickRandom(activePlayers, null));
    }

    private void BuildPlayers()
    {
        allPlayers.Clear();
        int localIndex = (GameState.Instance != null) ? GameState.Instance.localPlayerIndex : 0;
        localIndex = Mathf.Clamp(localIndex, 0, playerCount - 1);

        // Real-human slots come from matchmaking; local is always human, and any
        // slot not flagged human is filled by an AI bot.
        var humans = (humanSlots != null && humanSlots.Count > 0) ? humanSlots : new List<int> { localIndex };

        for (int i = 0; i < playerCount; i++)
        {
            string name = (GameState.Instance != null && i < GameState.Instance.rivalNames.Length)
                ? GameState.Instance.rivalNames[i] : "RIVAL " + (i + 1);
            Color col = (GameState.Instance != null && i < GameState.Instance.rivalColors.Length)
                ? GameState.Instance.rivalColors[i] : Color.white;
            bool isHuman = i == localIndex || humans.Contains(i);
            var p = new Player(i, name, i == localIndex) { color = col, isBot = !isHuman };
            allPlayers.Add(p);
        }
        if (ProfileManager.Instance != null)
        {
            var local = allPlayers.FirstOrDefault(p => p.isLocal);
            if (local != null) local.playerName = ProfileManager.Instance.currentProfile.playerName;
        }
        realPlayers = allPlayers.Count(p => !p.isBot);
        botCount = playerCount - realPlayers;
        Debug.Log($"[FFA] Lobby: {realPlayers} real player(s), {botCount} AI bot(s).");
    }

    private void NextTurn(Player holder)
    {
        if (isMatchComplete) return;
        if (activePlayers.Count <= 1) { EndMatch(); return; }
        if (holder == null) holder = PickRandom(activePlayers, null);

        currentBombHolder = holder;
        foreach (var p in allPlayers) p.hasBomb = false;
        holder.hasBomb = true;

        SpawnPuzzleFor(holder);

        OnTurnChanged?.Invoke(holder);
        OnMatchStateChanged?.Invoke();
    }

    private void SpawnPuzzleFor(Player holder)
    {
        ClearCurrentPuzzle();

        RankedArenaPuzzleType type = RankedArena.PuzzleTypes[rng.Next(RankedArena.PuzzleTypes.Count)];
        GameObject prefab = ResolveOverlayPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"No overlay prefab found for {type}; skipping turn.");
            return;
        }

        Canvas canvas = EnsureCanvas();
        currentOverlay = Instantiate(prefab, canvas.transform);
        currentOverlay.transform.SetSiblingIndex(1);
        RectTransform rt = currentOverlay.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Single centered overlay: scale it up to fill ~80% of the screen
            // (capped so it never exceeds ~80% of the height, leaving room for HUD).
            OverlayFitter.Fit(rt, canvas, 0.80f, 0.80f);
            rt.anchoredPosition = Vector2.zero;
        }

        currentPuzzle = currentOverlay.GetComponentInChildren<BasePuzzle>();
        if (currentPuzzle == null)
        {
            Debug.LogWarning($"Overlay {prefab.name} has no BasePuzzle.");
            return;
        }

        int difficulty = 1 + totalSolves / Mathf.Max(1, activePlayers.Count);
        float fuse = Mathf.Max(minFuseTime, baseFuseTime / currentSpeedMultiplier);
        currentPuzzle.puzzleTime = fuse;
        currentPuzzle.OnSolved += HandleSolved;
        currentPuzzle.OnTimedOut += HandleTimedOut;
        currentPuzzle.InitializePuzzle(difficulty, rng.Next());
        currentPuzzle.inputEnabled = holder.isLocal;
        currentPuzzle.StartPuzzle();

        Debug.Log($"{holder.playerName} ({(holder.isLocal ? "YOU" : "BOT")}) is solving {type} | fuse {fuse:0.0}s | diff {difficulty}");

        if (holder.isBot)
            botRoutine = StartCoroutine(BotSolveRoutine());
    }

    private IEnumerator BotSolveRoutine()
    {
        // Bots advance the puzzle in chunks until solved or the fuse blows.
        while (currentPuzzle != null && !currentPuzzle.isSolved && !currentPuzzle.isTimedOut)
        {
            yield return new WaitForSeconds(botSolveInterval);
            if (currentPuzzle != null) currentPuzzle.AutoSolveChunk();
        }
    }

    private void HandleSolved(BasePuzzle puzzle)
    {
        if (puzzle != currentPuzzle) return;
        StopBot();

        totalSolves++;
        currentSpeedMultiplier += speedIncreasePerSolve;
        if (currentBombHolder != null) currentBombHolder.puzzlesSolved++;

        Debug.Log($"{currentBombHolder?.playerName} solved it! Total solves: {totalSolves}");

        // Pass the bomb to a different random alive player.
        Player next = PickRandom(activePlayers, currentBombHolder);
        StartCoroutine(PassBombAfterDelay(next, 0.8f));
    }

    private IEnumerator PassBombAfterDelay(Player next, float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearCurrentPuzzle();
        NextTurn(next);
    }

    private void HandleTimedOut(BasePuzzle puzzle)
    {
        if (puzzle != currentPuzzle) return;
        StopBot();

        Player eliminated = currentBombHolder;
        if (eliminated != null)
        {
            eliminated.isEliminated = true;
            activePlayers.Remove(eliminated);
            if (!eliminationOrder.Contains(eliminated)) eliminationOrder.Add(eliminated);
            Debug.Log($"💥 {eliminated.playerName} ran out of time and is ELIMINATED! ({activePlayers.Count} left)");
        }

        currentRoundIndex++;
        StartCoroutine(ContinueAfterExplosion());
    }

    private IEnumerator ContinueAfterExplosion()
    {
        yield return new WaitForSeconds(1.5f);
        ClearCurrentPuzzle();
        if (activePlayers.Count > 1) NextTurn(PickRandom(activePlayers, null));
        else EndMatch();
    }

    private void EndMatch()
    {
        if (isMatchComplete) return;
        isMatchComplete = true;
        StopBot();
        ClearCurrentPuzzle();

        winner = activePlayers.Count >= 1 ? activePlayers[0] : null;
        Debug.Log($"🏆 Ranked FFA complete. Winner: {(winner != null ? winner.playerName : "none")}");
        OnMatchStateChanged?.Invoke();
        OnMatchEnded?.Invoke(winner);

        bool localWon = winner != null && winner.isLocal;
        int xp = localWon ? 200 : 50;
        int coins = localWon ? 100 : 20;

        if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.AddXP(xp);
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.currentProfile.coins += coins;
            ProfileManager.Instance.AwardRp(localWon ? 100 : 25, localWon);
        }
    }

    // Returns all players ordered best-to-worst for the end-of-match leaderboard:
    // the winner first, then any other survivors (by puzzles solved), then the
    // eliminated players in reverse elimination order (last out ranks higher),
    // with puzzlesSolved as the tiebreak.
    public List<Player> GetRankedResults()
    {
        var results = new List<Player>();
        if (winner != null) results.Add(winner);

        foreach (var p in activePlayers
                     .Where(p => p != winner)
                     .OrderByDescending(p => p.puzzlesSolved))
            if (!results.Contains(p)) results.Add(p);

        for (int i = eliminationOrder.Count - 1; i >= 0; i--)
            if (!results.Contains(eliminationOrder[i])) results.Add(eliminationOrder[i]);

        foreach (var p in allPlayers.OrderByDescending(p => p.puzzlesSolved))
            if (!results.Contains(p)) results.Add(p);

        return results;
    }

    // ---- Helpers ----

    private Player PickRandom(List<Player> pool, Player exclude)
    {
        List<Player> candidates = pool.Where(p => p != exclude && !p.isEliminated).ToList();
        if (candidates.Count == 0) candidates = pool.Where(p => !p.isEliminated).ToList();
        if (candidates.Count == 0) return null;
        return candidates[rng.Next(candidates.Count)];
    }

    private GameObject ResolveOverlayPrefab(RankedArenaPuzzleType type)
    {
        string fragment = RankedArena.PrefabNameFragment(type);
        GameObject match = puzzleOverlayPrefabs.FirstOrDefault(p => p != null && p.name.Contains(fragment));
        if (match != null) return match;
        return puzzleOverlayPrefabs.FirstOrDefault(p => p != null);
    }

    private Canvas EnsureCanvas()
    {
        if (matchCanvas != null) return matchCanvas;
        matchCanvas = FindAnyObjectByType<Canvas>();
        if (matchCanvas == null)
        {
            var go = new GameObject("MatchCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            matchCanvas = go.GetComponent<Canvas>();
            matchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
        }
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return matchCanvas;
    }

    private void ClearCurrentPuzzle()
    {
        StopBot();
        if (currentPuzzle != null)
        {
            currentPuzzle.OnSolved -= HandleSolved;
            currentPuzzle.OnTimedOut -= HandleTimedOut;
        }
        if (currentOverlay != null) Destroy(currentOverlay);
        currentOverlay = null;
        currentPuzzle = null;
    }

    private void StopBot()
    {
        if (botRoutine != null) { StopCoroutine(botRoutine); botRoutine = null; }
    }
}
