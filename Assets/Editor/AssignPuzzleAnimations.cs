#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AssignPuzzleAnimations
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string AnimFolder = "Assets/Animations/Puzzles";

    [MenuItem("Tools/Puzzles/Assign Animations To Prefabs")]
    public static void Assign()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
        var animGuids = AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { AnimFolder });
        var animMap = animGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);

        int assigned = 0;
        foreach (var pg in prefabGuids)
        {
            var ppath = AssetDatabase.GUIDToAssetPath(pg);
            var name = Path.GetFileNameWithoutExtension(ppath);
            // Try to find matching controller by exact or base name
            string candidate = null;
            if (animMap.TryGetValue(name, out var ap)) candidate = ap;
            else
            {
                // try variants
                foreach (var k in animMap.Keys)
                {
                    if (k.StartsWith(name) || name.StartsWith(k) || k.Contains(name) || name.Contains(k))
                    {
                        candidate = animMap[k];
                        break;
                    }
                }
            }
            if (candidate == null) continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ppath);
            if (prefab == null) continue;

            var instance = PrefabUtility.LoadPrefabContents(ppath);
            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(candidate);
            if (controller == null) continue;
            animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(instance, ppath);
            PrefabUtility.UnloadPrefabContents(instance);
            assigned++;
            Debug.Log($"Assigned {candidate} to prefab {ppath}");
        }

        EditorUtility.DisplayDialog("Assign Puzzle Animations", $"Assigned animations to {assigned} prefabs.", "OK");
        AssetDatabase.Refresh();
    }
}
#endif
