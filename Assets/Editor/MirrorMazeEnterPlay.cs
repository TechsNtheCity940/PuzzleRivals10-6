using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
internal static class MirrorMazeEnterPlay
{
    static MirrorMazeEnterPlay()
    {
        if (SessionState.GetString("MMTest.Enter", "0") == "1")
        {
            SessionState.SetString("MMTest.Enter", "0");
            SessionState.SetString("MMTest.Run", "1");
            EditorSceneManager.OpenScene("Assets/Scenes/Match.unity");
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
        }
    }
}
