#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimationGenerator
{
    private static readonly string[] SheetRoots = new[]
    {
        "Assets/Textures/Puzzles",
        "Assets/Textures/Effects",
    };

    private const string AnimRoot = "Assets/Animations";
    private const string PrefabRoot = "Assets/Prefabs";
    private const int Cols = 4;
    private const int Rows = 4;
    private const float FrameSeconds = 1f / 12f; // 12 FPS

    [MenuItem("Tools/Animations/Generate Animations from Sheets")]
    public static void GenerateAll()
    {
        foreach (var root in SheetRoots)
        {
            ProcessSheetRoot(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Animation Generator", "Generation complete. Animations and prefabs saved under Assets/Animations and Assets/Prefabs.", "OK");
    }

    private static bool AssetsUpToDate(string sheetPath, string clipPath, string controllerPath, string prefabPath)
    {
        if (!File.Exists(clipPath) || !File.Exists(controllerPath) || !File.Exists(prefabPath))
            return false;

        var sheetTime = File.GetLastWriteTimeUtc(sheetPath);
        return File.GetLastWriteTimeUtc(clipPath) >= sheetTime
            && File.GetLastWriteTimeUtc(controllerPath) >= sheetTime
            && File.GetLastWriteTimeUtc(prefabPath) >= sheetTime;
    }

    private static void DeleteExistingAsset(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
    }

    private static void ProcessSheetRoot(string sheetsFolder)
    {
        if (!AssetDatabase.IsValidFolder(sheetsFolder))
        {
            Debug.LogWarning($"Sheet root not found: {sheetsFolder}");
            return;
        }

        string category = new DirectoryInfo(sheetsFolder).Name;
        string animOutFolder = Path.Combine(AnimRoot, category);
        string prefabOutFolder = Path.Combine(PrefabRoot, category);
        Directory.CreateDirectory(animOutFolder);
        Directory.CreateDirectory(prefabOutFolder);

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { sheetsFolder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().EndsWith("_sheet.png"))
                continue;

            string baseName = Path.GetFileNameWithoutExtension(path).Replace("_sheet", "");
            var clipPath = Path.Combine(animOutFolder, baseName + ".anim");
            var controllerPath = Path.Combine(animOutFolder, baseName + ".controller");
            var prefabPath = Path.Combine(prefabOutFolder, baseName + ".prefab");

            if (AssetsUpToDate(path, clipPath, controllerPath, prefabPath))
            {
                Debug.Log($"Skipping unchanged animation: {category}/{baseName}");
                continue;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = Mathf.Max(importer.maxTextureSize, 1024);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
                continue;

            int frameWidth = texture.width / Cols;
            int frameHeight = texture.height / Rows;
            var metas = new SpriteMetaData[Cols * Rows];
            int index = 0;
            for (int row = 0; row < Rows; row++)
            for (int col = 0; col < Cols; col++)
            {
                metas[index] = new SpriteMetaData
                {
                    rect = new Rect(col * frameWidth, texture.height - (row + 1) * frameHeight, frameWidth, frameHeight),
                    pivot = new Vector2(0.5f, 0.5f),
                    name = Path.GetFileNameWithoutExtension(path) + "_" + index,
                    alignment = (int)SpriteAlignment.Center,
                };
                index++;
            }

            importer.spritesheet = metas;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length == 0)
                continue;

            Directory.CreateDirectory(animOutFolder);
            Directory.CreateDirectory(prefabOutFolder);

            DeleteExistingAsset(clipPath);
            DeleteExistingAsset(controllerPath);
            DeleteExistingAsset(prefabPath);

            var clip = new AnimationClip { frameRate = 60 };
            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };
            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * FrameSeconds,
                    value = sprites[i],
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AssetDatabase.CreateAsset(clip, clipPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddMotion(clip);

            var go = new GameObject(baseName);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[0];
            sr.transform.localPosition = Vector3.zero;
            sr.drawMode = SpriteDrawMode.Simple;
            var animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"Generated animation & prefab for: {category}/{baseName}");
        }
    }
}
#endif
