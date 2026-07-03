using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RankedMatchManager : MonoBehaviour
{
    public static RankedMatchManager Instance;

    [Header("Match Settings")]
    public int seed;
    public List<Player> activePlayers = new List<Player>();
    public List<Player> roundPlayerSequence = new List<Player>();
    public Dictionary<int, RankedArenaPuzzleType> playerPuzzles = new Dictionary<int, RankedArenaPuzzleType>();

    [Header("State")]
    public int currentRoundIndex = 1;
    public Player currentBombHolder;
    public bool isMatchComplete = false;
    public Player winner;
    public float currentSpeedMultiplier = 1.0f;
    public float speedIncreasePerSolve = 0.05f;

    [Header("Positions")]
    public Vector3 solvingSpot = new Vector3(0, -8, 0);

    [Header("UI")]
    public List<GameObject> puzzlePrefabs = new List<GameObject>();
    public GameObject puzzleOverlay;

    private System.Random rng;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeMatch(List<Player> players, int matchSeed)
    {
        Debug.Log($"Initializing Match with {players.Count} players and seed {matchSeed}.");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayArenaMusic();
        }

        activePlayers = new List<Player>(players);
seed = matchSeed;
        rng = new System.Random(seed);
        
        StartNewRound();
    }

    public void StartNewRound()
    {
        if (activePlayers.Count <= 1)
        {
            EndMatch();
            return;
        }

        Debug.Log($"Starting Round {currentRoundIndex} with {activePlayers.Count} players.");
        
        // Shuffle player order for this round
        roundPlayerSequence = activePlayers.OrderBy(x => rng.Next()).ToList();
        
        // Assign puzzles to each player for this round
        playerPuzzles.Clear();
        foreach (var player in activePlayers)
        {
            var puzzleType = RankedArena.PuzzleTypes[rng.Next(RankedArena.PuzzleTypes.Count)];
            playerPuzzles[player.playerId] = puzzleType;
        }

        // Reset speed for the round start, but scale with round index
        currentSpeedMultiplier = 1.0f + (currentRoundIndex - 1) * 0.2f;

        // First player in sequence starts with the bomb
        currentBombHolder = roundPlayerSequence[0];
        
        // Move current holder to center, then start puzzle
        MoveToSolvingSpot(currentBombHolder);
        
        // Trigger UI update
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRound(currentRoundIndex, activePlayers.Count);
            if (ProfileManager.Instance != null)
            {
                UIManager.Instance.UpdatePlayerCard(ProfileManager.Instance.currentProfile);
            }
        }
    }

    private void MoveToSolvingSpot(Player player)
    {
        player.SetMoveTarget(solvingSpot, () => {
            SetupBombForPlayer(player);
        });
    }

    private void SetupBombForPlayer(Player player)
    {
        Bomb bomb = FindAnyObjectByType<Bomb>();
        if (bomb != null)
        {
            bomb.AttachToPlayer(player);
            player.hasBomb = true;
            bomb.ResetTimer(currentSpeedMultiplier);
            
            // Start the puzzle for this player
            StartPuzzleForPlayer(player);
        }
    }

    private void StartPuzzleForPlayer(Player player)
    {
        RankedArenaPuzzleType pType = playerPuzzles[player.playerId];
        Debug.Log($"Player {player.playerName} starting puzzle: {pType}");

        if (puzzleOverlay != null) Destroy(puzzleOverlay);

        GameObject prefab = null;
        if (pType == RankedArenaPuzzleType.GridLogic)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Collapse"));
        else if (pType == RankedArenaPuzzleType.PatternRecall)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Mirror"));
        else if (pType == RankedArenaPuzzleType.PipePulse)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Pipe"));
        else if (pType == RankedArenaPuzzleType.ComboRush)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Combo"));
        else if (pType == RankedArenaPuzzleType.RuleBreaker)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Rule"));
        else if (pType == RankedArenaPuzzleType.LinkLock)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Link"));
        else if (pType == RankedArenaPuzzleType.MazeRush)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Maze"));
        else if (pType == RankedArenaPuzzleType.CircuitScramble)
            prefab = puzzlePrefabs.Find(p => p.name.Contains("Circuit"));

        if (prefab == null && puzzlePrefabs.Count > 0) prefab = puzzlePrefabs[0];

        if (prefab != null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            puzzleOverlay = Instantiate(prefab, canvas.transform);
            puzzleOverlay.SetActive(true);
            
            BasePuzzle puzzle = puzzleOverlay.GetComponentInChildren<BasePuzzle>();
            if (puzzle != null)
            {
                puzzle.InitializePuzzle();
                puzzle.StartPuzzle();
            }
        }
    }

    public void OnPuzzleSolved(Player player)
    {
        if (player != currentBombHolder) return;

        Debug.Log($"Player {player.playerName} solved their puzzle!");
        
        if (puzzleOverlay != null) Destroy(puzzleOverlay);

        // Increase speed for next player
        currentSpeedMultiplier += speedIncreasePerSolve;

        // Find next player in sequence
        int currentIndex = roundPlayerSequence.IndexOf(player);
        int nextIndex = (currentIndex + 1) % roundPlayerSequence.Count;
        Player nextPlayer = roundPlayerSequence[nextIndex];

        MoveToSolvingSpot(nextPlayer);
        currentBombHolder = nextPlayer;
    }

    public void OnBombExploded(Player holder)
{
        Debug.Log($"Bomb exploded! Player {holder.playerName} is eliminated.");
        holder.Eliminate();
        activePlayers.Remove(holder);
        
        currentRoundIndex++;
        
        if (activePlayers.Count > 1)
        {
            StartCoroutine(StartNextRoundAfterDelay());
        }
        else
        {
            EndMatch();
        }
    }

    private IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        StartNewRound();
    }

    private void EndMatch()
    {
        isMatchComplete = true;
        
        List<MatchResult> results = new List<MatchResult>();

        if (activePlayers.Count == 1)
        {
            winner = activePlayers[0];
            Debug.Log($"Match Complete! Winner is {winner.playerName}");
        }

        // We should really track all players who participated.
        // For simplicity, let's assume we have a list of all participants somewhere or just show current state.
        // Let's use activePlayers and assume everyone else was eliminated.
        
        // In a real match, we'd have a list of participants.
        // Assuming we have 'activePlayers' as the survivors and we can infer others.
        
        // Let's just create a simple result for the current player for now to demonstrate.
        if (ProfileManager.Instance != null)
        {
            bool isWinner = winner != null && winner.playerName == ProfileManager.Instance.currentProfile.playerName;
            int xp = isWinner ? 200 : 50;
            int coins = isWinner ? 100 : 20;
            
            SeasonPassManager.Instance.AddXP(xp);
            ProfileManager.Instance.currentProfile.coins += coins;
            if (isWinner) ProfileManager.Instance.currentProfile.totalWins++;
            ProfileManager.Instance.SaveProfile();

            results.Add(new MatchResult { playerName = ProfileManager.Instance.currentProfile.playerName, score = 0, isWinner = isWinner, xpGained = xp, coinsGained = coins });
        }

        if (MatchScoreboardUI.Instance != null) MatchScoreboardUI.Instance.Show(results);
    }
private void Update()
{
    if (UnityEngine.InputSystem.Keyboard.current != null)
    {
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (currentBombHolder != null) OnPuzzleSolved(currentBombHolder);
        }
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            Bomb bomb = FindAnyObjectByType<Bomb>();
            if (bomb != null) bomb.Explode();
        }
    }
}
}
