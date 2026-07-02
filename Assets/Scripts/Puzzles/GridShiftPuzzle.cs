using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GridShiftPuzzle : BasePuzzle
{
    [Header("Grid Shift")]
    public int gridSize = 4;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public RectTransform targetGridContainer; // Optional: shows the target state
    public float tileSpacing = 110f;
    public float targetScale = 0.5f;

    [Header("Art")]
    public Sprite[] tileFrames; 
    public Sprite[] moveAnimation;
    public Sprite emptyTileSprite;

    private GridTile[,] _grid;
    private int[,] _board; 
    private Vector2Int _emptyPos;

    protected override void BuildBoard()
    {
        int n = Mathf.Clamp(gridSize, 3, 4);
        _grid = new GridTile[n, n];
        _board = new int[n, n];

        ClearContainer(gridContainer);
        ClearContainer(targetGridContainer);

        // Initialize solved state
        List<int> tiles = new List<int>();
        for (int i = 0; i < n * n - 1; i++) tiles.Add(i);
        tiles.Add(-1); // Empty slot

        // Shuffle
        Shuffle(tiles, n);

        float center = (n - 1) * tileSpacing * 0.5f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                int val = tiles[x + y * n];
                _board[x, y] = val;

                GameObject go = Instantiate(tilePrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(x * tileSpacing - center, -(y * tileSpacing) + center);
                // Tiles must fill their cell; without this they render at the
                // GridTile prefab's default size and the picture won't line up.
                rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

                GridTile gt = go.GetComponent<GridTile>();
                gt.Init(x, y, val);
                _grid[x, y] = gt;

                if (val == -1)
                {
                    _emptyPos = new Vector2Int(x, y);
                    if (emptyTileSprite != null) gt.SetSprite(emptyTileSprite);
                    else gt.SetColor(new Color(0, 0, 0, 0));
                }
                else
                {
                    if (tileFrames != null && val < tileFrames.Length)
                        gt.SetSprite(tileFrames[val]);
                    gt.OnClicked += OnTileClicked;
                }
            }
        }

        // Build target display if container exists
        if (targetGridContainer != null)
        {
            float tSpacing = tileSpacing * targetScale;
            float tCenter = (n - 1) * tSpacing * 0.5f;

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    int val = (x + y * n == n * n - 1) ? -1 : (x + y * n);
                    GameObject go = Instantiate(tilePrefab, targetGridContainer);
                    RectTransform rt = (RectTransform)go.transform;
                    rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);
                    rt.localScale = Vector3.one * targetScale;
                    rt.anchoredPosition = new Vector2(x * tSpacing - tCenter, -(y * tSpacing) + tCenter);

                    GridTile gt = go.GetComponent<GridTile>();
                    gt.Init(x, y, val);
                    
                    if (val == -1)
                    {
                        if (emptyTileSprite != null) gt.SetSprite(emptyTileSprite);
                        else gt.SetColor(new Color(0, 0, 0, 0));
                    }
                    else
                    {
                        if (tileFrames != null && val < tileFrames.Length)
                            gt.SetSprite(tileFrames[val]);
                    }
                }
            }
        }
    }

    private void ClearContainer(RectTransform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    private void Shuffle(List<int> tiles, int n)
    {
        // To ensure solvability, we start from solved and make random moves
        int[] board = new int[n * n];
        for (int i = 0; i < n * n - 1; i++) board[i] = i;
        board[n * n - 1] = -1;

        int ex = n - 1;
        int ey = n - 1;

        int moves = 50 + difficulty * 10;
        for (int i = 0; i < moves; i++)
        {
            int dir = rng.Next(0, 4);
            int nx = ex, ny = ey;
            if (dir == 0) nx++;
            else if (dir == 1) nx--;
            else if (dir == 2) ny++;
            else if (dir == 3) ny--;

            if (nx >= 0 && nx < n && ny >= 0 && ny < n)
            {
                board[ex + ey * n] = board[nx + ny * n];
                board[nx + ny * n] = -1;
                ex = nx;
                ey = ny;
            }
        }

        for (int i = 0; i < n * n; i++) tiles[i] = board[i];
    }

    private void OnTileClicked(GridTile tile)
    {
        if (!CanAcceptInput()) return;

        Vector2Int pos = new Vector2Int(tile.x, tile.y);
        if (IsAdjacent(pos, _emptyPos))
        {
            Swap(pos, _emptyPos);
            CheckSolved();
        }
    }

    private void Swap(Vector2Int p1, Vector2Int p2)
    {
        int v1 = _board[p1.x, p1.y];
        int v2 = _board[p2.x, p2.y];

        _board[p1.x, p1.y] = v2;
        _board[p2.x, p2.y] = v1;

        GridTile t1 = _grid[p1.x, p1.y];
        GridTile t2 = _grid[p2.x, p2.y];

        // Animate t1 (the clicked tile) sliding to the empty spot t2
        StartCoroutine(SlideTile(t1, p2));
        
        // Logic swap
        _grid[p1.x, p1.y] = t2;
        _grid[p2.x, p2.y] = t1;
        
        t1.x = p2.x; t1.y = p2.y;
        t2.x = p1.x; t2.y = p1.y;

        if (v1 == -1) _emptyPos = p1;
        else if (v2 == -1) _emptyPos = p2;
    }

    private IEnumerator SlideTile(GridTile tile, Vector2Int targetCoord)
    {
        RectTransform rt = (RectTransform)tile.transform;
        Vector2 start = rt.anchoredPosition;
        int n = _grid.GetLength(0);
        float center = (n - 1) * tileSpacing * 0.5f;
        Vector2 target = new Vector2(targetCoord.x * tileSpacing - center, -(targetCoord.y * tileSpacing) + center);
        
        float duration = 0.15f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        rt.anchoredPosition = target;
    }

    private void CheckSolved()
    {
        int n = _grid.GetLength(0);
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                int expected = (x + y * n == n * n - 1) ? -1 : (x + y * n);
                if (_board[x, y] != expected) return;
            }
        }
        Solve();
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    public override void AutoSolveChunk()
    {
        // Greedily pick the move that reduces total Manhattan distance to solved state.
        int n = _grid.GetLength(0);
        List<Vector2Int> adj = new List<Vector2Int>();
        if (_emptyPos.x > 0) adj.Add(new Vector2Int(_emptyPos.x - 1, _emptyPos.y));
        if (_emptyPos.x < n - 1) adj.Add(new Vector2Int(_emptyPos.x + 1, _emptyPos.y));
        if (_emptyPos.y > 0) adj.Add(new Vector2Int(_emptyPos.x, _emptyPos.y - 1));
        if (_emptyPos.y < n - 1) adj.Add(new Vector2Int(_emptyPos.x, _emptyPos.y + 1));

        Vector2Int bestMove = adj[0];
        int minDistance = int.MaxValue;

        foreach (var move in adj)
        {
            // Simulate swap
            int tileVal = _board[move.x, move.y];
            _board[_emptyPos.x, _emptyPos.y] = tileVal;
            _board[move.x, move.y] = -1;

            int dist = CalculateTotalDistance(n);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestMove = move;
            }

            // Revert swap
            _board[move.x, move.y] = tileVal;
            _board[_emptyPos.x, _emptyPos.y] = -1;
        }

        // If no move reduces distance, pick a random one to avoid getting stuck
        if (minDistance >= CalculateTotalDistance(n))
        {
            bestMove = adj[rng.Next(adj.Count)];
        }

        Swap(bestMove, _emptyPos);
        CheckSolved();
    }

    private int CalculateTotalDistance(int n)
    {
        int total = 0;
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                int val = _board[x, y];
                if (val == -1) continue;

                int targetX = val % n;
                int targetY = val / n;
                total += Mathf.Abs(x - targetX) + Mathf.Abs(y - targetY);
            }
        }
        return total;
    }
}