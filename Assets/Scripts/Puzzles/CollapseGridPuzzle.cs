using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Structure-support demolition puzzle. The player is shown a brick building and
/// must safely remove as many normal (cyan) bricks as possible. Pink "!\" CORE
/// bricks and the cracked CRITICAL supports directly beneath them are
/// load-bearing — tapping any of those collapses the whole structure (fail).
/// Gold BRACE bricks are fixed anchors and cannot be removed, doesn't fall.
/// Clearing every safe brick solves the puzzle; the fuse running out (BasePuzzle) is the timeout.
/// </summary>
public class CollapseGridPuzzle : BasePuzzle
{
    private const int NORMAL = 0;   // cyan support — safe to remove
    private const int CORE = 1;     // pink weak-spot — collapses if touches the floor
    private const int CRITICAL = 2; // cyan support beneath a core — shown with cracked art
    private const int BRACE = 3;    // gold anchor — locked, cannot be removed, doesn't fall
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
    private bool isGravityRunning;

    [Header("Layout")]
    public int columns = 7;
    public int rows = 7;
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
        isGravityRunning = false;
        safeRemoved = 0;

        // Scale grid size with difficulty
        columns = Mathf.Clamp(6 + difficulty / 3, 6, 8);
        rows = Mathf.Clamp(7 + difficulty / 3, 7, 9);
        int coreCount = Mathf.Clamp(1 + difficulty / 2, 1, 4);

        roles = new int[columns, rows];
        grid = new GridTile[columns, rows];

        // Initialize empty
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                roles[c, r] = -1;

        // Fill with blocks
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                roles[c, r] = NORMAL;

        // Bottom foundation (not on every column to leave gaps)
        for (int c = 0; c < columns; c += 2) roles[c, rows - 1] = BRACE;

        // Random internal braces (stable platforms)
        int braceCount = 2 + difficulty / 2;
        for (int i = 0; i < braceCount; i++)
        {
            int c = rng.Next(0, columns);
            int r = rng.Next(rows / 3, rows - 1);
            roles[c, r] = BRACE;
        }

        // Place CORE bricks in upper region
        int coresPlaced = 0;
        int guard = 0;
        while (coresPlaced < coreCount && guard++ < 100)
        {
            int c = rng.Next(0, columns);
            int r = rng.Next(0, 3);
            if (roles[c, r] == NORMAL)
            {
                roles[c, r] = CORE;
                coresPlaced++;
            }
        }

        // Count safe
        safeTotal = 0;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                if (roles[c, r] == NORMAL) safeTotal++;

        // Instantiate
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                if (roles[c, r] != -1) CreateTile(c, r, roles[c, r]);

        UpdateCriticalStates();

        if (instructionText != null)
            instructionText.text = "DEMOLITION: Clear all Cyan blocks! Do NOT let Pink cores hit the floor (bottom row)!";
        UpdateProgress();
    }

    private void ClearContainer()
    {
        if (gridContainer == null) return;
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);
    }

    private void UpdateCriticalStates()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (roles[c, r] == NORMAL || roles[c, r] == CRITICAL)
                {
                    bool isSupportingCore = false;
                    if (r > 0 && roles[c, r - 1] == CORE) isSupportingCore = true;

                    int oldRole = roles[c, r];
                    roles[c, r] = isSupportingCore ? CRITICAL : NORMAL;

                    if (grid[c, r] != null && oldRole != roles[c, r])
                    {
                        var anim = grid[c, r].GetComponent<UISpriteSheetAnimator>();
                        if (anim != null) anim.PlaySet(FramesForRole(roles[c, r]), FpsForRole(roles[c, r]), true);
                    }
                }
            }
        }
    }

    private GridTile CreateTile(int col, int row, int role)
    {
        GameObject go = Instantiate(tilePrefab, gridContainer);
        var tile = go.GetComponent<GridTile>();
        tile.Init(col, row, role);

        var rt = (RectTransform)tile.transform;
        rt.sizeDelta = new Vector2(tileSpacing * 0.96f, tileSpacing * 0.96f);
        rt.anchoredPosition = GetAnchoredPos(col, row);

        var anim = go.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = go.AddComponent<UISpriteSheetAnimator>();
        anim.loop = true;
        anim.playOnEnable = false;
        anim.PlaySet(FramesForRole(role), FpsForRole(role), true);

        if (role == GOLD_NUGGET) tile.SetColor(new Color(1f, 0.8f, 0.2f, 1f));

        tile.OnClicked += HandleTileClicked;
        grid[col, row] = tile;
        return tile;
    }

    private Vector2 GetAnchoredPos(int c, int r)
    {
        return new Vector2(c * tileSpacing - CenterX, -(r * tileSpacing) + CenterY);
    }

    private Sprite[] FramesForRole(int role)
    {
        switch (role)
        {
            case CORE: return coreFrames;
            case CRITICAL: return criticalFrames;
            case BRACE: return braceFrames;
            case GOLD_NUGGET: return braceFrames;
            default: return normalFrames;
        }
    }

    private float FpsForRole(int role)
    {
        switch (role)
        {
            case CORE: return 12f;
            case CRITICAL: return 10f;
            case BRACE: return 6f;
            case GOLD_NUGGET: return 9f;
            default: return 6f;
        }
    }

    private void HandleTileClicked(UITile tile)
    {
        GridTile gt = tile as GridTile;
        if (gt == null) return;
        if (!CanAcceptInput() || collapsed || resolving || isGravityRunning || gt == null) return;

        int role = roles[gt.x, gt.y];
        if (role == NORMAL || role == CRITICAL)
        {
            RemoveSafe(gt);
        }
        else if (role == GOLD_NUGGET)
        {
            CollectNugget(gt);
        }
        else if (role == BRACE)
        {
            StartCoroutine(Shake(gt.transform, 0.15f, 4f));
        }
        else if (role == CORE)
        {
            StartCoroutine(Shake(gt.transform, 0.2f, 8f));
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
            anim.PlaySet(removeFrames, 20f, false, () => { if (go != null) Destroy(go); });
        else
            Destroy(go);

        StartCoroutine(ApplyGravityRoutine());
    }

    private void CollectNugget(GridTile tile)
    {
        timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 3f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(GameState.Instance.sfxUiConfirm, 0.7f);

        int c = tile.x, r = tile.y;
        grid[c, r] = null;
        roles[c, r] = -1;

        tile.OnClicked -= HandleTileClicked;
        tile.Pulse(1.4f, 0.2f);
        Destroy(tile.gameObject, 0.25f);

        StartCoroutine(ApplyGravityRoutine());
    }

    private IEnumerator ApplyGravityRoutine()
    {
        isGravityRunning = true;
        bool moved = true;

        int[] excludeRoles = { BRACE, CORE };

        while (moved)
        {
            moved = false;

            if (ApplyGravityStep(excludeRoles))
            {
                moved = true;
                UpdateCriticalStates();
                yield return new WaitForSeconds(0.13f);
            }
        }

        UpdateCriticalStates();
        CheckBoardState();
        isGravityRunning = false;
    }

    private bool ApplyGravityStep(int[] excludeRoles)
    {
        bool anyMoved = false;

        for (int c = 0; c < columns; c++)
        {
            for (int r = rows - 2; r >= 0; r--)
            {
                if (IsInBounds(c, r) && grid[c, r] != null)
                {
                    int role = roles[c, r];
                    bool isExcluded = false;

                    if (excludeRoles != null)
                    {
                        foreach (int excludedRole in excludeRoles)
                        {
                            if (role == excludedRole)
                            {
                                isExcluded = true;
                                break;
                            }
                        }
                    }

                    if (!isExcluded && IsInBounds(c, r + 1) && grid[c, r + 1] == null && roles[c, r + 1] == -1)
                    {
                        grid[c, r + 1] = grid[c, r];
                        roles[c, r + 1] = roles[c, r];
                        grid[c, r] = null;
                        roles[c, r] = -1;

                        grid[c, r + 1].Init(c, r + 1, roles[c, r + 1]);
                        grid[c, r + 1].MoveTo(GetAnchoredPos(c, r + 1), 0.12f);

                        anyMoved = true;
                    }
                }
            }
        }

        return anyMoved;
    }

    private bool IsInBounds(int c, int r)
    {
        return c >= 0 && c < columns && r >= 0 && r < rows;
    }

    private void CheckBoardState()
    {
        for (int c = 0; c < columns; c++)
        {
            if (IsInBounds(c, rows - 1) && roles[c, rows - 1] == CORE)
            {
                TriggerCollapse();
                return;
            }
        }

        if (safeRemoved >= safeTotal && !collapsed)
        {
            Solve();
        }
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
        if (instructionText != null) instructionText.text = "CORE HIT THE GROUND! STRUCTURE COLLAPSED!";

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
                        a.PlaySet(collapseFrames, 12f, false, () => {
                            if (burstGo != null) Object.Destroy(burstGo);
                        });
        }

        yield return Shake(gridContainer, 0.8f, 20f);
        yield return new WaitForSeconds(0.3f);
        TimeOut();
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
            progressText.text = "SUPPORTS REMOVED: " + safeRemoved + " / " + safeTotal;
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || collapsed || isGravityRunning || grid == null) return;

        List<GridTile> safest = new List<GridTile>();
        List<GridTile> risky = new List<GridTile>();
        List<GridTile> nuggets = new List<GridTile>();

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (grid[c, r] != null)
                {
                    if (roles[c, r] == NORMAL) safest.Add(grid[c, r]);
                    else if (roles[c, r] == CRITICAL) risky.Add(grid[c, r]);
                    else if (roles[c, r] == GOLD_NUGGET) nuggets.Add(grid[c, r]);
                }
            }
        }

        if (nuggets.Count > 0 && rng.Next(0, 10) < 3)
        {
            CollectNugget(nuggets[rng.Next(nuggets.Count)]);
        }
        else if (safest.Count > 0)
        {
            RemoveSafe(safest[rng.Next(safest.Count)]);
        }
        else if (risky.Count > 0)
        {
            foreach (var t in risky)
            {
                bool hasCatch = false;
                for (int rr = t.y + 1; rr < rows; rr++)
                {
                    if (roles[t.x, rr] != -1) { hasCatch = true; break; }
                }
                if (hasCatch) { RemoveSafe(t); return; }
            }
            RemoveSafe(risky[rng.Next(risky.Count)]);
        }
    }
}