#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PuzzleAnimationGenerator
{
    private const string SheetsFolder = "Assets/Textures/Puzzles";
    private const string AnimOutFolder = "Assets/Animations/Puzzles";
    private const string PrefabOutFolder = "Assets/Prefabs/Puzzles";
    private const int Cols = 4;
    private const int Rows = 4;
    private const float FrameSeconds = 1f / 12f; // 12 FPS

    [MenuItem("Tools/Puzzles/Generate Animations from Sheets")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory(AnimOutFolder);
        Directory.CreateDirectory(PrefabOutFolder);

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SheetsFolder });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.ToLower().EndsWith("_sheet.png")) continue;

            // Configure importer for multiple sprites
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = Mathf.Max(ti.maxTextureSize, 1024);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) continue;
            int tw = tex.width / Cols;
            int th = tex.height / Rows;

            var metas = new SpriteMetaData[Cols * Rows];
            int idx = 0;
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                var sm = new SpriteMetaData();
                sm.rect = new RectInt(c * tw, (tex.height - (r + 1) * th), tw, th);
                sm.pivot = new Vector2(0.5f, 0.5f);
                sm.name = Path.GetFileNameWithoutExtension(path) + "_" + idx;
                metas[idx++] = sm;
            }

            ti.spritesheet = metas;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Load created sprites
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length == 0) continue;

            // Create animation clip
            var baseName = Path.GetFileNameWithoutExtension(path).Replace("_sheet", "");
            var clipPath = Path.Combine(AnimOutFolder, baseName + ".anim");
            var clip = new AnimationClip();
            clip.frameRate = 60;

            var binding = new EditorCurveBinding();
            binding.type = typeof(SpriteRenderer);
            binding.propertyName = "m_Sprite";

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe();
                keyframes[i].time = i * FrameSeconds;
                keyframes[i].value = sprites[i];
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Save clip
            if (!AssetDatabase.IsValidFolder(AnimOutFolder)) AssetDatabase.CreateFolder("Assets/Animations", "Puzzles");
            AssetDatabase.CreateAsset(clip, clipPath);

            // Create AnimatorController
            var controllerPath = Path.Combine(AnimOutFolder, baseName + ".controller");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddMotion(clip);

            // Create prefab with SpriteRenderer + Animator
            if (!AssetDatabase.IsValidFolder(PrefabOutFolder)) AssetDatabase.CreateFolder("Assets/Prefabs", "Puzzles");
            var go = new GameObject(baseName);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[0];
            sr.transform.localPosition = Vector3.zero;
            var animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            var prefabPath = Path.Combine(PrefabOutFolder, baseName + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"Generated animation & prefab for: {baseName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Puzzle Animation Generator", "Generation complete. Animations and prefabs saved to Assets/Animations/Puzzles and Assets/Prefabs/Puzzles.", "OK");
    }
}
#endif
