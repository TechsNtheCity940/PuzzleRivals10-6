#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

// TEMPORARY: captures a play-mode screenshot of the running FFA arena so we can
// see what the characters actually look like (gray boxes?) at runtime.
public class WalkShotTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (SessionState.GetString("Shot.Run", "0") != "1") return;
        SessionState.SetString("Shot.Run", "0");
        var go = new GameObject("__WalkShotTest");
        go.AddComponent<WalkShotTest>();
    }

    private IEnumerator Start()
    {
        FFAArenaCharacterController ctrl = null;
        float t = 0f;
        while (ctrl == null && t < 8f) { ctrl = FFAArenaCharacterController.Instance; t += Time.deltaTime; yield return null; }
        // let things settle and characters start walking
        yield return new WaitForSeconds(2.5f);
        yield return new WaitForEndOfFrame();

        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = tex.EncodeToPNG();
        string path = "Assets/__walkshot.png";
        File.WriteAllBytes(path, png);
        Debug.Log("[Shot] saved " + path + " size=" + tex.width + "x" + tex.height);
        EditorApplication.isPlaying = false;
    }
}
#endif // UNITY_EDITOR
