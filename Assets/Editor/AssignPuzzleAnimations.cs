#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AssignPuzzleAnimations
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string AnimFolder = "Assets/Animations";

    [MenuItem("Tools/Animations/Assign Animations To Prefabs")]
    public static void Assign()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
        var animGuids = AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { AnimFolder });
        var animMap = animGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);

        int assigned = 0;
        foreach (var pg in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(pg);
            var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            string candidate = FindBestMatch(prefabName, animMap);
            if (candidate == null)
                continue;

            var instance = PrefabUtility.LoadPrefabContents(prefabPath);
            var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(candidate);
            if (controller == null)
            {
                PrefabUtility.UnloadPrefabContents(instance);
                continue;
            }

            animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            PrefabUtility.UnloadPrefabContents(instance);
            assigned++;
            Debug.Log($"Assigned {Path.GetFileName(candidate)} to prefab {prefabPath}");
        }

        EditorUtility.DisplayDialog("Assign Animations To Prefabs", $"Assigned animations to {assigned} prefabs.", "OK");
        AssetDatabase.Refresh();
    }

    private static string FindBestMatch(string prefabName, System.Collections.Generic.Dictionary<string, string> animMap)
    {
        if (animMap.TryGetValue(prefabName, out var exact))
            return exact;

        string lowerName = prefabName.ToLowerInvariant();
        foreach (var kvp in animMap)
        {
            var key = kvp.Key.ToLowerInvariant();
            if (key == lowerName || key == lowerName + "_anim" || key == lowerName + "effect" || key.Contains(lowerName) || lowerName.Contains(key) || key.StartsWith(lowerName) || lowerName.StartsWith(key))
                return kvp.Value;
        }

        foreach (var kvp in animMap)
        {
            var key = kvp.Key.ToLowerInvariant();
            if (key.Replace("_sheet", "").Replace("_anim", "").Contains(lowerName) || lowerName.Contains(key.Replace("_sheet", "").Replace("_anim", "")))
                return kvp.Value;
        }

        return null;
    }
}
#endif
