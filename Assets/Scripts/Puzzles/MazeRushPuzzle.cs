using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Maze Rush (UI overlay). A fresh perfect maze is generated every load via a
// recursive-backtracker, giving a different layout each time. Navigate from the
// Start to the Exit by clicking orthogonally-adjacent floor tiles. The route to
// the Exit is sealed by a locked GATE; reaching the CHECKPOINT (key) opens the
// gate, clearing the way to the Exit.
public class MazeRushPuzzle : BasePuzzle
{
    [Header("Maze Rush")]
    public int gridSize = 9;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 60f;
    public RectTransform beaconArrow;
    public TMPro.TMP_Text checkpointText;

    [Header("Art (optional, null = flat colour)")]
    public Sprite[] wallFrames;
    public Sprite[] floorFrames;
    public Sprite[] startFrames;
    public Sprite[] exitFrames;
    public Sprite[] checkpointFrames;
    public Sprite[] playerFrames;

    private const int WALL = 1;
    private const int FLOOR = 0;

    private static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    private int boardSize;
    private float spacing;
    private int[,] maze;            // [y, x]
    private MazeRushTile[,] tiles;  // [y, x]
    private Vector2Int playerPos;
    private Vector2Int startPos;
    private Vector2Int exitPos;
    private Vector2Int checkpointPos;  // the key
    private Vector2Int gatePos;        // locked door sealing the exit route
    private Vector2Int portalAPos = new Vector2Int(-1, -1);
    private Vector2Int portalBPos = new Vector2Int(-1, -1);
    private bool justTeleported = false;
    private bool checkpointCollected;
    private bool gateOpen;
    private RectTransform playerIcon;

    // ----------------------------------------------------------------- build
    protected override void BuildBoard()
    {
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }

        // Odd board size so cells (odd indices) are separated by wall columns.
        int cells = Mathf.Clamp(3 + difficulty / 2, 3, 5);
        boardSize = cells * 2 + 1;
        
        float containerSize = 1000f;
        if (gridContainer != null)
        {
            containerSize = Mathf.Min(gridContainer.rect.width, gridContainer.rect.height) * 0.9f;
        }
        spacing = containerSize / boardSize;

        maze = new int[boardSize, boardSize];
        tiles = new MazeRushTile[boardSize, boardSize];
        checkpointCollected = false;
        gateOpen = false;

        for (int y = 0; y < boardSize; y++)
            for (int x = 0; x < boardSize; x++)
                maze[y, x] = WALL;

        startPos = new Vector2Int(1, 1);
        exitPos = new Vector2Int(boardSize - 2, boardSize - 2);

        CarveMaze(startPos);

        List<Vector2Int> path = SolvePath(startPos, exitPos);
        if (path == null || path.Count < 4)
        {
            path = new List<Vector2Int>();
            for (int i = 1; i < boardSize - 1; i++) { maze[1, i] = FLOOR; path.Add(new Vector2Int(i, 1)); }
            for (int i = 1; i < boardSize - 1; i++) { maze[i, boardSize - 2] = FLOOR; path.Add(new Vector2Int(boardSize - 2, i)); }
            startPos = path[0]; exitPos = path[path.Count - 1];
        }

        gatePos = path[path.Count - 2];
        int cpIndex = Mathf.Clamp(path.Count / 2, 1, path.Count - 3);
        checkpointPos = path[cpIndex];

        // Gather list of eligible floor tiles for Warp Portals
        List<Vector2Int> eligibleFloors = new List<Vector2Int>();
        for (int y = 1; y < boardSize - 1; y++)
        {
            for (int x = 1; x < boardSize - 1; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (maze[y, x] == FLOOR &&
                    pos != startPos &&
                    pos != exitPos &&
                    pos != gatePos &&
                    pos != checkpointPos)
                {
                    eligibleFloors.Add(pos);
                }
            }
        }

        portalAPos = new Vector2Int(-1, -1);
        portalBPos = new Vector2Int(-1, -1);
        justTeleported = false;

        List<Vector2Int> leftFloors = eligibleFloors.FindAll(p => p.x < boardSize / 2);
        List<Vector2Int> rightFloors = eligibleFloors.FindAll(p => p.x > boardSize / 2);

        if (leftFloors.Count > 0 && rightFloors.Count > 0)
        {
            portalAPos = leftFloors[rng.Next(leftFloors.Count)];
            portalBPos = rightFloors[rng.Next(rightFloors.Count)];
        }
        else if (eligibleFloors.Count >= 2)
        {
            portalAPos = eligibleFloors[0];
            portalBPos = eligibleFloors[eligibleFloors.Count - 1];
        }

        float center = (boardSize - 1) * spacing / 2f;
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                GameObject go = Instantiate(tilePrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(x * spacing - center, -(y * spacing) + center);
                rt.sizeDelta = new Vector2(spacing, spacing);

                MazeRushTile tile = go.GetComponent<MazeRushTile>();
                Vector2Int gp = new Vector2Int(x, y);
                tile.SetFrames(wallFrames, floorFrames, startFrames, exitFrames, checkpointFrames, playerFrames);
                tile.Init(x, y);
                tile.SetWall(maze[y, x] == WALL);
                tile.SetStart(gp == startPos);
                tile.SetExit(gp == exitPos);
                tile.SetCheckpoint(gp == checkpointPos);
                if (gp == gatePos) tile.SetGate(true, true);
                if (gp == portalAPos) tile.SetPortal(true, 0);
                else if (gp == portalBPos) tile.SetPortal(true, 1);
                tile.OnClicked += HandleTileClicked;
                tiles[y, x] = tile;
            }
        }

        playerPos = startPos;
        CreatePlayerIcon();
        RefreshPlayer();
        UpdateBeacon();
        UpdateCheckpointText();
    }

    private void CreatePlayerIcon()
    {
        if (playerIcon != null) Destroy(playerIcon.gameObject);
        
        GameObject go = new GameObject("PlayerIcon", typeof(RectTransform), typeof(Image));
        playerIcon = go.GetComponent<RectTransform>();
        playerIcon.SetParent(gridContainer, false);
        playerIcon.sizeDelta = new Vector2(spacing * 0.8f, spacing * 0.8f);
        
        Image img = go.GetComponent<Image>();
        img.color = new Color(0.3f, 0.95f, 1f, 1f);
        if (playerFrames != null && playerFrames.Length > 0)
        {
            img.sprite = playerFrames[0];
            var anim = go.AddComponent<UISpriteSheetAnimator>();
            anim.PlaySet(playerFrames, 12f, true);
        }
        
        float center = (boardSize - 1) * spacing / 2f;
        playerIcon.anchoredPosition = new Vector2(playerPos.x * spacing - center, -(playerPos.y * spacing) + center);
    }

    private void CarveMaze(Vector2Int start)
    {
        var stack = new Stack<Vector2Int>();
        maze[start.y, start.x] = FLOOR;
        stack.Push(start);

        var step = new Vector2Int[] {
            new Vector2Int(2, 0), new Vector2Int(-2, 0),
            new Vector2Int(0, 2), new Vector2Int(0, -2)
        };

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Peek();
            var nbrs = new List<Vector2Int>();
            foreach (var s in step)
            {
                Vector2Int n = cur + s;
                if (n.x <= 0 || n.y <= 0 || n.x >= boardSize - 1 || n.y >= boardSize - 1) continue;
                if (maze[n.y, n.x] == WALL) nbrs.Add(n);
            }
            if (nbrs.Count == 0) { stack.Pop(); continue; }

            Vector2Int chosen = nbrs[rng.Next(nbrs.Count)];
            Vector2Int mid = cur + new Vector2Int((chosen.x - cur.x) / 2, (chosen.y - cur.y) / 2);
            maze[mid.y, mid.x] = FLOOR;
            maze[chosen.y, chosen.x] = FLOOR;
            stack.Push(chosen);
        }
    }

    private List<Vector2Int> SolvePath(Vector2Int from, Vector2Int to)
    {
        var came = new Dictionary<Vector2Int, Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(from); came[from] = from;
        bool found = false;
        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            if (p == to) { found = true; break; }
            foreach (var d in Dirs)
            {
                Vector2Int np = p + d;
                if (np.x < 0 || np.y < 0 || np.x >= boardSize || np.y >= boardSize) continue;
                if (maze[np.y, np.x] == WALL) continue;
                if (came.ContainsKey(np)) continue;
                came[np] = p;
                q.Enqueue(np);
            }
        }
        if (!found) return null;
        var path = new List<Vector2Int>();
        Vector2Int c = to;
        while (c != from) { path.Add(c); c = came[c]; }
        path.Add(from);
        path.Reverse();
        return path;
    }

    private void HandleTileClicked(MazeRushTile tile)
    {
        if (!CanAcceptInput()) return;
        if (tile == null) return;

        Vector2Int gp = new Vector2Int(tile.x, tile.y);
        if (!IsAdjacent(gp, playerPos)) return;
        if (!Passable(gp)) return;

        MoveTo(gp);
    }

    private bool Passable(Vector2Int gp)
    {
        if (gp.x < 0 || gp.y < 0 || gp.x >= boardSize || gp.y >= boardSize) return false;
        if (maze[gp.y, gp.x] == WALL) return false;
        if (gp == gatePos && !gateOpen) return false;
        return true;
    }

    private void MoveTo(Vector2Int gp)
    {
        playerPos = gp;

        bool teleportedNow = false;
        Vector2Int originalTarget = gp;

        if (gp == portalAPos && !justTeleported)
        {
            playerPos = portalBPos;
            teleportedNow = true;
            justTeleported = true;
        }
        else if (gp == portalBPos && !justTeleported)
        {
            playerPos = portalAPos;
            teleportedNow = true;
            justTeleported = true;
        }
        else if (gp != portalAPos && gp != portalBPos)
        {
            justTeleported = false;
        }

        float center = (boardSize - 1) * spacing / 2f;
        
        if (playerIcon != null)
        {
            if (teleportedNow)
            {
                Vector2 midPos = new Vector2(originalTarget.x * spacing - center, -(originalTarget.y * spacing) + center);
                Vector2 finalPos = new Vector2(playerPos.x * spacing - center, -(playerPos.y * spacing) + center);
                StartCoroutine(WarpMoveSequence(playerIcon, midPos, finalPos));
            }
            else
            {
                Vector2 targetPos = new Vector2(playerPos.x * spacing - center, -(playerPos.y * spacing) + center);
                StartCoroutine(SmoothMove(playerIcon, targetPos, 0.15f));
            }
        }

        if (playerPos == checkpointPos && !checkpointCollected)
        {
            checkpointCollected = true;
            gateOpen = true;
            if (tiles[playerPos.y, playerPos.x] != null) tiles[playerPos.y, playerPos.x].SetCheckpoint(false);
            if (tiles[gatePos.y, gatePos.x] != null) tiles[gatePos.y, gatePos.x].SetGate(true, false);
        }

        RefreshPlayer();
        UpdateBeacon();
        UpdateCheckpointText();

        if (playerPos == exitPos && checkpointCollected)
            Solve();
    }

    private IEnumerator SmoothMove(RectTransform rt, Vector2 target, float duration)
    {
        Vector2 start = rt.anchoredPosition;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        if (rt != null) rt.anchoredPosition = target;
    }

    private IEnumerator WarpMoveSequence(RectTransform rt, Vector2 midPos, Vector2 finalPos)
    {
        if (rt == null) yield break;

        // 1. Smoothly slide onto the first portal
        yield return StartCoroutine(SmoothMove(rt, midPos, 0.08f));
        if (rt == null) yield break;

        // 2. Shrink down to simulate warp hole entrance
        Vector3 originalScale = rt.localScale;
        float elapsed = 0f;
        float duration = 0.08f;
        while (elapsed < duration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        if (rt == null) yield break;
        rt.localScale = Vector3.zero;

        // 3. Move coordinate position instantly
        rt.anchoredPosition = finalPos;

        // 4. Grow back up at destination portal
        elapsed = 0f;
        while (elapsed < duration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
            yield return null;
        }
        if (rt != null) rt.localScale = originalScale;
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private void RefreshPlayer()
    {
        for (int y = 0; y < boardSize; y++)
            for (int x = 0; x < boardSize; x++)
                if (tiles[y, x] != null)
                    tiles[y, x].SetPlayer(new Vector2Int(x, y) == playerPos);
    }

    private void UpdateBeacon()
    {
        if (beaconArrow == null) return;
        Vector2Int goal = checkpointCollected ? exitPos : checkpointPos;
        Vector2 dir = new Vector2(goal.x - playerPos.x, -(goal.y - playerPos.y));
        if (dir.sqrMagnitude < 0.0001f)
        {
            beaconArrow.localRotation = Quaternion.identity;
            return;
        }
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        beaconArrow.localRotation = Quaternion.Euler(0f, 0f, ang);
    }

    private void UpdateCheckpointText()
    {
        if (checkpointText != null)
            checkpointText.text = checkpointCollected ? "EXIT UNLOCKED - REACH THE EXIT!" : "FIND THE KEY TO OPEN THE GATE";
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;
        Vector2Int target = checkpointCollected ? exitPos : checkpointPos;
        Vector2Int? step = BfsNextStep(playerPos, target);
        if (step.HasValue) MoveTo(step.Value);
    }

    private Vector2Int? BfsNextStep(Vector2Int from, Vector2Int target)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> came = new Dictionary<Vector2Int, Vector2Int>();
        q.Enqueue(from);
        came[from] = from;
        bool found = false;
        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            if (p == target) { found = true; break; }
            foreach (var d in Dirs)
            {
                Vector2Int np = p + d;
                if (!Passable(np)) continue;

                // Portal teleportation traversing inside pathfinding
                if (np == portalAPos) np = portalBPos;
                else if (np == portalBPos) np = portalAPos;

                if (came.ContainsKey(np)) continue;
                came[np] = p;
                q.Enqueue(np);
            }
        }
        if (!found) return null;
        Vector2Int step = target;
        while (came[step] != from && came[step] != step)
            step = came[step];
        if (step == from) return null;
        return step;
    }
}
