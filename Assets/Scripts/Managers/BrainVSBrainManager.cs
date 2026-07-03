using UnityEngine;
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
    public int attackPointThreshold = 100;
    public int lastPlayerAttackCheckpoint = 0;
    public int lastOpponentAttackCheckpoint = 0;

    public List<DefenseType> playerDefenseInventory = new List<DefenseType>();
    public List<DefenseType> opponentDefenseInventory = new List<DefenseType>();

    [Header("Match Settings")]
    public int matchSeed;
    private System.Random rng;

    public bool isMatchActive = false;

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

        if (UIManager.Instance != null)
        {
            // We'll need a specific UI for BrainVSBrain
            // UIManager.Instance.ShowBrainVSBrainUI();
        }
        
        Debug.Log("Brain VS Brain Match Initialized.");
    }

    public void AddPoints(int points, bool isPlayer)
    {
        if (!isMatchActive) return;

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

        // Update UI Live Meter
        if (BrainVSBrainUI.Instance != null) BrainVSBrainUI.Instance.UpdateScores(playerPoints, opponentPoints);
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

    public void OnPuzzleSolved(bool isPlayer)
    {
        if (!isMatchActive) return;
        
        Debug.Log($"{(isPlayer ? "Player" : "Opponent")} solved the puzzle!");
        EndMatch(isPlayer);
    }

    private void EndMatch(bool playerWon)
    {
        isMatchActive = false;
        Debug.Log($"Match Over! Winner: {(playerWon ? "Player" : "Opponent")}");
        
        int baseXP = 50;
        int winXP = playerWon ? 100 : 20;
        int coins = playerWon ? 25 : 5;

        if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.AddXP(baseXP + winXP);
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.currentProfile.coins += coins;
            if (playerWon) ProfileManager.Instance.currentProfile.totalWins++;
            else ProfileManager.Instance.currentProfile.totalLosses++;
            ProfileManager.Instance.SaveProfile();
        }

        List<MatchResult> results = new List<MatchResult>
        {
            new MatchResult { playerName = ProfileManager.Instance.currentProfile.playerName, score = playerPoints, isWinner = playerWon, xpGained = baseXP + winXP, coinsGained = coins },
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
    }

    if (BrainVSBrainUI.Instance != null)
    {
        string msg = targetIsPlayer ? $"Opponent Attack: {type}!" : $"Player Attack: {type}!";
        Color color = targetIsPlayer ? Color.red : Color.cyan;
        BrainVSBrainUI.Instance.ShowNotification(msg, color);
    }
}
}
