using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.AI.Assistant.Tests
{
    [InitializeOnLoad]
    internal static class CircuitScrambleTest
    {
        private const string StateKey = "CircuitScrambleTest.State";
        private const string ResultKey = "CircuitScrambleTest.Result";

        static CircuitScrambleTest()
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
                Debug.Log("[Test] Starting Circuit Scramble Test...");
                SessionState.SetString(StateKey, "EnteringPlayMode");
                EditorApplication.isPlaying = true;
                return;
            }

            if (state == "EnteringPlayMode")
            {
                if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                {
                    SessionState.SetString(StateKey, "InPlayMode_Loading");
                    SessionState.SetFloat("CircuitScrambleTest.StartTime", (float)EditorApplication.timeSinceStartup);
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

                float elapsed = (float)EditorApplication.timeSinceStartup - SessionState.GetFloat("CircuitScrambleTest.StartTime", 0);
                
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
                    // Setup GameState
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
                        var prefab = rmm.puzzleOverlayPrefabs.Find(p => p != null && p.name.Contains("CircuitScramble"));
                        if (prefab != null)
                        {
                            Debug.Log("[Test] Forcing CircuitScramble in MatchManager");
                            rmm.puzzleOverlayPrefabs = new List<GameObject> { prefab };
                            
                            var puzzleObj = Object.FindObjectOfType<CircuitScramblePuzzle>();
                            if (puzzleObj == null) {
                                var clearMethod = typeof(RankedMatchManager).GetMethod("ClearCurrentPuzzle", BindingFlags.NonPublic | BindingFlags.Instance);
                                var nextMethod = typeof(RankedMatchManager).GetMethod("NextTurn", BindingFlags.NonPublic | BindingFlags.Instance);
                                
                                clearMethod?.Invoke(rmm, null);
                                nextMethod?.Invoke(rmm, new object[] { null });
                            } else {
                                SessionState.SetString(StateKey, "InPlayMode_Solving");
                            }
                        }
                    }
                    
                    if (elapsed > 30f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out waiting for MatchManager");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Solving")
                {
                    var puzzleObj = Object.FindObjectOfType<CircuitScramblePuzzle>();
                    if (puzzleObj != null)
                    {
                        if (puzzleObj.isSolved)
                        {
                            Debug.Log("[Test] Circuit Scramble Solved!");
                            SessionState.SetString(ResultKey, "SUCCESS");
                            SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                        }
                        else
                        {
                            float lastStepTime = SessionState.GetFloat("CircuitScrambleTest.LastStepTime", 0);
                            if (Time.time - lastStepTime > 0.4f)
                            {
                                puzzleObj.AutoSolveChunk();
                                SessionState.SetFloat("CircuitScrambleTest.LastStepTime", Time.time);
                            }
                        }
                    }
                    
                    if (elapsed > 120f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Solvability timeout");
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

        [MenuItem("Test/RunCircuitScramble")]
        public static void Run()
        {
            SessionState.SetString(StateKey, "Start");
            SessionState.EraseString(ResultKey);
            SessionState.SetFloat("CircuitScrambleTest.LastStepTime", 0);
        }
    }
}