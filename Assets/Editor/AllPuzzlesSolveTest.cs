using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.AI.Assistant.Tests
{
    [InitializeOnLoad]
    internal static class AllPuzzlesSolveTest
    {
        private const string StateKey = "AllPuzzlesSolveTest.State";
        private const string ResultKey = "AllPuzzlesSolveTest.Result";
        private static readonly HashSet<RankedArenaPuzzleType> CoveredTypes = new HashSet<RankedArenaPuzzleType>();
        private static readonly Dictionary<int, RankedArenaPuzzleType> RoundTypes = new Dictionary<int, RankedArenaPuzzleType>();
        private static readonly Dictionary<int, int> RoundStartSolvedCounts = new Dictionary<int, int>();
        private static int TrackedRoundIndex = -1;

        static AllPuzzlesSolveTest()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            if (state == "Idle") return;

            if (state == "Start")
            {
                Debug.Log("[Test] Starting All Puzzles Solve Test...");
                SessionState.SetString(StateKey, "EnteringPlayMode");
                EditorApplication.isPlaying = true;
                return;
            }

            if (state == "EnteringPlayMode")
            {
                if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                {
                    SessionState.SetString(StateKey, "InPlayMode_Loading");
                    SessionState.SetFloat("AllPuzzlesSolveTest.StartTime", (float)EditorApplication.timeSinceStartup);
                    Debug.Log("[Test] Entered Play Mode; preparing Match scene.");
                }
                return;
            }

            if (state.StartsWith("InPlayMode"))
            {
                if (!EditorApplication.isPlaying)
                {
                    SessionState.SetString(StateKey, "Done");
                    return;
                }

                float elapsed = (float)EditorApplication.timeSinceStartup - SessionState.GetFloat("AllPuzzlesSolveTest.StartTime", 0);
                
                if (state == "InPlayMode_Loading")
                {
                    if (elapsed > 2f)
                    {
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Match")
                            UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
                        SessionState.SetString(StateKey, "InPlayMode_WaitingForMatch");
                        Debug.Log("[Test] Match scene loaded; waiting for FFA manager.");
                    }
                }
                else if (state == "InPlayMode_WaitingForMatch")
                {
                    var gs = GameState.Instance;
                    if (gs == null)
                    {
                        // GameState has no prefab in this project. Create the singleton
                        // at runtime so Awake initializes Instance and persists it.
                        var go = new GameObject("GameState");
                        gs = go.AddComponent<GameState>();
                    }
                    gs.currentMode = GameState.Mode.FFA;

                    var rmm = Object.FindAnyObjectByType<RankedMatchManager>();
                    if (rmm != null)
                    {
                        SessionState.SetString(StateKey, "InPlayMode_Solving");
                        Debug.Log($"[Test] FFA manager ready with {rmm.activePlayers.Count} active players.");
                    }
                    
                    if (elapsed > 30f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out waiting for MatchManager");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Solving")
                {
                    var rmm = Object.FindAnyObjectByType<RankedMatchManager>();
                    if (rmm != null)
                    {
                        if (TrackedRoundIndex < 0)
                        {
                            CaptureRound(rmm);
                        }
                        else if (rmm.currentRoundIndex != TrackedRoundIndex)
                        {
                            FinalizeRound(rmm);
                            if (CoveredTypes.Count >= RankedArena.PuzzleTypes.Count)
                            {
                                Debug.Log($"[Test] All {CoveredTypes.Count}/{RankedArena.PuzzleTypes.Count} ranked puzzle types solved successfully.");
                                SessionState.SetString(ResultKey, "SUCCESS");
                                SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                                return;
                            }

                            CaptureRound(rmm);
                        }

                        if (rmm.isMatchComplete)
                        {
                            FinalizeRound(rmm);
                            bool passed = CoveredTypes.Count >= RankedArena.PuzzleTypes.Count;
                            Debug.Log($"[Test] Match ended with puzzle coverage {CoveredTypes.Count}/{RankedArena.PuzzleTypes.Count}.");
                            SessionState.SetString(ResultKey, passed ? "SUCCESS" : $"FAILURE: Only {CoveredTypes.Count}/{RankedArena.PuzzleTypes.Count} puzzle types were solved.");
                            SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                            return;
                        }
                    }

                    BasePuzzle puzzleObj = null;
                    BasePuzzle[] activePuzzles = Object.FindObjectsByType<BasePuzzle>(FindObjectsInactive.Exclude);
                    foreach (var candidate in activePuzzles)
                    {
                        if (candidate != null && candidate.IsRunning && !candidate.isSolved && !candidate.isTimedOut)
                        {
                            puzzleObj = candidate;
                            break;
                        }
                    }

                    if (puzzleObj != null)
                    {
                        int instanceId = puzzleObj.GetInstanceID();
                        int lastLoggedId = SessionState.GetInt("AllPuzzlesSolveTest.LastPuzzleId", 0);
                        if (instanceId != lastLoggedId)
                        {
                            Debug.Log($"[Test] Found Active Puzzle: {puzzleObj.puzzleName} ({puzzleObj.GetType().Name}) at localScale: {puzzleObj.transform.root.localScale}");
                            SessionState.SetInt("AllPuzzlesSolveTest.LastPuzzleId", instanceId);
                        }

                        // Editor time is unscaled and continues even if gameplay time is paused.
                        float now = (float)EditorApplication.timeSinceStartup;
                        float lastStepTime = SessionState.GetFloat("AllPuzzlesSolveTest.LastStepTime", 0f);
                        if (now - lastStepTime > 0.4f)
                        {
                            Debug.Log($"[Test] Auto-solving chunk for {puzzleObj.puzzleName}");
                            puzzleObj.AutoSolveChunk();
                            SessionState.SetFloat("AllPuzzlesSolveTest.LastStepTime", now);
                        }
                    }
                    
                    if (elapsed > 160f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Solvability/Simulation timeout");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Cleanup")
                {
                    EditorApplication.isPlaying = false;
                    SessionState.SetString(StateKey, "Done");
                }
            }
        }

        private static void CaptureRound(RankedMatchManager rmm)
        {
            TrackedRoundIndex = rmm.currentRoundIndex;
            RoundTypes.Clear();
            RoundStartSolvedCounts.Clear();

            if (rmm.playerPuzzles == null) return;
            foreach (var assignment in rmm.playerPuzzles)
            {
                RoundTypes[assignment.Key] = assignment.Value;
                int solvedCount = 0;
                if (rmm.allPlayers != null)
                {
                    foreach (var player in rmm.allPlayers)
                    {
                        if (player != null && player.playerId == assignment.Key)
                        {
                            solvedCount = player.puzzlesSolved;
                            break;
                        }
                    }
                }
                RoundStartSolvedCounts[assignment.Key] = solvedCount;
            }

            Debug.Log($"[Test] Tracking round {TrackedRoundIndex} with {RoundTypes.Count} puzzle assignments.");
        }

        private static void FinalizeRound(RankedMatchManager rmm)
        {
            foreach (var assignment in RoundTypes)
            {
                Player player = null;
                if (rmm.allPlayers != null)
                {
                    foreach (var candidate in rmm.allPlayers)
                    {
                        if (candidate != null && candidate.playerId == assignment.Key)
                        {
                            player = candidate;
                            break;
                        }
                    }
                }

                int startingCount = 0;
                RoundStartSolvedCounts.TryGetValue(assignment.Key, out startingCount);
                if (player != null && player.puzzlesSolved > startingCount)
                {
                    if (CoveredTypes.Add(assignment.Value))
                        Debug.Log($"[Test] Verified puzzle type: {assignment.Value} ({CoveredTypes.Count}/{RankedArena.PuzzleTypes.Count}).");
                }
            }
        }

        [MenuItem("Test/RunAllPuzzlesSolve")]
        public static void Run()
        {
            CoveredTypes.Clear();
            RoundTypes.Clear();
            RoundStartSolvedCounts.Clear();
            TrackedRoundIndex = -1;
            UnityEngine.Random.InitState(20261030);

            SessionState.SetString(StateKey, "Start");
            SessionState.EraseString(ResultKey);
            SessionState.SetFloat("AllPuzzlesSolveTest.LastStepTime", (float)EditorApplication.timeSinceStartup);
            SessionState.SetInt("AllPuzzlesSolveTest.LastPuzzleId", 0);
        }
    }
}