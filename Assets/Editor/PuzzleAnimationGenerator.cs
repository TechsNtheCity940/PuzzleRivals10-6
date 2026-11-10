#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Compatibility entry point for the old menu item. The former generator assumed
/// every sheet was a 4x4 square grid and created disconnected SpriteRenderer
/// prefabs. PuzzleRivals now uses verified per-asset layouts and Sprite[] fields.
/// </summary>
public static class AnimationGenerator
{
    [MenuItem("Tools/Animations/Generate Animations from Sheets")]
    public static void GenerateAll()
    {
        Debug.Log("[AnimationGenerator] Routing to the verified PuzzleRivals animation setup. The legacy generic 4x4 generator is disabled.");
        PuzzleRivalsAnimationSetup.GenerateAndAssign();
    }
}
#endif
