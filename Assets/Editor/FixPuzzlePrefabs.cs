using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class FixPuzzlePrefabs
{
    [MenuItem("PuzzleRivals/Fix All Puzzle Prefabs")]
    public static void FixAllPuzzlePrefabs()
    {
        var rmm = GameObject.FindObjectOfType<RankedMatchManager>();
        if (rmm == null)
        {
            Debug.LogError("RankedMatchManager not found in scene!");
            return;
        }
        
        // Find all overlay prefabs
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets/Prefabs"});
        var overlayPrefabs = new List<GameObject>();
        
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.name.EndsWith("Overlay"))
            {
                overlayPrefabs.Add(prefab);
                Debug.Log($"[FixPuzzlePrefabs] Found overlay prefab: {prefab.name}");
            }
        }
        
        if (overlayPrefabs.Count > 0)
        {
            rmm.puzzlePrefabs = overlayPrefabs;
            Debug.Log($"[FixPuzzlePrefabs] Assigned {overlayPrefabs.Count} overlay prefabs to RankedMatchManager");
            
            // Save the scene
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[FixPuzzlePrefabs] Scene saved!");
            }
        }
        else
        {
            Debug.LogWarning("[FixPuzzlePrefabs] No overlay prefabs found!");
        }
    }
}