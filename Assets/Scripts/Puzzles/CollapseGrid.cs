using UnityEngine;
using System.Collections.Generic;

public class CollapseGrid : PuzzleBase
{
    [Header("Assets")]
    public GameObject cellPrefab;
    public Sprite cellSprite;
    public Sprite[] animFrames;

    private int gridW = 6;
    private int gridH = 6;
    private int numColors = 4;
    private int threshold = 12;
    private int[,] cells;
    private CollapseCell[,] cellNodes;

    private Color[] COLORS = {
        new Color(1.0f, 0.18f, 0.78f), // magenta
        new Color(0.0f, 0.90f, 1.0f),  // cyan
        new Color(0.61f, 1.0f, 0.18f), // lime
        new Color(1.0f, 0.54f, 0.12f), // orange
        new Color(0.78f, 0.35f, 1.0f)  // purple
    };

    public override void Setup(int d)
    {
        base.Setup(d);
        numColors = Mathf.Clamp(3 + (d - 1) / 2, 3, 5);
        int total = gridW * gridH;
        threshold = Mathf.Clamp((int)(total * 0.35f), 8, total - 12);
        BuildBoard();
    }

    private void BuildBoard()
    {
        cells = new int[gridH, gridW];
        cellNodes = new CollapseCell[gridH, gridW];

        for (int y = 0; y < gridH; y++)
            for (int x = 0; x < gridW; x++)
                cells[y, x] = Random.Range(0, numColors);

        for (int i = 0; i < 3; i++)
        {
            for (int y = 0; y < gridH; y++)
            {
                for (int x = 0; x < gridW; x++)
                {
                    if (Random.Range(0, 3) == 0 && x > 0) cells[y, x] = cells[y, x - 1];
                    else if (Random.Range(0, 3) == 0 && y > 0) cells[y, x] = cells[y - 1, x];
                }
            }
        }

        float spacing = 1.0f;
        float startX = -(gridW - 1) * spacing / 2f;
        float startY = (gridH - 1) * spacing / 2f;

        for (int y = 0; y < gridH; y++)
        {
            for (int x = 0; x < gridW; x++)
            {
                Vector3 pos = new Vector3(startX + x * spacing, startY - y * spacing, 0);
                GameObject go = Instantiate(cellPrefab, transform);
                go.transform.localPosition = pos;
                CollapseCell cell = go.GetComponent<CollapseCell>();
                cell.Setup(x, y, cells[y, x], COLORS[cells[y, x]], cellSprite);
                cell.animFrames = animFrames;
                cell.OnClicked += HandleCellClick;
                cellNodes[y, x] = cell;
            }
        }
    }

    private void HandleCellClick(int x, int y)
    {
        if (!inputEnabled || isSolved) return;
        int color = cells[y, x];
        if (color == -1) return;

        List<Vector2Int> cluster = FindCluster(x, y, color);
        if (cluster.Count < 2 && difficulty > 1) return;

        foreach (var p in cluster)
        {
            cells[p.y, p.x] = -1;
            if (cellNodes[p.y, p.x] != null)
            {
                cellNodes[p.y, p.x].Vaporize();
                cellNodes[p.y, p.x] = null;
            }
        }

        if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzlePlace, 0.6f);
        CheckSolved();
    }

    private List<Vector2Int> FindCluster(int sx, int sy, int color)
    {
        List<Vector2Int> cluster = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(new Vector2Int(sx, sy));
        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            if (visited.Contains(p)) continue;
            visited.Add(p);

            if (p.x < 0 || p.y < 0 || p.x >= gridW || p.y >= gridH) continue;
            if (cells[p.y, p.x] != color) continue;

            cluster.Add(p);
            queue.Enqueue(new Vector2Int(p.x + 1, p.y));
            queue.Enqueue(new Vector2Int(p.x - 1, p.y));
            queue.Enqueue(new Vector2Int(p.x, p.y + 1));
            queue.Enqueue(new Vector2Int(p.x, p.y - 1));
        }
        return cluster;
    }

    private void CheckSolved()
    {
        int remaining = 0;
        for (int y = 0; y < gridH; y++)
            for (int x = 0; x < gridW; x++)
                if (cells[y, x] != -1) remaining++;

        if (remaining <= threshold)
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.8f);
            Solve(90 + difficulty * 40);
        }
    }

    public void BotSolve()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int y = 0; y < gridH; y++)
            for (int x = 0; x < gridW; x++)
                if (cells[y, x] != -1) candidates.Add(new Vector2Int(x, y));

        if (candidates.Count > 0)
        {
            Vector2Int pick = candidates[Random.Range(0, candidates.Count)];
            HandleCellClick(pick.x, pick.y);
        }
    }
}
