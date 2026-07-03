// Play Mode test: open Match.unity (FFA), confirm the HUD spawns 4 player cards,
// the bomb holder is highlighted, and the fuse bar updates. Self-deletes.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[InitializeOnLoad]
internal static class HudFlowTest
{
    const string StateKey = "HudFlow.State";
    const string ResultKey = "HudFlow.Result";
    const string ScriptPath = "Assets/Editor/HudFlowTest.cs";
    const string Sentinel = "HUD_FLOW_DONE";

    static int frames;
    static double startTime;
    static bool validated;
    static readonly List<string> errors = new List<string>();

    static HudFlowTest()
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
        if ((type == LogType.Error || type == LogType.Exception) && errors.Count < 30) errors.Add(msg);
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        frames++;
        if (frames < 5) return;
        if (startTime == 0) startTime = EditorApplication.timeSinceStartup;
        double elapsed = EditorApplication.timeSinceStartup - startTime;
        if (elapsed < 3.0) return;

        if (!validated)
        {
            validated = true;
            var ctrl = Object.FindFirstObjectByType<FFAHudController>();
            var cards = Object.FindObjectsByType<PlayerCard>(FindObjectsSortMode.None);
            int holderHi = 0, bombOn = 0;
            foreach (var c in cards)
            {
                if (c.holderHighlight != null && c.holderHighlight.activeInHierarchy) holderHi++;
                if (c.bombIcon != null && c.bombIcon.gameObject.activeInHierarchy) bombOn++;
            }
            float fill = ctrl != null && ctrl.fuseFill != null ? ctrl.fuseFill.fillAmount : -1f;
            string holderTxt = ctrl != null && ctrl.holderText != null ? ctrl.holderText.text : "(null)";

            string res = $"hud={(ctrl != null)} | cards={cards.Length} | holderHighlighted={holderHi} | bombIconsOn={bombOn} | fuseFill={fill:0.00} | holderText='{holderTxt}' | errors={errors.Count}";
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
        if (AssetDatabase.AssetPathExists(ScriptPath)) AssetDatabase.DeleteAsset(ScriptPath);
    }
}
