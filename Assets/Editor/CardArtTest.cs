// Play Mode test: open Match.unity (FFA), let it build the HUD cards, then
// verify each card's images have sprites + non-zero size and report per-rival
// emblem assignment. Self-deletes.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

[InitializeOnLoad]
internal static class CardArtTest
{
    const string StateKey = "CardArt.State";
    const string ResultKey = "CardArt.Result";
    const string ScriptPath = "Assets/Editor/CardArtTest.cs";
    const string Sentinel = "CARD_ART_DONE";

    static int frames;
    static double startTime;
    static bool validated;
    static readonly List<string> errors = new List<string>();

    static CardArtTest()
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
        if (EditorApplication.timeSinceStartup - startTime < 3.0) return;

        if (!validated)
        {
            validated = true;
            var cards = Object.FindObjectsByType<PlayerCard>(FindObjectsSortMode.None);
            var sb = new StringBuilder();
            sb.Append($"cards={cards.Length} | ");
            foreach (var c in cards)
            {
                string emblem = c.emblem != null && c.emblem.sprite != null ? c.emblem.sprite.name : "NULL";
                bool frameOk = c.frame != null && c.frame.sprite != null;
                bool bgOk = c.backgroundCard != null && c.backgroundCard.sprite != null;
                var rt = c.GetComponent<RectTransform>();
                bool sized = rt != null && rt.rect.width > 0 && rt.rect.height > 0;
                sb.Append($"[{c.nameText?.text}: emblem={emblem} frame={frameOk} bg={bgOk} sized={sized}] ");
            }
            sb.Append($"| errors={errors.Count}");
            if (errors.Count > 0) sb.Append(" :: " + string.Join(" || ", errors));
            SessionState.SetString(ResultKey, sb.ToString());
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
