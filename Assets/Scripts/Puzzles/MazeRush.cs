using UnityEngine;
using System.Collections.Generic;

// Maze Rush - navigate from a start corner to the opposite corner. Click an
// adjacent FLOOR cell to walk into it. Walls block movement. Solve when the
// goal is reached. Faithful port of maze_rush.gd. World-space (sprites +
// BoxCollider2D + OnMouseDown), matching the project's puzzle conventions.
public class MazeRush : PuzzleBase
{
    [Header("Sprites")]
    public Sprite wallSprite;   // Assets/Textures/Walls/maze_wall.png
    public Sprite floorSprite;  // Assets/Textures/Floors/circuit_floor.png
    public Sprite playerSprite; // Assets/Textures/Effects/player_orb.png
    public Sprite goalSprite;   // Assets/Textures/Effects/exit_portal.png

    private const int WALL = 1;
    private const int FLOOR = 0;
    private const float SPACING = 0.72f;

    private int gridSize = 7;
    private int[,] maze;            // [y, x]
    private MazeCell[,] cellNodes;  // [y, x]
    private Vector2Int playerPos;
    private Vector2Int goalPos;

    public override void Setup(int d)
    {
        base.Setup(d);
        gridSize = Mathf.Clamp(6 + d - 1, 5, 8);
        Build();
    }

    private void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        maze = new int[gridSize, gridSize];
        cellNodes = new MazeCell[gridSize, gridSize];

        // Step 1: start with all walls.
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                maze[y, x] = WALL;

        // Step 2: random pick of start + goal corners for layout variety.
        int cornerChoice = Random.Range(0, 4);
        switch (cornerChoice)
        {
            case 0:
                playerPos = new Vector2Int(0, 0);
                goalPos = new Vector2Int(gridSize - 1, gridSize - 1);
                break;
            case 1:
                playerPos = new Vector2Int(gridSize - 1, 0);
                goalPos = new Vector2Int(0, gridSize - 1);
                break;
            case 2:
                playerPos = new Vector2Int(0, gridSize - 1);
                goalPos = new Vector2Int(gridSize - 1, 0);
                break;
            default:
                playerPos = new Vector2Int(gridSize - 1, gridSize - 1);
                goalPos = new Vector2Int(0, 0);
                break;
        }

        // Step 3: GUARANTEE a path with a biased random walk from start to goal.
        int cx2 = playerPos.x;
        int cy2 = playerPos.y;
        maze[cy2, cx2] = FLOOR;
        int safety = gridSize * gridSize * 4;
        while ((cx2 != goalPos.x || cy2 != goalPos.y) && safety > 0)
        {
            safety--;
            int dx = Sign(goalPos.x - cx2);
            int dy = Sign(goalPos.y - cy2);
            bool moveX = Random.value < 0.5f;
            if (Random.value < 0.4f)
            {
                moveX = Random.Range(0, 2) == 0;
                dx = Random.Range(0, 3) - 1;
                dy = Random.Range(0, 3) - 1;
            }
            int nx = cx2;
            int ny = cy2;
            if (moveX && dx != 0)
                nx = Mathf.Clamp(cx2 + dx, 0, gridSize - 1);
            else if (dy != 0)
                ny = Mathf.Clamp(cy2 + dy, 0, gridSize - 1);
            else if (dx != 0)
                nx = Mathf.Clamp(cx2 + dx, 0, gridSize - 1);
            if (nx == cx2 && ny == cy2)
            {
                // No progress - force a step toward the goal.
                if (dx != 0)
                    nx = Mathf.Clamp(cx2 + dx, 0, gridSize - 1);
                else if (dy != 0)
                    ny = Mathf.Clamp(cy2 + dy, 0, gridSize - 1);
            }
            cx2 = nx;
            cy2 = ny;
            maze[cy2, cx2] = FLOOR;
        }
        maze[goalPos.y, goalPos.x] = FLOOR;

        // Step 4: carve extra random floor pockets.
        int extras = gridSize * gridSize / 3;
        for (int i = 0; i < extras; i++)
        {
            int rx = Random.Range(0, gridSize);
            int ry = Random.Range(0, gridSize);
            Vector2Int rp = new Vector2Int(rx, ry);
            if (rp != playerPos && rp != goalPos)
            {
                if (Random.value < 0.5f)
                    maze[ry, rx] = FLOOR;
            }
        }

        // Step 5: build cells.
        for (int cy = 0; cy < gridSize; cy++)
            for (int cx = 0; cx < gridSize; cx++)
                cellNodes[cy, cx] = MakeCell(cx, cy);
        RefreshAll();
    }

    private int Sign(int v)
    {
        return v > 0 ? 1 : (v < 0 ? -1 : 0);
    }

    private Vector3 CellWorld(int x, int y)
    {
        float startX = -(gridSize - 1) * SPACING / 2f;
        float startY = (gridSize - 1) * SPACING / 2f;
        return new Vector3(startX + x * SPACING, startY - y * SPACING, 0f);
    }

    private MazeCell MakeCell(int x, int y)
    {
        GameObject go = new GameObject("Cell_" + x + "_" + y);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = CellWorld(x, y);

        GameObject terr = new GameObject("Terrain");
        terr.transform.SetParent(go.transform, false);
        SpriteRenderer tr = terr.AddComponent<SpriteRenderer>();
        tr.sortingOrder = 1;

        GameObject ent = new GameObject("Entity");
        ent.transform.SetParent(go.transform, false);
        SpriteRenderer er = ent.AddComponent<SpriteRenderer>();
        er.sortingOrder = 2;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(SPACING, SPACING);

        MazeCell mc = go.AddComponent<MazeCell>();
        mc.terrain = tr;
        mc.entity = er;
        mc.Init(x, y);
        mc.OnClicked += HandleClick;
        return mc;
    }

    private void ApplySprite(SpriteRenderer sr, Sprite sp, float target)
    {
        sr.sprite = sp;
        if (sp != null)
        {
            float s = sp.bounds.size.x;
            if (s > 0.0001f)
                sr.transform.localScale = Vector3.one * (target / s);
        }
    }

    private void RefreshCell(int x, int y)
    {
        MazeCell node = cellNodes[y, x];
        if (node == null) return;

        int v = maze[y, x];
        bool isPlayer = (x == playerPos.x && y == playerPos.y);
        bool isGoal = (x == goalPos.x && y == goalPos.y);

        node.entity.sprite = null;

        if (v == WALL)
        {
            ApplySprite(node.terrain, wallSprite, SPACING * 0.98f);
            node.terrain.color = Color.white;
        }
        else
        {
            ApplySprite(node.terrain, floorSprite, SPACING * 0.98f);
            if (isPlayer)
            {
                node.terrain.color = Color.white;
                ApplySprite(node.entity, playerSprite, SPACING * 0.7f);
                node.entity.color = new Color(0.0f, 0.90f, 1.0f, 1.0f);
            }
            else if (isGoal)
            {
                node.terrain.color = Color.white;
                ApplySprite(node.entity, goalSprite, SPACING * 0.7f);
                node.entity.color = new Color(0.61f, 1.0f, 0.18f, 1.0f);
            }
            else if (IsAdjacentToPlayer(x, y))
            {
                node.terrain.color = new Color(0.55f, 1.0f, 1.15f, 1.0f);
            }
            else
            {
                node.terrain.color = new Color(0.82f, 0.86f, 0.96f, 1.0f);
            }
        }
    }

    private void RefreshAll()
    {
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                RefreshCell(x, y);
    }

    private bool IsAdjacentToPlayer(int x, int y)
    {
        return Mathf.Abs(x - playerPos.x) + Mathf.Abs(y - playerPos.y) == 1;
    }

    private void HandleClick(int x, int y)
    {
        if (!inputEnabled || isSolved) return;
        if (!IsAdjacentToPlayer(x, y)) return;

        if (maze[y, x] == WALL)
        {
            if (AudioManager.Instance != null && GameState.Instance != null)
                AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.6f);
            return;
        }

        playerPos = new Vector2Int(x, y);
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzlePlace, 0.6f);
        RefreshAll();

        if (playerPos == goalPos)
        {
            if (AudioManager.Instance != null && GameState.Instance != null)
                AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.7f);
            Solve(100 + difficulty * 40);
        }
    }

    public void BotSolve()
    {
        int guard = gridSize * gridSize + 5;
        while (!isSolved && guard-- > 0)
        {
            Vector2Int? next = BfsNextStep();
            if (next == null) break;
            BotMove(next.Value.x, next.Value.y);
        }
    }

    private void BotMove(int x, int y)
    {
        if (isSolved) return;
        if (maze[y, x] == WALL) return;
        playerPos = new Vector2Int(x, y);
        RefreshAll();
        if (playerPos == goalPos)
        {
            if (AudioManager.Instance != null && GameState.Instance != null)
                AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.7f);
            Solve(100 + difficulty * 40);
        }
    }

    private Vector2Int? BfsNextStep()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(playerPos);
        cameFrom[playerPos] = playerPos;
        bool found = false;
        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            if (p == goalPos) { found = true; break; }
            foreach (var d in dirs)
            {
                int nx = p.x + d.x;
                int ny = p.y + d.y;
                if (nx < 0 || ny < 0 || nx >= gridSize || ny >= gridSize) continue;
                if (maze[ny, nx] == WALL) continue;
                Vector2Int np = new Vector2Int(nx, ny);
                if (cameFrom.ContainsKey(np)) continue;
                cameFrom[np] = p;
                queue.Enqueue(np);
            }
        }

        if (!found) return null;

        Vector2Int step = goalPos;
        while (cameFrom[step] != playerPos && cameFrom[step] != step)
            step = cameFrom[step];
        return step;
    }
}
