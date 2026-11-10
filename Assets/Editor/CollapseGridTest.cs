using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Unity.AI.Assistant.Tests
{
    [InitializeOnLoad]
    internal static class CollapseGridTest
    {
        private const string StateKey = "CollapseGridTest.State";
        private const string ResultKey = "CollapseGridTest.Result";

        static CollapseGridTest()
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
                Debug.Log("[Test] Starting Collapse Grid Test...");
                SessionState.SetString(StateKey, "EnteringPlayMode");
                EditorApplication.isPlaying = true;
                return;
            }

            if (state == "EnteringPlayMode")
            {
                if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                {
                    Debug.Log("[Test] Entered Play Mode. Loading scene...");
                    SessionState.SetString(StateKey, "InPlayMode_Loading");
                    SessionState.SetFloat("CollapseGridTest.StartTime", (float)EditorApplication.timeSinceStartup);
                }
                return;
            }

            if (state.StartsWith("InPlayMode"))
            {
                if (!EditorApplication.isPlaying)
                {
                    Debug.Log("[Test] Play Mode exited unexpectedly.");
                    SessionState.SetString(StateKey, "Done");
                    return;
                }

                float elapsed = (float)EditorApplication.timeSinceStartup - SessionState.GetFloat("CollapseGridTest.StartTime", 0);
                
                if (state == "InPlayMode_Loading")
                {
                    if (elapsed > 3f)
                    {
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Match")
                        {
                            UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
                        }
                        SessionState.SetString(StateKey, "InPlayMode_WaitingForMatch");
                    }
                }
                else if (state == "InPlayMode_WaitingForMatch")
                {
                    // Ensure GameState
                    var gsObj = GameObject.Find("GameState");
                    if (gsObj == null) {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameState.prefab");
                        if (prefab != null) {
                            gsObj = Object.Instantiate(prefab);
                            gsObj.name = "GameState";
                        }
                    }
                    
                    if (gsObj != null) {
                        var gs = gsObj.GetComponent("GameState");
                        if (gs != null) {
                            var field = gs.GetType().GetField("currentMode");
                            if (field != null) field.SetValue(gs, 1); // FFA
                        }
                    }

                    var rmm = GameObject.FindObjectOfType<RankedMatchManager>();
                    if (rmm != null)
                    {
                        var prefab = rmm.puzzlePrefabs.Find(p => p != null && p.name.Contains("CollapseGrid"));
                        if (prefab != null)
                        {
                            Debug.Log("[Test] Forcing CollapseGrid in RankedMatchManager");
                            rmm.puzzlePrefabs = new List<GameObject> { prefab };
                            
                            // Force immediate turn change if not solving CollapseGrid
                            var currentPuzzle = rmm.GetType().GetProperty("CurrentPuzzle")?.GetValue(rmm) as BasePuzzle;
                            if (currentPuzzle == null || !currentPuzzle.name.Contains("CollapseGrid"))
                            {
                                Debug.Log("[Test] Current puzzle is not CollapseGrid. Resetting...");
                                rmm.Invoke("ClearCurrentPuzzle", 0);
                                rmm.Invoke("NextTurn", 0);
                            }
                            else
                            {
                                SessionState.SetString(StateKey, "InPlayMode_Solving");
                            }
                        }
                    }
                    
                    if (elapsed > 20f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out waiting for MatchManager");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Solving")
                {
                    var puzzleObj = GameObject.FindObjectOfType<CollapseGridPuzzle>();
                    if (puzzleObj != null)
                    {
                        if (puzzleObj.isSolved)
                        {
                            Debug.Log("[Test] Collapse Grid Solved!");
                            SessionState.SetString(ResultKey, "SUCCESS: Collapse Grid Solved");
                            SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                        }
                        else if (puzzleObj.isTimedOut)
                        {
                            Debug.Log("[Test] Collapse Grid Timed Out/Collapsed!");
                            SessionState.SetString(ResultKey, "FAILURE: Structure Collapsed");
                            SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                        }
                        else
                        {
                            float lastStepTime = SessionState.GetFloat("CollapseGridTest.LastStepTime", 0);
                            if (Time.time - lastStepTime > 0.4f)
                            {
                                Debug.Log("[Test] Auto-solving chunk...");
                                puzzleObj.AutoSolveChunk();
                                SessionState.SetFloat("CollapseGridTest.LastStepTime", Time.time);
                            }
                        }
                    }
                    
                    if (elapsed > 80f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out solving");
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

        [MenuItem("Test/RunCollapseGrid")]
        public static void Run()
        {
            SessionState.SetString(StateKey, "Start");
            SessionState.EraseString(ResultKey);
            SessionState.SetFloat("CollapseGridTest.LastStepTime", 0);
        }
    }
}