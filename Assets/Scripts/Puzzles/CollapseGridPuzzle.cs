using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Structure-support demolition puzzle. The player is shown a brick building and
// must safely remove as many normal (cyan) bricks as possible. Pink "!" CORE
// bricks and the cracked CRITICAL supports directly beneath them are
// load-bearing — tapping any of those collapses the whole structure (fail).
// Gold BRACE bricks are fixed anchors and cannot be removed. Clearing every safe
// brick solves the puzzle; the fuse running out (BasePuzzle) is the timeout.
public class CollapseGridPuzzle : BasePuzzle
{
    // Brick roles (stored in GridTile.type).
    private const int NORMAL = 0;   // cyan support — safe to remove
    private const int CORE = 1;     // pink weak-spot — collapses if touched
    private const int CRITICAL = 2; // cracked support beneath a core — collapses if removed
    private const int BRACE = 3;    // gold anchor — locked, cannot be removed
    private const int GOLD_NUGGET = 4; // gold nugget — bonus / reward

    [Header("Animated Frames (sliced sheets)")]
    public Sprite[] normalFrames;
    public Sprite[] coreFrames;
    public Sprite[] criticalFrames;
    public Sprite[] braceFrames;
    public Sprite[] removeFrames;
    public Sprite[] collapseFrames;

    [Header("UI")]
    public TMP_Text progressText;
    public TMP_Text instructionText;

    private int[,] roles;
    private GridTile[,] grid;
    private int safeTotal;
    private int safeRemoved;
    private bool collapsed;
    private bool resolving;
    private Dictionary<GridTile, Coroutine> pendingClicks = new Dictionary<GridTile, Coroutine>();

    [Header("Layout")]
    public int columns = 6;
    public int rows = 6;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 92f;

    private float CenterX => (columns - 1) * tileSpacing * 0.5f;
    private float CenterY => (rows - 1) * tileSpacing * 0.5f;

    protected override void BuildBoard()
    {
        ClearContainer();
        collapsed = false;
        resolving = false;
        safeRemoved = 0;
        pendingClicks.Clear();

        // Scale the structure with difficulty.
        columns = Mathf.Clamp(5 + difficulty / 2, 5, 7);
        rows = Mathf.Clamp(5 + difficulty / 2, 5, 7);
        int coreCount = Mathf.Clamp(difficulty, 1, 3);

        roles = new int[columns, rows];
        grid = new GridTile[columns, rows];

        // Start as a solid building of normal bricks.
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                roles[c, r] = NORMAL;

        // Gold BRACE anchors at the two bottom corners (foundation pillars).
        roles[0, rows - 1] = BRACE;
        roles[columns - 1, rows - 1] = BRACE;

        // Place CORE bricks in the upper region; the brick directly beneath each
        // becomes a CRITICAL load-bearing support.
        var usedCols = new HashSet<int>();
        int placed = 0, guard = 0;
        while (placed < coreCount && guard++ < 100)
        {
            int c = rng.Next(1, columns - 1);          // avoid edge columns
            int r = rng.Next(0, Mathf.Max(1, rows - 2)); // upper rows, leave room below
            if (usedCols.Contains(c)) continue;
            if (roles[c, r] != NORMAL) continue;
            usedCols.Add(c);
            roles[c, r] = CORE;
            if (r + 1 < rows && roles[c, r + 1] == NORMAL)
                roles[c, r + 1] = CRITICAL;
            placed++;
        }

        // Place Gold Nuggets inside the building's inner core
        int nuggetCount = Mathf.Clamp(difficulty, 1, 2);
        int nuggetsPlaced = 0;
        guard = 0;
        while (nuggetsPlaced < nuggetCount && guard++ < 100)
        {
            int c = rng.Next(1, columns - 1);
            int r = rng.Next(1, rows - 1);
            if (roles[c, r] == NORMAL)
            {
                roles[c, r] = GOLD_NUGGET;
                nuggetsPlaced++;
            }
        }

        // Count the safe (normal) bricks — clearing them all solves the puzzle.
        safeTotal = 0;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                if (roles[c, r] == NORMAL) safeTotal++;

        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                CreateTile(c, r, roles[c, r]);

        if (instructionText != null)
            instructionText.text = "Remove the cyan bricks. Avoid the pink core and cracked supports! Isolate & tap Gold Nuggets!";
        UpdateProgress();
    }

    private void ClearContainer()
    {
        if (gridContainer == null) return;
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);
    }

    private GridTile CreateTile(int col, int row, int role)
    {
        GameObject go = Instantiate(tilePrefab, gridContainer);
        var tile = go.GetComponent<GridTile>();
        tile.Init(col, row, role);

        var rt = (RectTransform)tile.transform;
        rt.sizeDelta = new Vector2(tileSpacing * 0.96f, tileSpacing * 0.96f);
        rt.anchoredPosition = new Vector2(col * tileSpacing - CenterX, -(row * tileSpacing) + CenterY);

        var anim = go.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = go.AddComponent<UISpriteSheetAnimator>();
        anim.loop = true;
        anim.playOnEnable = false;
        anim.PlaySet(FramesForRole(role), FpsForRole(role), true);

        if (role == GOLD_NUGGET)
        {
            // Amber/Gold glowing tint color
            tile.SetColor(new Color(1f, 0.7f, 0f, 1f));
        }

        tile.OnClicked += HandleTileClicked;
        grid[col, row] = tile;
        return tile;
    }

    private Sprite[] FramesForRole(int role)
    {
        switch (role)
        {
            case CORE: return coreFrames;
            case CRITICAL: return criticalFrames;
            case BRACE: return braceFrames;
            case GOLD_NUGGET: return braceFrames; // Reuse brace frames
            default: return normalFrames;
        }
    }

    private float FpsForRole(int role)
    {
        switch (role)
        {
            case CORE: return 12f;      // urgent pulse
            case CRITICAL: return 8f;   // stressed flicker
            case BRACE: return 6f;      // slow gold shimmer
            case GOLD_NUGGET: return 8f; // fast sparkling shimmer
            default: return 6f;         // gentle cyan shimmer
        }
    }

    private void HandleTileClicked(GridTile tile)
    {
        if (!CanAcceptInput() || collapsed || resolving || tile == null) return;
        var t = grid[tile.x, tile.y];
        if (t != tile) return;

        int role = roles[tile.x, tile.y];
        switch (role)
        {
            case NORMAL:
                RemoveSafe(tile);
                break;
            case BRACE:
                StartCoroutine(Shake(tile.transform, 0.18f, 5f)); // locked anchor, no penalty
                break;
            case CORE:
            case CRITICAL:
                TriggerCollapse();
                break;
        }
    }

    private void RemoveSafe(GridTile tile)
    {
        int c = tile.x, r = tile.y;
        grid[c, r] = null;
        roles[c, r] = -1;
        safeRemoved++;
        UpdateProgress();

        tile.OnClicked -= HandleTileClicked;
        var anim = tile.GetComponent<UISpriteSheetAnimator>();
        GameObject go = tile.gameObject;
        if (anim != null && removeFrames != null && removeFrames.Length > 0)
            anim.PlaySet(removeFrames, 18f, false, () => { if (go != null) Destroy(go); });
        else
            Destroy(go);

        if (safeRemoved >= safeTotal) Solve();
    }

    private void TriggerCollapse()
    {
        if (collapsed) return;
        collapsed = true;
        resolving = true;
        StartCoroutine(CollapseRoutine());
    }

    private IEnumerator CollapseRoutine()
    {
        if (instructionText != null) instructionText.text = "STRUCTURE COLLAPSED!";

        // Full-overlay collapse burst using the danger frames.
        if (collapseFrames != null && collapseFrames.Length > 0 && gridContainer != null)
        {
            var burstGo = new GameObject("CollapseBurst", typeof(RectTransform));
            var brt = burstGo.transform as RectTransform;
            brt.SetParent(gridContainer.parent != null ? gridContainer.parent : gridContainer, false);
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            var img = burstGo.AddComponent<Image>();
            img.raycastTarget = false;
            var a = burstGo.AddComponent<UISpriteSheetAnimator>();
            a.playOnEnable = false;
            a.PlaySet(collapseFrames, 10f, false);
        }

        if (gridContainer != null)
            yield return Shake(gridContainer, 0.6f, 16f);
        else
            yield return new WaitForSeconds(0.6f);

        yield return new WaitForSeconds(0.3f);
        TimeOut(); // collapse == failure (bomb explodes / elimination)
    }

    private IEnumerator Shake(Transform target, float duration, float magnitude)
    {
        if (target == null) yield break;
        Vector3 origin = target.localPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float dx = (float)(rng.NextDouble() * 2 - 1) * magnitude;
            float dy = (float)(rng.NextDouble() * 2 - 1) * magnitude;
            target.localPosition = origin + new Vector3(dx, dy, 0f);
            yield return null;
        }
        target.localPosition = origin;
    }

    private void UpdateProgress()
    {
        if (progressText != null)
            progressText.text = "BRICKS CLEARED: " + safeRemoved + " / " + safeTotal;
    }

    // Bots only ever pull safe bricks — never the core or its critical supports.
    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || collapsed || grid == null) return;

        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                if (grid[c, r] != null && roles[c, r] == NORMAL)
                {
                    RemoveSafe(grid[c, r]);
                    return;
                }
    }
}
