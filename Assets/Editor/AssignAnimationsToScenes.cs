#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class AssignAnimationsToScenes
{
    private const string AnimFolder = "Assets/Animations/Puzzles";
    private const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Tools/Puzzles/Assign Animations To Scenes (Project-wide)")]
    public static void Assign()
    {
        var animGuids = AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { AnimFolder });
        var animMap = animGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToDictionary(p => Path.GetFileNameWithoutExtension(p), p => p);

        // Collect scenes: prefer ScenesFolder, fallback to all scenes in project
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
        if (sceneGuids == null || sceneGuids.Length == 0)
        {
            sceneGuids = AssetDatabase.FindAssets("t:Scene");
        }
        var scenePaths = sceneGuids.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();

        int totalAssigned = 0;
        for (int i = 0; i < scenePaths.Length; i++)
        {
            var sp = scenePaths[i];
            var scene = EditorSceneManager.OpenScene(sp, OpenSceneMode.Single);
            var rootGOs = scene.GetRootGameObjects();
            foreach (var root in rootGOs)
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
            var name = obj.name;
            string controllerPath = null;

            // Exact match
            if (animMap.TryGetValue(name, out var p)) controllerPath = p;
            else
            {
                // Best-effort fuzzy match
                var key = animMap.Keys.FirstOrDefault(k => k.Equals(name) || k.Equals(name + "_anim") || k.Contains(name) || name.Contains(k) || k.StartsWith(name) || name.StartsWith(k));
                if (key != null) controllerPath = animMap[key];
            }

            if (controllerPath == null) continue;

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null) continue;

            var animator = obj.GetComponent<Animator>();
            if (animator == null) animator = obj.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            // Normalize transform for sprites
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;

                // Ensure sprite is centered (pivot handled at import)
                // Set draw mode to simple
                sr.drawMode = SpriteDrawMode.Simple;
            }

            count++;
        }
        return count;
    }
}
#endif
