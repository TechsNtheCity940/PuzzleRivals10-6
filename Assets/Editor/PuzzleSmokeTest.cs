// Play Mode smoke test: instantiate each overlay puzzle under a Canvas,
// InitializePuzzle, then hammer AutoSolveChunk and confirm it solves.
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;

[InitializeOnLoad]
internal static class PuzzleSmokeTest
{
    const string StateKey = "PuzzleSmoke.State";
    const string ResultKey = "PuzzleSmoke.Result";
    const string ScriptPath = "Assets/Editor/PuzzleSmokeTest.cs";
    const string Sentinel = "PUZZLE_SMOKE_DONE";

    static readonly string[] Prefabs = {
        "Assets/Prefabs/PipePulseOverlay.prefab",
        "Assets/Prefabs/CircuitScrambleOverlay.prefab",
        "Assets/Prefabs/CollapseGridOverlay.prefab",
        "Assets/Prefabs/MazeRushOverlay.prefab",
        "Assets/Prefabs/MirrorMazeOverlay.prefab",
        "Assets/Prefabs/LinkLockOverlay.prefab",
        "Assets/Prefabs/RuleBreakerOverlay.prefab",
        "Assets/Prefabs/ComboRushOverlay.prefab",
    };

    static Canvas canvas;
    static int index;
    static GameObject overlay;
    static BasePuzzle puzzle;
    static int calls;
    static int waitFrames;
    static StringBuilder report;
    static bool started;

    const int MaxCallsPerPuzzle = 4000;
    const int CallsPerTick = 8;

    static PuzzleSmokeTest()
    {
        string state = SessionState.GetString(StateKey, "Idle");
        switch (state)
        {
            case "Launch":
                SessionState.SetString(StateKey, "Running");
                EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
                break;
            case "Running":
                EditorApplication.update += Tick;
                break;
            case "Done":
                Debug.Log(Sentinel);
                EditorApplication.delayCall += Cleanup;
                break;
        }
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;

        if (!started)
        {
            waitFrames++;
            if (waitFrames < 5) return; // let play mode settle
            started = true;
            report = new StringBuilder();
            EnsureCanvasAndEventSystem();
            index = -1;
            NextPuzzle();
            return;
        }

        if (puzzle == null) return; // finishing

        for (int i = 0; i < CallsPerTick; i++)
        {
            if (puzzle.isSolved || puzzle.isTimedOut) break;
            puzzle.AutoSolveChunk();
            calls++;
            if (calls >= MaxCallsPerPuzzle) break;
        }

        if (puzzle.isSolved || puzzle.isTimedOut || calls >= MaxCallsPerPuzzle)
        {
            int tiles = overlay != null ? CountTiles(overlay) : 0;
            string status = puzzle.isSolved ? "SOLVED" : (puzzle.isTimedOut ? "TIMEDOUT" : "STUCK");
            report.AppendLine($"{puzzle.GetType().Name}: {status} in {calls} calls (tiles~{tiles})");
            if (overlay != null) Object.Destroy(overlay);
            overlay = null;
            puzzle = null;
            NextPuzzle();
        }
    }

    static void NextPuzzle()
    {
        index++;
        if (index >= Prefabs.Length)
        {
            SessionState.SetString(ResultKey, report.ToString());
            SessionState.SetString(StateKey, "Done");
            EditorApplication.update -= Tick;
            EditorApplication.isPlaying = false;
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs[index]);
        if (prefab == null)
        {
            report.AppendLine($"{Prefabs[index]}: PREFAB NULL");
            NextPuzzle();
            return;
        }

        overlay = Object.Instantiate(prefab, canvas.transform);
        var rt = overlay.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        puzzle = overlay.GetComponentInChildren<BasePuzzle>(true);
        calls = 0;

        if (puzzle == null)
        {
            report.AppendLine($"{prefab.name}: NO BasePuzzle");
            Object.Destroy(overlay);
            overlay = null;
            NextPuzzle();
            return;
        }

        try
        {
            puzzle.puzzleTime = 1000000f; // prevent fuse timeout during the test
            puzzle.inputEnabled = true;
            puzzle.InitializePuzzle(1, 12345);
            puzzle.StartPuzzle();
        }
        catch (System.Exception e)
        {
            report.AppendLine($"{puzzle.GetType().Name}: EXCEPTION in build/start: {e.Message}");
            Object.Destroy(overlay);
            overlay = null;
            puzzle = null;
            NextPuzzle();
        }
    }

    static int CountTiles(GameObject go)
    {
        int n = 0;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) n++;
        return n;
    }

    static void EnsureCanvasAndEventSystem()
    {
        var go = new GameObject("SmokeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static void Cleanup()
    {
        SessionState.EraseString(StateKey);
        if (AssetDatabase.AssetPathExists(ScriptPath))
            AssetDatabase.DeleteAsset(ScriptPath);
    }
}
