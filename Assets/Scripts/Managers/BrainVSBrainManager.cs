using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AttackType { Scramble, Freeze, InvertControls, Blur }
public enum DefenseType { Block, Rebound, Absorb, AutoSolve }

public class BrainVSBrainManager : MonoBehaviour
{
    public static BrainVSBrainManager Instance;

    [Header("Match State")]
    public int playerPoints = 0;
    public int opponentPoints = 0;
    public int targetScore = 1000;
    public int attackPointThreshold = 100;
    public int lastPlayerAttackCheckpoint = 0;
    public int lastOpponentAttackCheckpoint = 0;

    public List<DefenseType> playerDefenseInventory = new List<DefenseType>();
    public List<DefenseType> opponentDefenseInventory = new List<DefenseType>();

    [Header("Match Settings")]
    public int matchSeed;
    private System.Random rng;

    public bool isMatchActive = false;
    
    // UI references
    public Canvas canvas;
    public List<GameObject> overlayPrefabs = new List<GameObject>();
    public float botSolveInterval = 1.5f;
    public int realPlayers = 1;
    public bool opponentIsBot = true;

    // Events for HUD
    public event System.Action OnScoreChanged;
    public event System.Action<string, Color> OnNotification;
    public event System.Action<bool> OnMatchEnded;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("BrainVSBrainManager: no Canvas is available for the two puzzle boards.");
            isMatchActive = false;
            return;
        }

        CreatePuzzleRoots();
        SpawnPuzzle(true);
        SpawnPuzzle(false);
        Debug.Log("Brain VS Brain Match Initialized.");
    }

    public void AddPoints(int points, bool isPlayer)
    {
        if (!isMatchActive || points <= 0) return;
        if (rng == null) rng = new System.Random(matchSeed);

        if (isPlayer)
        {
            playerPoints += points;
            CheckAttackCheckpoints(true);
        }
        else
        {
            opponentPoints += points;
            CheckAttackCheckpoints(false);
        }

        // Update UI Live Meter.
        if (BrainVSBrainUI.Instance != null) BrainVSBrainUI.Instance.UpdateScores(playerPoints, opponentPoints);
        OnScoreChanged?.Invoke();

        if (playerPoints >= targetScore)
        {
            EndMatch(true);
            return;
        }
        if (opponentPoints >= targetScore)
        {
            EndMatch(false);
            return;
        }
    }

    private void CheckAttackCheckpoints(bool isPlayer)
    {
        int currentPoints = isPlayer ? playerPoints : opponentPoints;
        int lastCheckpoint = isPlayer ? lastPlayerAttackCheckpoint : lastOpponentAttackCheckpoint;

        if (attackPointThreshold <= 0) return;
        if (currentPoints >= lastCheckpoint + attackPointThreshold)
        {
            if (isPlayer) lastPlayerAttackCheckpoint += attackPointThreshold;
            else lastOpponentAttackCheckpoint += attackPointThreshold;

            bool inLead = isPlayer ? (playerPoints >= opponentPoints) : (opponentPoints >= playerPoints);

            if (inLead)
            {
                // Leading player only produces attacks
                TriggerAttack(!isPlayer); // Attack the other player
            }
            else
            {
                // Losing player produces defense AND attack
                AddRandomDefense(isPlayer);
                TriggerAttack(!isPlayer);
            }
        }
    }

    private void TriggerAttack(bool targetIsPlayer)
    {
        AttackType type = (AttackType)rng.Next(0, System.Enum.GetValues(typeof(AttackType)).Length);
        
        // Check for defense
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
        
        Debug.Log($"{(isPlayer ? "Player" : "Opponent")} earned defense: {type}");
    }

    public BasePuzzle playerPuzzle;
    public BasePuzzle opponentPuzzle;

    private RectTransform playerPuzzleRoot;
    private RectTransform opponentPuzzleRoot;
    private GameObject playerPuzzleObject;
    private GameObject opponentPuzzleObject;
    private Coroutine opponentBotRoutine;

    public void OnPuzzleSolved(bool isPlayer)
    {
        if (!isMatchActive) return;

        Debug.Log($"{(isPlayer ? "Player" : "Opponent")} solved the puzzle!");
        if (isPlayer && playerPuzzle != null) playerPuzzle.StopPuzzle();
        if (!isPlayer && opponentPuzzle != null) opponentPuzzle.StopPuzzle();

        AddPoints(100, isPlayer);
        if (isMatchActive)
            StartCoroutine(RespawnPuzzleAfterDelay(isPlayer));
    }

    private IEnumerator RespawnPuzzleAfterDelay(bool isPlayer)
    {
        yield return new WaitForSeconds(0.35f);
        if (isMatchActive) SpawnPuzzle(isPlayer);
    }

    private void CreatePuzzleRoots()
    {
        playerPuzzleRoot = CreatePuzzleRoot("PlayerPuzzleViewport", new Vector2(0f, 0f), new Vector2(0.5f, 1f));
        opponentPuzzleRoot = CreatePuzzleRoot("OpponentPuzzleViewport", new Vector2(0.5f, 0f), new Vector2(1f, 1f));
    }

    private RectTransform CreatePuzzleRoot(string rootName, Vector2 anchorMin, Vector2 anchorMax)
    {
        var root = new GameObject(rootName, typeof(RectTransform), typeof(RectMask2D), typeof(CanvasGroup));
        root.transform.SetParent(canvas.transform, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private void SpawnPuzzle(bool isPlayer)
    {
        if (!isMatchActive) return;
        RectTransform parent = isPlayer ? playerPuzzleRoot : opponentPuzzleRoot;
        if (parent == null) return;

        GameObject oldObject = isPlayer ? playerPuzzleObject : opponentPuzzleObject;
        if (oldObject != null) Destroy(oldObject);

        var candidates = new List<GameObject>();
        foreach (var prefab in overlayPrefabs)
            if (prefab != null && prefab.GetComponentInChildren<BasePuzzle>(true) != null)
                candidates.Add(prefab);

        if (candidates.Count == 0)
        {
            Debug.LogError("BrainVSBrainManager: overlayPrefabs contains no BasePuzzle prefabs.");
            return;
        }

        GameObject instance = Instantiate(candidates[rng.Next(candidates.Count)], parent, false);
        var rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        BasePuzzle puzzle = instance.GetComponentInChildren<BasePuzzle>(true);
        if (puzzle == null)
        {
            Destroy(instance);
            Debug.LogError("BrainVSBrainManager: selected puzzle prefab has no BasePuzzle component.");
            return;
        }

        puzzle.inputEnabled = isPlayer || !opponentIsBot;
        puzzle.allowAutomatedInput = !isPlayer && opponentIsBot;
        puzzle.OnSolved += _ => OnPuzzleSolved(isPlayer);
        int difficulty = Mathf.Clamp(1 + (playerPoints + opponentPoints) / 600, 1, 5);
        puzzle.InitializePuzzle(difficulty, rng.Next());
        puzzle.StartPuzzle();

        var group = parent.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.interactable = isPlayer || !opponentIsBot;
            group.blocksRaycasts = isPlayer || !opponentIsBot;
        }

        if (isPlayer)
        {
            playerPuzzleObject = instance;
            playerPuzzle = puzzle;
        }
        else
        {
            opponentPuzzleObject = instance;
            opponentPuzzle = puzzle;
            if (opponentIsBot) opponentBotRoutine = StartCoroutine(AutoSolveOpponent(puzzle));
        }
    }

    private IEnumerator AutoSolveOpponent(BasePuzzle puzzle)
    {
        while (isMatchActive && puzzle != null && !puzzle.isSolved && !puzzle.isTimedOut)
        {
            puzzle.AutoSolveChunk();
            yield return new WaitForSeconds(Mathf.Max(0.2f, botSolveInterval));
        }
        opponentBotRoutine = null;
    }

    private void OnDestroy()
    {
        if (opponentBotRoutine != null) StopCoroutine(opponentBotRoutine);
        if (Instance == this) Instance = null;
    }

    private void EndMatch(bool playerWon)
    {
        isMatchActive = false;
        if (opponentBotRoutine != null)
        {
            StopCoroutine(opponentBotRoutine);
            opponentBotRoutine = null;
        }
        if (playerPuzzle != null) playerPuzzle.StopPuzzle();
        if (opponentPuzzle != null) opponentPuzzle.StopPuzzle();
        Debug.Log($"Match Over! Winner: {(playerWon ? "Player" : "Opponent")}");
        
        int baseXP = 50;
        int winXP = playerWon ? 100 : 20;
        int coins = playerWon ? 25 : 5;

        if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.AddXP(baseXP + winXP);
        if (ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.coins += coins;
            if (playerWon) ProfileManager.Instance.currentProfile.totalWins++;
            else ProfileManager.Instance.currentProfile.totalLosses++;
            ProfileManager.Instance.SaveProfile();
        }

        OnMatchEnded?.Invoke(playerWon);

        string localName = ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null
            ? ProfileManager.Instance.currentProfile.playerName : "YOU";
        List<MatchResult> results = new List<MatchResult>
        {
            new MatchResult { playerName = localName, score = playerPoints, isWinner = playerWon, xpGained = baseXP + winXP, coinsGained = coins },
            new MatchResult { playerName = "Opponent", score = opponentPoints, isWinner = !playerWon, xpGained = 0, coinsGained = 0 }
        };

        if (MatchScoreboardUI.Instance != null) MatchScoreboardUI.Instance.Show(results);
    }

    private void ProcessDefense(DefenseType defense, bool isPlayerDefending, AttackType incomingAttack)
    {
        Debug.Log($"{(isPlayerDefending ? "Player" : "Opponent")} used defense: {defense} against {incomingAttack}");
        
        if (VFXManager.Instance != null)
        {
            BasePuzzle defPuzzle = isPlayerDefending ? playerPuzzle : opponentPuzzle;
            if (defPuzzle != null) VFXManager.Instance.PlayDefenseEffect(defPuzzle.transform.position);
        }

        if (BrainVSBrainUI.Instance != null)
        {
            string msg = isPlayerDefending ? $"Player Defense: {defense}!" : $"Opponent Defense: {defense}!";
            Color color = isPlayerDefending ? Color.green : new Color(1, 0.5f, 0); // Orange
            BrainVSBrainUI.Instance.ShowNotification(msg, color);
        }

        switch (defense)
        {
            case DefenseType.Block:
                // Attack cancelled
                break;
            case DefenseType.Rebound:
                // Send same attack back to attacker
                ExecuteAttack(incomingAttack, !isPlayerDefending);
                break;
            case DefenseType.Absorb:
                // Convert to points
                AddPoints(25, isPlayerDefending);
                break;
            case DefenseType.AutoSolve:
                BasePuzzle puzzle = isPlayerDefending ? playerPuzzle : opponentPuzzle;
                if (puzzle != null) puzzle.AutoSolveChunk();
                break;
        }
    }
    
    private void ExecuteAttack(AttackType type, bool targetIsPlayer)
        {
            Debug.Log($"Executing Attack: {type} on {(targetIsPlayer ? "Player" : "Opponent")}");
        
            BasePuzzle targetPuzzle = targetIsPlayer ? playerPuzzle : opponentPuzzle;
            if (targetPuzzle != null)
            {
                targetPuzzle.ApplyAttack(type);
                // Play attack VFX at target position
                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.PlayAttackEffect(targetPuzzle.transform.position);
                }
            }
        
            string msg = targetIsPlayer ? $"Opponent Attack: {type}!" : $"Player Attack: {type}!";
            Color color = targetIsPlayer ? Color.red : Color.cyan;
            if (BrainVSBrainUI.Instance != null)
            {
                BrainVSBrainUI.Instance.ShowNotification(msg, color);
            }
        
            OnNotification?.Invoke(msg, color);
        }
}