using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 10);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 100;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _hasRun = false;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (_hasRun) return;
            _hasRun = true;
            EditorApplication.update -= WaitFramesThenRun;

            Application.logMessageReceived += OnLogMessage;
            
            var runnerGo = new GameObject("__PlayModeTestCoroRunner");
            Object.DontDestroyOnLoad(runnerGo);
            var runner = runnerGo.AddComponent<CoroRunner>();
            runner.StartCoroutine(RunAsyncTest());
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        private static IEnumerator RunAsyncTest()
        {
            string resultJson = "";
            bool success = false;
            string error = "";

            TargetShootingPuzzle puzzle = null;
            float elapsed = 0f;
            while (puzzle == null && elapsed < 8f)
            {
                puzzle = Object.FindAnyObjectByType<TargetShootingPuzzle>(FindObjectsInactive.Include);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (puzzle == null)
            {
                error = "TargetShootingPuzzle not found in active scene after 8 seconds.";
            }
            else
            {
                Debug.Log("[Test] Found TargetShootingPuzzle. isSolved=" + puzzle.isSolved);
                
                // Let's run bot solve step by step
                int steps = 0;
                while (!puzzle.isSolved && steps < 60)
                {
                    puzzle.AutoSolveChunk();
                    steps++;
                    yield return new WaitForSeconds(0.12f);
                }

                if (puzzle.isSolved)
                {
                    Debug.Log("[Test] TargetShootingPuzzle solved cleanly in " + steps + " steps!");
                    success = true;
                }
                else
                {
                    error = "Puzzle failed to solve within 60 steps.";
                }
            }

            Application.logMessageReceived -= OnLogMessage;
            PlayerPrefs.DeleteKey("CSTest.Run");

            resultJson = SerializeResult(success, error);
            string filePath = System.IO.Path.Combine(Application.dataPath, "../cstest_result.txt");
            System.IO.File.WriteAllText(filePath, success ? "SUCCESS: Target Shooting solved" : "ERROR: " + error);

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static string SerializeResult(bool success, string error)
        {
            return JsonUtility.ToJson(new TestResult
            {
                success = success,
                error = error,
                logs = _capturedLogs.ToArray()
            });
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
        }

        private class CoroRunner : MonoBehaviour { }
    }
}