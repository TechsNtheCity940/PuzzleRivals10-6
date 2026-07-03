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
                    }
                }
                else if (state == "InPlayMode_WaitingForMatch")
                {
                    var gs = GameState.Instance;
                    if (gs == null) {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameState.prefab");
                        if (prefab != null) {
                            var go = Object.Instantiate(prefab);
                            go.name = "GameState";
                            gs = go.GetComponent<GameState>();
                        }
                    }
                    if (gs != null) gs.currentMode = GameState.Mode.FFA;

                    var rmm = Object.FindObjectOfType<RankedMatchManager>();
                    if (rmm != null)
                    {
                        SessionState.SetString(StateKey, "InPlayMode_Solving");
                    }
                    
                    if (elapsed > 30f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out waiting for MatchManager");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Solving")
                {
                    var rmm = Object.FindObjectOfType<RankedMatchManager>();
                    if (rmm != null && rmm.isMatchComplete)
                    {
                        Debug.Log("[Test] Match completed successfully!");
                        SessionState.SetString(ResultKey, "SUCCESS");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                        return;
                    }

                    var puzzleObj = Object.FindObjectOfType<BasePuzzle>();
                    if (puzzleObj != null)
                    {
                        Debug.Log($"[Test] Found Active Puzzle: {puzzleObj.puzzleName} ({puzzleObj.GetType().Name}) at localScale: {puzzleObj.transform.root.localScale}");
                        
                        float lastStepTime = SessionState.GetFloat("AllPuzzlesSolveTest.LastStepTime", 0);
                        if (Time.time - lastStepTime > 0.4f)
                        {
                            Debug.Log($"[Test] Auto-solving chunk for {puzzleObj.puzzleName}");
                            puzzleObj.AutoSolveChunk();
                            SessionState.SetFloat("AllPuzzlesSolveTest.LastStepTime", Time.time);
                        }
                    }
                    
                    if (elapsed > 120f)
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

        [MenuItem("Test/RunAllPuzzlesSolve")]
        public static void Run()
        {
            SessionState.SetString(StateKey, "Start");
            SessionState.EraseString(ResultKey);
            SessionState.SetFloat("AllPuzzlesSolveTest.LastStepTime", 0);
        }
    }
}