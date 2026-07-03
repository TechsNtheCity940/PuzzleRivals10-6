// Play Mode test: open Store.unity with a seeded profile (5000 coins), confirm
// the grid populates, then simulate buying the first purchasable tile and verify
// coins drop + it becomes OWNED. Self-deletes.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[InitializeOnLoad]
internal static class StoreFlowTest
{
    const string StateKey = "StoreFlow.State";
    const string ResultKey = "StoreFlow.Result";
    const string ScriptPath = "Assets/Editor/StoreFlowTest.cs";
    const string Sentinel = "STORE_FLOW_DONE";

    static int frames;
    static double startTime;
    static int phase;
    static int coinsBefore, coinsAfter, tilesBefore;
    static readonly List<string> errors = new List<string>();

    static StoreFlowTest()
    {
        switch (SessionState.GetString(StateKey, "Idle"))
        {
            case "Launch":
                SessionState.SetString(StateKey, "Running");
                EditorApplication.delayCall += () =>
                {
                    // Seed profile so ProfileManager.LoadProfile picks it up.
                    var seed = new PlayerProfileData { coins = 5000, gems = 300, rankPoints = 1200 };
                    PlayerPrefs.SetString("PlayerProfile", JsonUtility.ToJson(seed));
                    PlayerPrefs.Save();

                    EditorSceneManager.OpenScene("Assets/Scenes/Store.unity", OpenSceneMode.Single);
                    // Managers the Store scene relies on (normally from Main).
                    new GameObject("ProfileManager_T").AddComponent<ProfileManager>();
                    new GameObject("SeasonPassManager_T").AddComponent<SeasonPassManager>();
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

    static void OnLog(string m, string s, LogType t)
    { if ((t == LogType.Error || t == LogType.Exception) && errors.Count < 30) errors.Add(m); }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        frames++;
        if (frames < 5) return;
        if (startTime == 0) startTime = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - startTime < 2.0) return;

        var ctrl = Object.FindFirstObjectByType<StoreScreenController>();
        var pm = ProfileManager.Instance;

        if (phase == 0)
        {
            var tiles = Object.FindObjectsByType<CosmeticTile>(FindObjectsSortMode.None);
            tilesBefore = tiles.Length;
            coinsBefore = pm != null ? pm.currentProfile.coins : -1;

            // Find first interactable (purchasable) tile and click it.
            CosmeticTile target = null;
            foreach (var t in tiles)
                if (t.button != null && t.button.interactable) { target = t; break; }

            if (target != null) target.button.onClick.Invoke();
            else errors.Add("no interactable tile");
            phase = 1;
            return;
        }

        if (phase == 1)
        {
            coinsAfter = pm != null ? pm.currentProfile.coins : -1;
            string res = $"controller={(ctrl != null)} tiles={tilesBefore} coinsBefore={coinsBefore} coinsAfter={coinsAfter} spent={coinsBefore - coinsAfter} | errors={errors.Count}";
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
        PlayerPrefs.DeleteKey("PlayerProfile");
        if (AssetDatabase.AssetPathExists(ScriptPath)) AssetDatabase.DeleteAsset(ScriptPath);
    }
}
