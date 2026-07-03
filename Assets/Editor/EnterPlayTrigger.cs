using UnityEditor;
using UnityEngine;

// Minimal one-shot: when CMrt.Enter flag is set, enter play mode (deferred so the
// reload settles). The runtime tester auto-spawns via RuntimeInitializeOnLoadMethod.
[InitializeOnLoad]
internal static class EnterPlayTrigger
{
    static EnterPlayTrigger()
    {
        if (SessionState.GetString("CMrt.Enter", "0") == "1")
        {
            SessionState.SetString("CMrt.Enter", "0");
            SessionState.SetString("CMrt.Run", "1");
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
        }
    }
}
