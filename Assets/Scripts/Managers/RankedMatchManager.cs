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
    // Preserve eliminated players for HUD cards and final standings.
    public List<Player> allPlayers = new List<Player>();
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
    public Canvas matchCanvas;
    public GameObject bombPrefab;
    
    // Events for HUD
    public event System.Action OnMatchStateChanged;
    public event System.Action<Player> OnMatchEnded;

    private System.Random rng;
    private string _currentPuzzleKey;
    private BasePuzzle _activePuzzle;
    private System.Action<BasePuzzle> _activePuzzleSolved;
    private Coroutine _botSolveRoutine;

    // Shuffle-bag rotation: every ranked puzzle appears once before a new cycle.
    private List<RankedArenaPuzzleType> _puzzleBag = new List<RankedArenaPuzzleType>();
    private int _puzzleBagIndex;
    private bool _hasLastAssignedPuzzle;
    private RankedArenaPuzzleType _lastAssignedPuzzle;
    private readonly Dictionary<int, RankedArenaPuzzleType> _lastPuzzleByPlayer =
        new Dictionary<int, RankedArenaPuzzleType>();

    // Reference to the persistent Bomb for fuse tracking.
    private Bomb _bomb;

    private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        
            // Auto-populate puzzlePrefabs if empty (for runtime fallback)
            if (puzzlePrefabs == null || puzzlePrefabs.Count == 0)
            {
                AutoPopulatePuzzlePrefabs();
            }
        }
    
        private void AutoPopulatePuzzlePrefabs()
        {
            // Try to load from Resources folder first
            var resourcesPrefabs = Resources.LoadAll<GameObject>("Prefabs");
            if (resourcesPrefabs != null)
            {
                puzzlePrefabs = new List<GameObject>(resourcesPrefabs);
                Debug.Log($"[AutoPopulate] Loaded {puzzlePrefabs.Count} prefabs from Resources/Prefabs");
                return;
            }
        
            // Fallback: scan Assets/Prefabs folder at runtime (requires Editor or build-time setup)
            // This is a best-effort attempt - prefabs should be assigned in Editor Inspector
            if (puzzlePrefabs == null || puzzlePrefabs.Count == 0)
            {
                Debug.LogWarning("[AutoPopulate] No puzzle prefabs found. Please assign them in the RankedMatchManager Inspector.");
            }
        }

    public void InitializeMatch(List<Player> players, int matchSeed, List<int> humanSlots = null)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("RankedMatchManager: cannot start a match without players.");
            return;
        }
        Debug.Log($"Initializing Match with {players.Count} players and seed {matchSeed}.");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayArenaMusic();
        }

        allPlayers = new List<Player>(players);
        activePlayers = new List<Player>(players);
        seed = matchSeed;
        rng = new System.Random(seed);
        currentRoundIndex = 1;
        isMatchComplete = false;
        winner = null;
        currentSpeedMultiplier = 1f;
        _puzzleBag.Clear();
        _puzzleBagIndex = 0;
        _hasLastAssignedPuzzle = false;
        _lastPuzzleByPlayer.Clear();
        playerPuzzles.Clear();

        foreach (var p in activePlayers)
        {
            p.puzzlesSolved = 0;
            p.isEliminated = false;
            p.hasBomb = false;
            p.isBot = humanSlots == null ? !p.isLocal : !humanSlots.Contains(p.playerId);
        }

        PuzzlePool.EnsureInstance();
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
        
        // Assign distinct puzzles this round using a shuffled bag. This avoids
        // same-round duplicates and consecutive repeats for each player.
        playerPuzzles.Clear();
        var usedThisRound = new HashSet<RankedArenaPuzzleType>();
        foreach (var player in activePlayers)
        {
            var puzzleType = NextPuzzleType(player, usedThisRound);
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
            if (ProfileManager.Instance != null && ProfileManager.Instance.currentProfile != null)
            {
                UIManager.Instance.UpdatePlayerCard(ProfileManager.Instance.currentProfile);
            }
        }
        
        OnMatchStateChanged?.Invoke();
    }

    private RankedArenaPuzzleType NextPuzzleType(
        Player player,
        HashSet<RankedArenaPuzzleType> usedThisRound)
    {
        if (RankedArena.PuzzleTypes == null || RankedArena.PuzzleTypes.Count == 0)
            throw new System.InvalidOperationException("RankedArena has no puzzle types configured.");

        if (_puzzleBagIndex >= _puzzleBag.Count)
            RefillPuzzleBag();

        int candidate = FindNextPuzzleCandidate(player, usedThisRound, true, true);
        if (candidate < 0)
        {
            // Refill early while carrying unused entries forward. No puzzle is
            // discarded just because the current round already used it.
            RefillPuzzleBag();
            candidate = FindNextPuzzleCandidate(player, usedThisRound, true, true);
        }
        if (candidate < 0)
            candidate = FindNextPuzzleCandidate(player, usedThisRound, true, false);
        if (candidate < 0)
            candidate = FindNextPuzzleCandidate(player, usedThisRound, false, false);
        if (candidate < 0)
        {
            RefillPuzzleBag();
            candidate = _puzzleBagIndex;
        }

        SwapPuzzleBagItems(_puzzleBagIndex, candidate);
        RankedArenaPuzzleType selected = _puzzleBag[_puzzleBagIndex++];
        usedThisRound.Add(selected);
        if (player != null)
            _lastPuzzleByPlayer[player.playerId] = selected;
        _lastAssignedPuzzle = selected;
        _hasLastAssignedPuzzle = true;
        return selected;
    }

    private int FindNextPuzzleCandidate(
        Player player,
        HashSet<RankedArenaPuzzleType> usedThisRound,
        bool avoidPlayerRepeat,
        bool avoidGlobalRepeat)
    {
        RankedArenaPuzzleType lastForPlayer = default;
        bool hasPlayerHistory = player != null &&
            _lastPuzzleByPlayer.TryGetValue(player.playerId, out lastForPlayer);

        for (int i = _puzzleBagIndex; i < _puzzleBag.Count; i++)
        {
            RankedArenaPuzzleType candidate = _puzzleBag[i];
            if (usedThisRound.Contains(candidate)) continue;
            if (avoidPlayerRepeat && hasPlayerHistory && candidate == lastForPlayer) continue;
            if (avoidGlobalRepeat && _hasLastAssignedPuzzle && candidate == _lastAssignedPuzzle) continue;
            return i;
        }

        return -1;
    }

    private void RefillPuzzleBag()
    {
        if (rng == null) rng = new System.Random(seed);

        var carryOver = new List<RankedArenaPuzzleType>();
        for (int i = _puzzleBagIndex; i < _puzzleBag.Count; i++)
            carryOver.Add(_puzzleBag[i]);

        var nextBag = new List<RankedArenaPuzzleType>();
        foreach (RankedArenaPuzzleType type in RankedArena.PuzzleTypes)
        {
            if (!carryOver.Contains(type))
                nextBag.Add(type);
        }

        for (int i = nextBag.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            RankedArenaPuzzleType temp = nextBag[i];
            nextBag[i] = nextBag[swapIndex];
            nextBag[swapIndex] = temp;
        }

        if (_hasLastAssignedPuzzle && nextBag.Count > 1 &&
            nextBag[0] == _lastAssignedPuzzle)
        {
            int swapIndex = nextBag.FindIndex(1, type => type != _lastAssignedPuzzle);
            if (swapIndex > 0)
            {
                RankedArenaPuzzleType temp = nextBag[0];
                nextBag[0] = nextBag[swapIndex];
                nextBag[swapIndex] = temp;
            }
        }

        // Carry unconsumed entries forward when a round crosses the bag boundary,
        // so no puzzle type is silently dropped from the rotation.
        nextBag.AddRange(carryOver);
        _puzzleBag = nextBag;
        _puzzleBagIndex = 0;
    }

    private void SwapPuzzleBagItems(int a, int b)
    {
        if (a == b) return;
        RankedArenaPuzzleType temp = _puzzleBag[a];
        _puzzleBag[a] = _puzzleBag[b];
        _puzzleBag[b] = temp;
    }

    private void MoveToSolvingSpot(Player player)
    {
        // Player data class doesn't have physical movement - skip for now
        SetupBombForPlayer(player);
    }

    private void SetupBombForPlayer(Player player)
    {
        if (player == null || isMatchComplete) return;

        if (_bomb == null) _bomb = FindAnyObjectByType<Bomb>();
        if (_bomb == null && bombPrefab != null)
            _bomb = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity).GetComponent<Bomb>();

        if (_bomb == null)
        {
            Debug.LogError("RankedMatchManager: Bomb prefab is missing or has no Bomb component. Assign it on MatchBootstrap.");
            return;
        }

        _bomb.AttachToPlayer(player);
        player.hasBomb = true;
        _bomb.ResetTimer(currentSpeedMultiplier);
        _bomb.SetLit(true);
        StartPuzzleForPlayer(player);
    }

    private void StartPuzzleForPlayer(Player player)
    {
        if (player == null || isMatchComplete) return;
        RankedArenaPuzzleType pType = playerPuzzles[player.playerId];
        Debug.Log($"Player {player.playerName} starting puzzle: {pType}");

        ReturnCurrentPuzzle();
        PuzzlePool pool = PuzzlePool.EnsureInstance();
        string fragment = RankedArena.PrefabNameFragment(pType);
        GameObject prefab = puzzlePrefabs.Find(p => p != null &&
            p.name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0);
        if (prefab == null)
        {
            Debug.LogError($"RankedMatchManager: no prefab found for {pType} (expected '{fragment}').");
            return;
        }

        _currentPuzzleKey = prefab.name;
        Canvas targetCanvas = matchCanvas != null ? matchCanvas : FindAnyObjectByType<Canvas>();
        puzzleOverlay = pool.Get(_currentPuzzleKey, prefab, targetCanvas != null ? targetCanvas.transform : null);
        puzzleOverlay.SetActive(true);

        _activePuzzle = puzzleOverlay.GetComponentInChildren<BasePuzzle>(true);
        if (_activePuzzle == null)
        {
            Debug.LogError($"RankedMatchManager: prefab '{prefab.name}' has no BasePuzzle component.");
            ReturnCurrentPuzzle();
            return;
        }

        _activePuzzle.inputEnabled = player.isLocal;
        _activePuzzle.allowAutomatedInput = player.isBot;
        _activePuzzleSolved = _ => OnPuzzleSolved(player);
        _activePuzzle.OnSolved += _activePuzzleSolved;
        _activePuzzle.InitializePuzzle();
        _activePuzzle.StartPuzzle();

        if (player.isBot)
            _botSolveRoutine = StartCoroutine(AutoSolveBot(_activePuzzle));
    }

    private IEnumerator AutoSolveBot(BasePuzzle puzzle)
    {
        while (puzzle != null && !puzzle.isSolved && !puzzle.isTimedOut && !isMatchComplete)
        {
            puzzle.AutoSolveChunk();
            yield return new WaitForSeconds(0.35f);
        }
        _botSolveRoutine = null;
    }

    private void ReturnCurrentPuzzle()
    {
        if (_botSolveRoutine != null)
        {
            StopCoroutine(_botSolveRoutine);
            _botSolveRoutine = null;
        }

        if (_activePuzzle != null)
        {
            if (_activePuzzleSolved != null) _activePuzzle.OnSolved -= _activePuzzleSolved;
            _activePuzzle.StopPuzzle();
        }
        _activePuzzle = null;
        _activePuzzleSolved = null;

        if (puzzleOverlay != null)
        {
            PuzzlePool.EnsureInstance().Return(puzzleOverlay);
            puzzleOverlay = null;
        }
        _currentPuzzleKey = null;
    }

    public void OnPuzzleSolved(Player player)
    {
        if (isMatchComplete || player == null || player != currentBombHolder) return;

        Debug.Log($"Player {player.playerName} solved their puzzle!");
        player.hasBomb = false;
        ReturnCurrentPuzzle();

        // Increase speed for next player
        currentSpeedMultiplier += speedIncreasePerSolve;

        // Find next player in sequence
        int currentIndex = roundPlayerSequence.IndexOf(player);
        int nextIndex = (currentIndex + 1) % roundPlayerSequence.Count;
        Player nextPlayer = roundPlayerSequence[nextIndex];
        
        // Update solved count and transfer ownership before starting the next puzzle.
        player.puzzlesSolved++;
        currentBombHolder = nextPlayer;
        MoveToSolvingSpot(nextPlayer);
        OnMatchStateChanged?.Invoke();
    }

    public void OnBombExploded(Player holder)
    {
        if (isMatchComplete || holder == null || !activePlayers.Contains(holder)) return;

        Debug.Log($"Bomb exploded! Player {holder.playerName} is eliminated.");
        holder.isEliminated = true;
        holder.hasBomb = false;
        ReturnCurrentPuzzle();
        if (_bomb != null) _bomb.SetLit(false);
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
        
        OnMatchStateChanged?.Invoke();
    }

    private IEnumerator StartNextRoundAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        StartNewRound();
    }

    private void EndMatch()
    {
        if (isMatchComplete) return;
        isMatchComplete = true;
        ReturnCurrentPuzzle();
        
        // Cleanup bomb
        if (_bomb != null)
        {
            _bomb.SetLit(false);
        }
        
        List<MatchResult> results = new List<MatchResult>();

        if (activePlayers.Count == 1)
        {
            winner = activePlayers[0];
            Debug.Log($"Match Complete! Winner is {winner.playerName}");
        }

        var profile = ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile : null;
        if (profile != null)
        {
            bool isWinner = winner != null && winner.playerName == profile.playerName;
            int xp = isWinner ? 200 : 50;
            int coins = isWinner ? 100 : 20;

            if (SeasonPassManager.Instance != null) SeasonPassManager.Instance.AddXP(xp);
            profile.coins += coins;
            if (isWinner) profile.totalWins++;
            ProfileManager.Instance.SaveProfile();
            results.Add(new MatchResult { playerName = profile.playerName, score = 0, isWinner = isWinner, xpGained = xp, coinsGained = coins });
        }

        OnMatchEnded?.Invoke(winner);
        
        // Show scoreboard via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowScoreboard(results);
        }
    }
    
    // Fuse access for HUD
    public float CurrentFuseNormalized => _bomb != null ? _bomb.CurrentFuseNormalized : 0f;
    public float CurrentFuseRemaining => _bomb != null ? _bomb.CurrentFuseRemaining : 0f;
    
    // Results for leaderboard
    public List<Player> GetRankedResults()
    {
        var results = new List<Player>(allPlayers);
        // Sort by puzzles solved descending
        results.Sort((a, b) => b.puzzlesSolved.CompareTo(a.puzzlesSolved));
        return results;
    }

    // Results for leaderboard (as MatchResult list)
    public List<MatchResult> GetRankedMatchResults()
    {
        var results = new List<MatchResult>();
        foreach (var p in allPlayers)
        {
            results.Add(new MatchResult
            {
                playerName = p.playerName,
                score = p.puzzlesSolved,
                isWinner = p == winner,
                xpGained = 0,
                coinsGained = 0
            });
        }
        return results;
    }
    
    private void OnDestroy()
    {
        ReturnCurrentPuzzle();
        if (Instance == this) Instance = null;
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
                if (_bomb != null) _bomb.Explode();
            }
        }
    }
}