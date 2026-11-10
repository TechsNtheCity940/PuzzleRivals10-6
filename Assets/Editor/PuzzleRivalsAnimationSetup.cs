#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Slices the project's real animation strips using their authored frame layouts
/// and assigns them to the UI/runtime components that actually consume Sprite[].
/// This intentionally replaces the old generic 4x4 SpriteRenderer generator.
/// </summary>
public static class PuzzleRivalsAnimationSetup
{
    private const string Textures = "Assets/Textures/";

    [MenuItem("PuzzleRivals/Setup Verified Animation Sheets")]
    public static void GenerateAndAssign()
    {
        int sliced = 0;
        int assigned = 0;

        Sprite[] bombIdle = SliceStrip(Textures + "Effects/Anim/bomb_idle_08f_v01.png", 8);
        Sprite[] bombHeld = SliceStrip(Textures + "Effects/Anim/bomb_held_08f_v01.png", 8);
        Sprite[] bombPass = SliceStrip(Textures + "Effects/Anim/bomb_pass_12f_v01.png", 12);
        Sprite[] explosion = SliceStrip(Textures + "Effects/Anim/explosion_24f_v01.png", 24);
        sliced += Count(bombIdle, bombHeld, bombPass, explosion);

        Sprite[] targetBlue = SliceStrip(Textures + "Puzzles/ScoreAttack/targets_blue_shatter.png", 8);
        Sprite[] targetGreen = SliceStrip(Textures + "Puzzles/ScoreAttack/targets_green_shatter.png", 8);
        Sprite[] targetPurple = SliceStrip(Textures + "Puzzles/ScoreAttack/targets_purple_shatter.png", 8);
        Sprite[] targetRed = SliceStrip(Textures + "Puzzles/ScoreAttack/targets_red_shatter.png", 8);
        sliced += Count(targetBlue, targetGreen, targetPurple, targetRed);

        Sprite[] beam = SliceStrip(Textures + "Puzzles/MirrorMaze/traveling_beam.png", 4);
        Sprite[] emitter = SliceStrip(Textures + "Puzzles/MirrorMaze/source_emitter.png", 4);
        Sprite[] receiver = SliceStrip(Textures + "Puzzles/MirrorMaze/target_receiver.png", 4);
        sliced += Count(beam, emitter, receiver);

        Sprite[] coreCharge = SliceStrip(Textures + "Puzzles/CircuitScramble/cs_core_charge.png", 8);
        Sprite[] mazePlayer = SliceGrid(Textures + "Puzzles/MazeRush/maze_player_anim.png", 4, 4);
        Sprite[] linkStraight = SliceGrid(Textures + "Puzzles/LinkLock/linklock_straight_flow.png", 4, 4, 8);
        Sprite[] linkCorner = SliceGrid(Textures + "Puzzles/LinkLock/linklock_corner_flow.png", 4, 4, 8);
        sliced += Count(coreCharge, mazePlayer, linkStraight, linkCorner);

        Sprite[] collapseIdle = SliceStrip(Textures + "Puzzles/CollapseGrid/collapse_grid_support_idle.png", 2);
        Sprite[] collapseCore = SliceStrip(Textures + "Puzzles/CollapseGrid/collapse_grid_weak_spot_pulse.png", 2);
        Sprite[] collapseCritical = SliceStrip(Textures + "Puzzles/CollapseGrid/collapse_grid_crack_spread.png", 2);
        Sprite[] collapseBrace = SliceStrip(Textures + "Puzzles/CollapseGrid/collapse_grid_brace.png", 2);
        Sprite[] collapseRemove = SliceStrip(Textures + "Puzzles/CollapseGrid/collapse_grid_debris_fall.png", 5);
        Sprite[] collapseBurst = SliceGrid(Textures + "Puzzles/CollapseGrid/collapse_anim.png", 4, 2);
        sliced += Count(collapseIdle, collapseCore, collapseCritical, collapseBrace, collapseRemove, collapseBurst);

        Sprite[] pipeStraight = SliceStrip(Textures + "Puzzles/PipePulse/pipe_straight_flow.png", 2);
        Sprite[] pipeCurve = SliceGrid(Textures + "Puzzles/PipePulse/pipe_curve_flow.png", 4, 4);
        // Use the RGBA sprite sheets. The newer pp_*_new images have a checkerboard
        // baked into RGB and would render that grid as part of the game art.
        Sprite[] pipeSource = SliceGrid(Textures + "Puzzles/PipePulse/pipe_source_v2.png", 4, 4);
        Sprite[] pipeReceiver = SliceGrid(Textures + "Puzzles/PipePulse/pipe_receiver_v2.png", 4, 4);
        sliced += Count(pipeStraight, pipeCurve, pipeSource, pipeReceiver);

        if (bombIdle.Length > 0 && bombPass.Length > 0)
            assigned += AssignBombPrefab(bombIdle, bombPass);

        assigned += AssignSceneAnimations(bombIdle, bombHeld, explosion);

        AssignBoth<CollapseGridPuzzle>("CollapseGridOverlay", p =>
        {
            p.normalFrames = collapseIdle;
            p.coreFrames = collapseCore;
            p.criticalFrames = collapseCritical;
            p.braceFrames = collapseBrace;
            p.removeFrames = collapseRemove;
            p.collapseFrames = collapseBurst;
        }, ref assigned);

        AssignBoth<CircuitScramblePuzzle>("CircuitScrambleOverlay", p =>
            p.coreChargeFrames = coreCharge, ref assigned);

        AssignBoth<LinkLockPuzzle>("LinkLockOverlay", p =>
        {
            p.straightFlowFrames = linkStraight;
            p.cornerFlowFrames = linkCorner;
        }, ref assigned);

        AssignBoth<MazeRushPuzzle>("MazeRushOverlay", p =>
            p.playerFrames = mazePlayer, ref assigned);

        AssignBoth<MirrorMazePuzzle>("MirrorMazeOverlay", p =>
        {
            p.beamFrames = beam;
            p.emitterFrames = emitter;
            p.receiverFrames = receiver;
        }, ref assigned);

        AssignBoth<TargetShootingPuzzle>("TargetShootOverlay", p =>
        {
            p.blueShatterFrames = targetBlue;
            p.greenShatterFrames = targetGreen;
            p.purpleShatterFrames = targetPurple;
            p.redShatterFrames = targetRed;
        }, ref assigned);

        AssignBoth<ScoreAttack>("ScoreAttackOverlay", p =>
        {
            p.shatterBlueFrames = targetBlue;
            p.shatterGreenFrames = targetGreen;
            p.shatterPurpleFrames = targetPurple;
            p.shatterRedFrames = targetRed;
        }, ref assigned);

        AssignBoth<PipePulsePuzzle>("PipePulseOverlay", p =>
        {
            var sets = p.pipeAnimations ?? Array.Empty<PipePulsePuzzle.PipeAnimationSet>();
            if (sets.Length < 3) Array.Resize(ref sets, 3);
            sets[0].flowFrames = pipeStraight;
            sets[1].flowFrames = pipeCurve;
            if (p.pipeSprites != null)
            {
                if (p.pipeSprites.Length > 0) sets[0].fallback = p.pipeSprites[0];
                if (p.pipeSprites.Length > 1) sets[1].fallback = p.pipeSprites[1];
                if (p.pipeSprites.Length > 2) sets[2].fallback = p.pipeSprites[2];
            }
            p.pipeAnimations = sets;
            p.sourceFrames = pipeSource;
            p.receiverFrames = pipeReceiver;
        }, ref assigned);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[PuzzleRivalsAnimationSetup] Sliced {sliced} verified frame sets and updated {assigned} prefab/scene components.");

        // Allow this setup to run deterministically from Unity batch mode.
        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    private static int Count(params Sprite[][] sets)
    {
        return sets.Count(s => s != null && s.Length > 0);
    }

    private static Sprite[] SliceStrip(string path, int frameCount)
    {
        return SliceGrid(path, frameCount, 1);
    }

    private static Sprite[] SliceGrid(string path, int columns, int rows, int frameLimit = 0)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (texture == null || importer == null)
        {
            Debug.LogError($"[PuzzleRivalsAnimationSetup] Missing animation texture: {path}");
            return Array.Empty<Sprite>();
        }

        int totalFrames = columns * rows;
        int count = frameLimit > 0 ? Mathf.Min(frameLimit, totalFrames) : totalFrames;
        var metadata = new SpriteMetaData[count];
        string baseName = Path.GetFileNameWithoutExtension(path);

        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            int x0 = Mathf.RoundToInt(col * texture.width / (float)columns);
            int x1 = Mathf.RoundToInt((col + 1) * texture.width / (float)columns);
            int topY0 = Mathf.RoundToInt(row * texture.height / (float)rows);
            int topY1 = Mathf.RoundToInt((row + 1) * texture.height / (float)rows);
            int y = texture.height - topY1;

            metadata[i] = new SpriteMetaData
            {
                name = baseName + "_" + i.ToString("D2"),
                rect = new Rect(x0, y, x1 - x0, topY1 - topY0),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 8192;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritesheet = metadata;
        importer.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(s => s.name, StringComparer.Ordinal)
            .ToArray();

        if (sprites.Length != count)
            Debug.LogError($"[PuzzleRivalsAnimationSetup] Expected {count} frames at {path}, imported {sprites.Length}.");

        return sprites;
    }

    private static int AssignBombPrefab(Sprite[] idle, Sprite[] pass)
    {
        const string path = "Assets/Prefabs/Bomb.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) return 0;

        try
        {
            var bomb = root.GetComponentInChildren<Bomb>(true);
            if (bomb == null) return 0;

            bomb.idleSprite = idle[0];
            bomb.litSprite = idle[0];
            bomb.idleFrames = Array.Empty<Sprite>();
            bomb.litFrames = idle;
            bomb.passFrames = pass;
            bomb.idleFps = 10f;
            bomb.litFps = 14f;
            if (bomb.spriteRenderer == null)
                bomb.spriteRenderer = root.GetComponentInChildren<SpriteRenderer>(true);
            if (bomb.spriteRenderer != null)
                bomb.spriteRenderer.sprite = idle[0];

            EditorUtility.SetDirty(bomb);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            return 1;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static int AssignSceneAnimations(Sprite[] bombIdle, Sprite[] bombHeld, Sprite[] explosion)
    {
        if (bombIdle == null || bombIdle.Length == 0 || bombHeld == null || bombHeld.Length == 0 || explosion == null || explosion.Length == 0)
        {
            Debug.LogError("[PuzzleRivalsAnimationSetup] Scene animation assignment skipped because a required frame set is missing.");
            return 0;
        }

        const string path = "Assets/Scenes/Match.unity";
        Scene previousActive = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Scene matchScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
        bool openedHere = !matchScene.IsValid() || !matchScene.isLoaded;

        if (openedHere)
            matchScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

        int updated = 0;
        try
        {
            foreach (var root in matchScene.GetRootGameObjects())
            {
                foreach (var controller in root.GetComponentsInChildren<FFAArenaCharacterController>(true))
                {
                    controller.bombHeldFrames = bombHeld;
                    controller.bombLitSprite = bombIdle[0];
                    controller.explosionFrames = explosion;
                    controller.explosionFps = 48f;
                    EditorUtility.SetDirty(controller);
                    updated++;
                }

                foreach (var hud in root.GetComponentsInChildren<FFAHudController>(true))
                {
                    hud.bombHeldFrames = bombHeld;
                    hud.bombLitSprite = bombIdle[0];
                    hud.explosionSprite = explosion[0];
                    EditorUtility.SetDirty(hud);
                    updated++;
                }
            }

            EditorSceneManager.SaveScene(matchScene);
        }
        finally
        {
            if (openedHere) EditorSceneManager.CloseScene(matchScene, true);
            if (previousActive.IsValid() && previousActive.isLoaded)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(previousActive);
        }

        return updated;
    }

    private static void AssignBoth<T>(string prefabName, Action<T> configure, ref int assigned)
        where T : Component
    {
        string[] paths =
        {
            "Assets/Resources/Prefabs/" + prefabName + ".prefab",
            "Assets/Prefabs/" + prefabName + ".prefab"
        };

        foreach (string path in paths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;

            try
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component == null)
                {
                    Debug.LogWarning($"[PuzzleRivalsAnimationSetup] {typeof(T).Name} not found in {path}.");
                    continue;
                }

                configure(component);
                EditorUtility.SetDirty(component);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                assigned++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
