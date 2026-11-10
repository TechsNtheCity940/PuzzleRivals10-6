#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

// TEMPORARY runtime tester (player-loop context) for the CardMatching preview.
// Spawns the CardMatchOverlay, runs Initialize+Start, and records whether the
// cards start face-up with the clock stopped, then flip down and the clock starts.
public class CardPreviewRuntimeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (SessionState.GetString("CMrt.Run", "0") != "1") return;
        SessionState.SetString("CMrt.Run", "0");
        var go = new GameObject("__CardPreviewRuntimeTest");
        go.AddComponent<CardPreviewRuntimeTest>();
    }

    private IEnumerator Start()
    {
        var canvasGo = new GameObject("__CMrt", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CardMatchOverlay.prefab");
        var inst = Object.Instantiate(prefab, canvasGo.transform);
        var rt = inst.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; rt.localScale = Vector3.one;

        MonoBehaviour pp = null;
        foreach (var mb in inst.GetComponentsInChildren<MonoBehaviour>(true))
            if (mb.GetType().Name == "CardMatchingPuzzle") { pp = mb; break; }
        var T = pp.GetType();
        var grid = (System.Array)T.GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pp);
        var cardBack = T.GetField("cardBack").GetValue(pp) as Object;
        var isRunningF = typeof(BasePuzzle).GetField("isRunning", BindingFlags.NonPublic | BindingFlags.Instance);

        T.GetMethod("InitializePuzzle", new[] { typeof(int), typeof(int) }).Invoke(pp, new object[] { 1, 7 });
        // re-fetch grid after BuildBoard
        grid = (System.Array)T.GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pp);

        int cards = 0; foreach (var t in grid) if (t != null) cards++;

        T.GetMethod("StartPuzzle").Invoke(pp, null);

        // Sample right after start (preview phase)
        yield return null;
        int upEarly = FacesUp(grid, cardBack);
        bool runEarly = (bool)isRunningF.GetValue(pp);

        // Wait through preview + flip (previewDuration 2.5 + flip 0.35 + margin)
        yield return new WaitForSeconds(3.4f);
        int upLate = FacesUp(grid, cardBack);
        bool runLate = (bool)isRunningF.GetValue(pp);

        bool success = cards == 12 && upEarly == 12 && !runEarly && upLate == 0 && runLate;
        string r = "RTRESULT success=" + success + " cards=" + cards
            + " | early facesUp=" + upEarly + " clockRunning=" + runEarly
            + " | late facesUp=" + upLate + " clockRunning=" + runLate;
        Debug.Log("[CMrt] " + r);
        SessionState.SetString("CMrt.Result", r);

        Object.Destroy(canvasGo);
        EditorApplication.isPlaying = false;
    }

    private static int FacesUp(System.Array grid, Object cardBack)
    {
        int up = 0;
        foreach (var tile in grid)
        {
            if (tile == null) continue;
            var t = (Component)tile;
            var img = (UnityEngine.UI.Image)t.GetType().GetProperty("Image").GetValue(t);
            if (img != null && img.sprite != null && (Object)img.sprite != cardBack) up++;
        }
        return up;
    }
}
#endif // UNITY_EDITOR
