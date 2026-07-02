#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AssignAnimationsToScenes
{
    private const string AnimFolder = "Assets/Animations";
    private const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Tools/Animations/Assign Animations To Scenes (Project-wide)")]
    public static void Assign()
    {
        var animGuids = AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { AnimFolder });
        var animMap = animGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);

        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
        if (sceneGuids == null || sceneGuids.Length == 0)
        {
            sceneGuids = AssetDatabase.FindAssets("t:Scene");
        }

        var scenePaths = sceneGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
        int totalAssigned = 0;

        foreach (var sp in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(sp, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                totalAssigned += ProcessGameObject(root, animMap);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Processed scene: {sp}");
        }

        EditorUtility.DisplayDialog("Assign Animations To Scenes", $"Assigned animations to {totalAssigned} objects across {scenePaths.Length} scenes.", "OK");
        AssetDatabase.Refresh();
    }

    private static int ProcessGameObject(GameObject root, Dictionary<string, string> animMap)
    {
        int count = 0;
        var transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            var obj = t.gameObject;
            var controllerPath = FindControllerForName(obj.name, animMap);
            if (controllerPath == null)
                continue;

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
                continue;

            var animator = obj.GetComponent<Animator>() ?? obj.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                sr.drawMode = SpriteDrawMode.Simple;
            }

            count++;
        }
        return count;
    }

    private static string FindControllerForName(string objectName, Dictionary<string, string> animMap)
    {
        if (animMap.TryGetValue(objectName, out var exact))
            return exact;

        string normalized = objectName.ToLowerInvariant();
        foreach (var kvp in animMap)
        {
            var key = kvp.Key.ToLowerInvariant();
            if (key == normalized || key == normalized + "_anim" || key == normalized + "effect" || key.Contains(normalized) || normalized.Contains(key) || key.StartsWith(normalized) || normalized.StartsWith(key))
                return kvp.Value;
        }

        foreach (var kvp in animMap)
        {
            var key = kvp.Key.ToLowerInvariant().Replace("_sheet", "").Replace("_anim", "").Replace("_effect", "");
            if (key.Contains(normalized) || normalized.Contains(key))
                return kvp.Value;
        }

        return null;
    }
}
#endif
