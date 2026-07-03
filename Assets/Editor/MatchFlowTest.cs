// Play Mode test: open Match.unity, enter play mode, confirm FFA boots a puzzle
// overlay with no runtime errors, then exit and self-delete.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
internal static class MatchFlowTest
{
    const string StateKey = "MatchFlow.State";
    const string ResultKey = "MatchFlow.Result";
    const string ScriptPath = "Assets/Editor/MatchFlowTest.cs";
    const string Sentinel = "MATCH_FLOW_DONE";

    static int frames;
    static double startTime;
    static bool validated;
    static readonly List<string> errors = new List<string>();

    static MatchFlowTest()
    {
        switch (SessionState.GetString(StateKey, "Idle"))
        {
            case "Launch":
                SessionState.SetString(StateKey, "Running");
                EditorApplication.delayCall += () =>
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/Match.unity", OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                };
                break;
            case "Running":
                EditorApplication.update += Tick;
                Application.logMessageReceived += OnLog;
                break;
            case "Done":
                Debug.Log(Sentinel);
                EditorApplication.delayCall += Cleanup;
                break;
        }
    }

    static void OnLog(string msg, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
            if (errors.Count < 30) errors.Add(msg);
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        frames++;
        if (frames < 5) return;
        if (startTime == 0) startTime = EditorApplication.timeSinceStartup;

        double elapsed = EditorApplication.timeSinceStartup - startTime;
        if (elapsed < 3.0) return; // let the match boot + spawn a puzzle

        if (!validated)
        {
            validated = true;
            var mgr = Object.FindFirstObjectByType<RankedMatchManager>();
            var puzzle = Object.FindFirstObjectByType<BasePuzzle>();
            var canvas = Object.FindFirstObjectByType<Canvas>();
            string res =
                $"RankedMatchManager={(mgr != null)} | activePlayers={(mgr != null ? mgr.activePlayers.Count : -1)} | " +
                $"puzzleSpawned={(puzzle != null)} ({(puzzle != null ? puzzle.GetType().Name : "none")}) | " +
                $"canvas={(canvas != null)} | errors={errors.Count}";
            if (errors.Count > 0) res += " :: " + string.Join(" || ", errors);
            SessionState.SetString(ResultKey, res);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            EditorApplication.isPlaying = false;
        }
    }

    static void Cleanup()
    {
        SessionState.EraseString(StateKey);
        if (AssetDatabase.AssetPathExists(ScriptPath))
            AssetDatabase.DeleteAsset(ScriptPath);
    }
}
