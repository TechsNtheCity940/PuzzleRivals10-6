#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

// TEMPORARY: verifies the FFA platform characters cycle their (new side-view)
// walk frames over time.
public class WalkAnimRuntimeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (SessionState.GetString("WalkT.Run", "0") != "1") return;
        SessionState.SetString("WalkT.Run", "0");
        var go = new GameObject("__WalkAnimTest");
        go.AddComponent<WalkAnimRuntimeTest>();
    }

    private IEnumerator Start()
    {
        FFAArenaCharacterController ctrl = null;
        float t = 0f;
        while (ctrl == null && t < 8f) { ctrl = FFAArenaCharacterController.Instance; t += Time.deltaTime; yield return null; }
        if (ctrl == null) { Finish("no controller"); yield break; }

        var seen = new System.Collections.Generic.HashSet<Sprite>[4];
        for (int i = 0; i < 4; i++) seen[i] = new System.Collections.Generic.HashSet<Sprite>();

        float elapsed = 0f;
        while (elapsed < 5f)
        {
            for (int i = 0; i < 4 && i < ctrl.characterImages.Length; i++)
            {
                var im = ctrl.characterImages[i];
                if (im != null && im.sprite != null) seen[i].Add(im.sprite);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        int animated = 0;
        string detail = "";
        for (int i = 0; i < 4; i++) { detail += " c"+i+"="+seen[i].Count; if (seen[i].Count >= 3) animated++; }
        Finish("animatedChars=" + animated + "/4 distinctSprites:" + detail);
    }

    private void Finish(string msg)
    {
        Debug.Log("[WalkT] RESULT " + msg);
        EditorApplication.isPlaying = false;
    }
}
#endif // UNITY_EDITOR
