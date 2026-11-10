using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class PuzzlePrefabsSetup
{
    [MenuItem("PuzzleRivals/Setup Puzzle Prefabs")]
    public static void SetupPuzzlePrefabs()
    {
        // Create Resources/Prefabs folder if it doesn't exist
        string resourcesPath = "Assets/Resources/Prefabs";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            Debug.Log("[PuzzlePrefabsSetup] Created Resources/Prefabs folder");
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
                Debug.Log($"[PuzzlePrefabsSetup] Found overlay prefab: {prefab.name}");
            }
        }

        if (overlayPrefabs.Count > 0)
        {
            // Copy overlay prefabs to Resources/Prefabs
            foreach (var prefab in overlayPrefabs)
            {
                string destPath = $"Assets/Resources/Prefabs/{prefab.name}.prefab";
                AssetDatabase.CopyAsset($"Assets/Prefabs/{prefab.name}.prefab", destPath);
                Debug.Log($"[PuzzlePrefabsSetup] Copied {prefab.name} to Resources/Prefabs");
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"[PuzzlePrefabsSetup] Setup complete! {overlayPrefabs.Count} overlay prefabs copied to Resources/Prefabs");
        }
        else
        {
            Debug.LogWarning("[PuzzlePrefabsSetup] No overlay prefabs found!");
        }
    }
}