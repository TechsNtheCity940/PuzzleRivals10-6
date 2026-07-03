using UnityEditor;

[InitializeOnLoad]
internal static class WalkShotEnter
{
    static WalkShotEnter()
    {
        if (SessionState.GetString("Shot.Enter", "0") == "1")
        {
            SessionState.SetString("Shot.Enter", "0");
            SessionState.SetString("Shot.Run", "1");
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
        }
    }
}
