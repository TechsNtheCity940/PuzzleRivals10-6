using UnityEditor;

[InitializeOnLoad]
internal static class WalkEnterPlay
{
    static WalkEnterPlay()
    {
        if (SessionState.GetString("WalkT.Enter", "0") == "1")
        {
            SessionState.SetString("WalkT.Enter", "0");
            SessionState.SetString("WalkT.Run", "1");
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
        }
    }
}
