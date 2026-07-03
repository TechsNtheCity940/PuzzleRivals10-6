using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Unity.AI.Assistant.Tests
{
    [InitializeOnLoad]
    internal static class MirrorMazeTest
    {
        private const string StateKey = "MirrorMazeTest.State";
        private const string ResultKey = "MirrorMazeTest.Result";

        static MirrorMazeTest()
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
                Debug.Log("[Test] Starting Mirror Maze Test...");
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
                    SessionState.SetFloat("MirrorMazeTest.StartTime", (float)EditorApplication.timeSinceStartup);
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

                float elapsed = (float)EditorApplication.timeSinceStartup - SessionState.GetFloat("MirrorMazeTest.StartTime", 0);
                
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

                    var bootstrap = GameObject.FindObjectOfType<MatchBootstrap>();
                    if (bootstrap != null)
                    {
                        var prefab = bootstrap.overlayPrefabs.Find(p => p != null && p.name.Contains("MirrorMaze"));
                        if (prefab != null)
                        {
                            Debug.Log("[Test] Found MirrorMaze prefab in bootstrap. Forcing it...");
                            bootstrap.overlayPrefabs = new List<GameObject> { prefab };
                            SessionState.SetString(StateKey, "InPlayMode_Solving");
                        }
                    }
                    
                    if (elapsed > 20f)
                    {
                        SessionState.SetString(ResultKey, "FAILURE: Timed out waiting for MatchBootstrap");
                        SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                    }
                }
                else if (state == "InPlayMode_Solving")
                {
                    var puzzleObj = GameObject.FindObjectOfType<MirrorMazePuzzle>();
                    if (puzzleObj != null)
                    {
                        if (puzzleObj.isSolved)
                        {
                            Debug.Log("[Test] Mirror Maze Solved!");
                            SessionState.SetString(ResultKey, "SUCCESS: Mirror Maze Solved");
                            SessionState.SetString(StateKey, "InPlayMode_Cleanup");
                        }
                        else
                        {
                            float lastStepTime = SessionState.GetFloat("MirrorMazeTest.LastStepTime", 0);
                            if (Time.time - lastStepTime > 0.3f)
                            {
                                Debug.Log("[Test] Auto-solving chunk...");
                                puzzleObj.AutoSolveChunk();
                                SessionState.SetFloat("MirrorMazeTest.LastStepTime", Time.time);
                            }
                        }
                    }
                    
                    if (elapsed > 60f)
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

        [MenuItem("Test/RunMirrorMaze")]
        public static void Run()
        {
            SessionState.SetString(StateKey, "Start");
            SessionState.EraseString(ResultKey);
            SessionState.SetFloat("MirrorMazeTest.LastStepTime", 0);
        }
    }
}