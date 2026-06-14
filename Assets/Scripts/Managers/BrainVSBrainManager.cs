using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AttackType { Scramble, Freeze, InvertControls, Blur }
public enum DefenseType { Block, Rebound, Absorb, AutoSolve }

// Brain VS Brain (1v1) score-meter race.
//
// Both sides solve their OWN puzzle of the SAME type (different variations).
// Each solve fills that side's score meter and immediately spawns a fresh
// variation. Crossing an attack threshold (while in the lead) fires an attack
// on the opponent's live puzzle (Freeze blocks their input, Scramble rebuilds
// their board); when behind, a side also banks a defense that auto-consumes
// against the next incoming attack. First to fill the meter (targetScore) wins.
//
// This manager owns puzzle spawning/respawning for both sides. The opponent is
// driven by a bot that calls AutoSolveChunk on an interval.
public class BrainVSBrainManager : MonoBehaviour
{
    public static BrainVSBrainManager Instance;

    [Header("Scene")]
    public Canvas canvas;
    public List<GameObject> overlayPrefabs = new List<GameObject>();
    // Larger, wider-spread side overlays so the two puzzles are legible without
    // overlapping. At 0.8 scale a 600-wide overlay is 480px (240 half-width);
    // anchored at +/-300 they fit a 1080-wide canvas edge-to-edge with a gap.
    public Vector2 playerAnchor = new Vector2(-300f, -40f);
    public Vector2 opponentAnchor = new Vector2(300f, -40f);
    public float overlayScale = 0.8f;

    [Header("Matchmaking")]
    public bool opponentIsBot = true;   // false when a real opponent was matched
    public int realPlayers = 1;

    [Header("Match Settings")]
    public int targetScore = 1000;
    public int pointsPerSolve = 100;
    public int attackPointThreshold = 200;
    public float botSolveInterval = 1.1f;
    public float respawnDelay = 0.5f;
    public float puzzleFuseTime = 9999f; // BvB has no bomb fuse; race is by solve speed

    [Header("Match State")]
    public int playerPoints = 0;
    public int opponentPoints = 0;
    public int lastPlayerAttackCheckpoint = 0;
    public int lastOpponentAttackCheckpoint = 0;
    public bool isMatchActive = false;

    public List<DefenseType> playerDefenseInventory = new List<DefenseType>();
    public List<DefenseType> opponentDefenseInventory = new List<DefenseType>();

    [Header("Puzzles (live)")]
    public BasePuzzle playerPuzzle;
    public BasePuzzle opponentPuzzle;

    // ---- HUD-facing events ----
    public event Action OnScoreChanged;                 // refresh both meters
    public event Action<string, Color> OnNotification;  // attack/defense banners
    public event Action<bool> OnMatchEnded;             // playerWon

    public int matchSeed;
    private System.Random rng;
    private RankedArenaPuzzleType currentType;
    private GameObject playerOverlay;
    private GameObject opponentOverlay;
    private Coroutine opponentBotRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void InitializeMatch(int seed)
    {
        matchSeed = seed;
        rng = new System.Random(seed);
        playerPoints = 0;
        opponentPoints = 0;
        lastPlayerAttackCheckpoint = 0;
        lastOpponentAttackCheckpoint = 0;
        playerDefenseInventory.Clear();
        opponentDefenseInventory.Clear();
        isMatchActive = true;

        // Same puzzle type for both sides this match.
        currentType = RankedArena.PuzzleTypes[rng.Next(RankedArena.PuzzleTypes.Count)];

        if (AudioManager.Instance != null) AudioManager.Instance.PlayArenaMusic();

        SpawnSide(true);
        SpawnSide(false);
        OnScoreChanged?.Invoke();

        Debug.Log($"[BvB] Lobby: {realPlayers} real player(s), opponent is {(opponentIsBot ? "AI bot" : "real player")}.");
        Debug.Log($"Brain VS Brain started: type={currentType}, target={targetScore}.");
    }

    // ---- Per-side puzzle spawning ----

    private void SpawnSide(bool isPlayer)
    {
        ClearSide(isPlayer);
        if (canvas == null) { Debug.LogWarning("BvB: no canvas."); return; }

        GameObject prefab = ResolveOverlayPrefab(currentType);
        if (prefab == null) { Debug.LogWarning("BvB: no overlay prefab."); return; }

        GameObject overlay = Instantiate(prefab, canvas.transform);
        var rt = overlay.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = isPlayer ? playerAnchor : opponentAnchor;
            rt.localScale = Vector3.one * overlayScale;
        }

        var puzzle = overlay.GetComponentInChildren<BasePuzzle>(true);
        if (puzzle == null) { Destroy(overlay); return; }

        int points = isPlayer ? playerPoints : opponentPoints;
        int difficulty = 1 + points / 300;
        puzzle.puzzleTime = puzzleFuseTime;
        puzzle.InitializePuzzle(difficulty, rng.Next());
        puzzle.inputEnabled = isPlayer;
        if (isPlayer) puzzle.OnSolved += HandlePlayerSolved;
        else puzzle.OnSolved += HandleOpponentSolved;
        puzzle.StartPuzzle();

        if (isPlayer) { playerPuzzle = puzzle; playerOverlay = overlay; }
        else
        {
            opponentPuzzle = puzzle;
            opponentOverlay = overlay;
            if (opponentIsBot) opponentBotRoutine = StartCoroutine(OpponentBot(puzzle));
        }
    }

    private IEnumerator OpponentBot(BasePuzzle puzzle)
    {
        while (isMatchActive && puzzle != null && !puzzle.isSolved && !puzzle.isTimedOut)
        {
            yield return new WaitForSeconds(botSolveInterval);
            if (isMatchActive && puzzle != null) puzzle.AutoSolveChunk();
        }
    }

    private void HandlePlayerSolved(BasePuzzle p) { OnSideSolved(true); }
    private void HandleOpponentSolved(BasePuzzle p) { OnSideSolved(false); }

    private void OnSideSolved(bool isPlayer)
    {
        if (!isMatchActive) return;
        AddPoints(pointsPerSolve, isPlayer);
        if (!isMatchActive) return; // a win may have ended the match in AddPoints
        StartCoroutine(RespawnAfterDelay(isPlayer));
    }

    private IEnumerator RespawnAfterDelay(bool isPlayer)
    {
        yield return new WaitForSeconds(respawnDelay);
        if (isMatchActive) SpawnSide(isPlayer);
    }

    // ---- Scoring + attacks ----

    public void AddPoints(int points, bool isPlayer)
    {
        if (!isMatchActive) return;

        if (isPlayer) { playerPoints += points; CheckAttackCheckpoints(true); }
        else { opponentPoints += points; CheckAttackCheckpoints(false); }

        OnScoreChanged?.Invoke();

        if (playerPoints >= targetScore) { EndMatch(true); return; }
        if (opponentPoints >= targetScore) { EndMatch(false); return; }
    }

    private void CheckAttackCheckpoints(bool isPlayer)
    {
        int currentPoints = isPlayer ? playerPoints : opponentPoints;
        int lastCheckpoint = isPlayer ? lastPlayerAttackCheckpoint : lastOpponentAttackCheckpoint;

        if (currentPoints >= lastCheckpoint + attackPointThreshold)
        {
            if (isPlayer) lastPlayerAttackCheckpoint += attackPointThreshold;
            else lastOpponentAttackCheckpoint += attackPointThreshold;

            bool inLead = isPlayer ? (playerPoints >= opponentPoints) : (opponentPoints >= playerPoints);
            if (!inLead) AddRandomDefense(isPlayer);
            TriggerAttack(!isPlayer);
        }
    }

    private void TriggerAttack(bool targetIsPlayer)
    {
        AttackType type = (AttackType)rng.Next(0, System.Enum.GetValues(typeof(AttackType)).Length);

        List<DefenseType> inventory = targetIsPlayer ? playerDefenseInventory : opponentDefenseInventory;
        if (inventory.Count > 0)
        {
            DefenseType defense = inventory[rng.Next(0, inventory.Count)];
            inventory.Remove(defense);
            ProcessDefense(defense, targetIsPlayer, type);
        }
        else
        {
            ExecuteAttack(type, targetIsPlayer);
        }
    }

    private void AddRandomDefense(bool isPlayer)
    {
        DefenseType type = (DefenseType)rng.Next(0, System.Enum.GetValues(typeof(DefenseType)).Length);
        if (isPlayer) playerDefenseInventory.Add(type);
        else opponentDefenseInventory.Add(type);

        string who = isPlayer ? "YOU" : "OPPONENT";
        Color col = isPlayer ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.6f, 0.2f);
        OnNotification?.Invoke($"{who} GAINED {type} DEFENSE", col);
    }

    private void ProcessDefense(DefenseType defense, bool isPlayerDefending, AttackType incomingAttack)
    {
        BasePuzzle defPuzzle = isPlayerDefending ? playerPuzzle : opponentPuzzle;
        if (VFXManager.Instance != null && defPuzzle != null)
            VFXManager.Instance.PlayDefenseEffect(defPuzzle.transform.position);

        string who = isPlayerDefending ? "YOU" : "OPPONENT";
        Color col = isPlayerDefending ? new Color(0.4f, 1f, 0.6f) : new Color(1f, 0.6f, 0.2f);
        OnNotification?.Invoke($"{who} DEFENDED ({defense})", col);

        switch (defense)
        {
            case DefenseType.Block:
                break; // attack cancelled
            case DefenseType.Rebound:
                ExecuteAttack(incomingAttack, !isPlayerDefending);
                break;
            case DefenseType.Absorb:
                AddPoints(25, isPlayerDefending);
                break;
            case DefenseType.AutoSolve:
                if (defPuzzle != null) defPuzzle.AutoSolveChunk();
                break;
        }
    }

    private void ExecuteAttack(AttackType type, bool targetIsPlayer)
    {
        BasePuzzle targetPuzzle = targetIsPlayer ? playerPuzzle : opponentPuzzle;
        if (targetPuzzle != null) targetPuzzle.ApplyAttack(type);
        if (VFXManager.Instance != null && targetPuzzle != null)
            VFXManager.Instance.PlayAttackEffect(targetPuzzle.transform.position);

        // Banner is framed from the local player's perspective.
        string msg = targetIsPlayer ? $"OPPONENT HIT YOU: {type}!" : $"YOU HIT OPPONENT: {type}!";
        Color col = targetIsPlayer ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 0.9f, 1f);
        OnNotification?.Invoke(msg, col);
    }

    // Back-compat shim (older callers used OnPuzzleSolved(bool)).
    public void OnPuzzleSolved(bool isPlayer) => OnSideSolved(isPlayer);

    private void EndMatch(bool playerWon)
    {
        if (!isMatchActive) return;
        isMatchActive = false;
        StopBot();

        Debug.Log($"BvB Over! Winner: {(playerWon ? "YOU" : "OPPONENT")} ({playerPoints} vs {opponentPoints})");

        int baseXP = 50;
        int winXP = playerWon ? 100 : 20;
        int coins = playerWon ? 25 : 5;

        if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.AddXP(baseXP + winXP);
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.currentProfile.coins += coins;
            ProfileManager.Instance.AwardRp(playerWon ? 100 : 25, playerWon);
        }

        OnScoreChanged?.Invoke();
        OnMatchEnded?.Invoke(playerWon);
    }

    // ---- Helpers ----

    private GameObject ResolveOverlayPrefab(RankedArenaPuzzleType type)
    {
        string fragment = RankedArena.PrefabNameFragment(type);
        GameObject match = overlayPrefabs.FirstOrDefault(p => p != null && p.name.Contains(fragment));
        return match != null ? match : overlayPrefabs.FirstOrDefault(p => p != null);
    }

    private void ClearSide(bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerPuzzle != null) playerPuzzle.OnSolved -= HandlePlayerSolved;
            if (playerOverlay != null) Destroy(playerOverlay);
            playerOverlay = null; playerPuzzle = null;
        }
        else
        {
            StopBot();
            if (opponentPuzzle != null) opponentPuzzle.OnSolved -= HandleOpponentSolved;
            if (opponentOverlay != null) Destroy(opponentOverlay);
            opponentOverlay = null; opponentPuzzle = null;
        }
    }

    private void StopBot()
    {
        if (opponentBotRoutine != null) { StopCoroutine(opponentBotRoutine); opponentBotRoutine = null; }
    }
}
