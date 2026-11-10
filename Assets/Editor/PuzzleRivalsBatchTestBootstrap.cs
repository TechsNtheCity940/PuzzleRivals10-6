using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PuzzleRivalsBatchTestBootstrap
{
    private const string MonitorKey = "PuzzleRivalsBatchTest.Monitor";
    private const string StartKey = "PuzzleRivalsBatchTest.Start";

    static PuzzleRivalsBatchTestBootstrap()
    {
        if (SessionState.GetBool(MonitorKey, false))
            EditorApplication.update += Monitor;
    }

    public static void Run()
    {
        SessionState.SetBool(MonitorKey, true);
        SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);

        EditorSceneManager.OpenScene("Assets/Scenes/Match.unity", OpenSceneMode.Single);

        // GameState is a runtime singleton, not a prefab in this project.
        // AllPuzzlesSolveTest creates it after Play Mode begins so Awake runs normally.
        Unity.AI.Assistant.Tests.AllPuzzlesSolveTest.Run();
        EditorApplication.update -= Monitor;
        EditorApplication.update += Monitor;
        Debug.Log("[PUZZLERIVALS_BATCH_TEST] Started FFA all-puzzles solve test.");
    }

    private static void Monitor()
    {
        string state = SessionState.GetString("AllPuzzlesSolveTest.State", "Idle");
        if (state == "Done")
        {
            string result = SessionState.GetString("AllPuzzlesSolveTest.Result", "Missing test result");
            Debug.Log("[PUZZLERIVALS_BATCH_TEST] RESULT=" + result);
            SessionState.SetBool(MonitorKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.Exit(result == "SUCCESS" ? 0 : 1);
            return;
        }

        float started = SessionState.GetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
        if (EditorApplication.timeSinceStartup - started > 190f)
        {
            Debug.LogError("[PUZZLERIVALS_BATCH_TEST] Timed out waiting for FFA test.");
            SessionState.SetBool(MonitorKey, false);
            EditorApplication.update -= Monitor;
            EditorApplication.Exit(2);
        }
    }
}
