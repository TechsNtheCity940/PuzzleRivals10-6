using System.Collections.Generic;
using UnityEngine;

// Flow-Free style "connect the matching nodes" puzzle rendered as a Canvas overlay.
// Build a gridSize x gridSize board, scatter colour node PAIRS, and let the player
// draw a non-crossing path between each pair. Solved when every pair is connected.
public class LinkLockPuzzle : BasePuzzle
{
    [Header("LinkLock")]
    public int gridSize = 5;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 110f;
    public TMPro.TMP_Text statusText;

    [Header("Art")]
    public Sprite nodeSprite; // icon drawn on node endpoints, tinted by colour
    public Sprite[] wireSprites; // 0: Straight, 1: Corner

    private LinkLockTile[,] _tiles;
private int[,] _nodeColor; // -1 empty, else colorID of a node at this cell
    private int[,] _pathColor; // -1 empty, else colorID of a path segment at this cell
    private Vector2Int[] _nodeA;
    private Vector2Int[] _nodeB;
    private List<Vector2Int>[] _paths;
    private bool[] _connected;
    private int _numColors;
    private int _activeColor = -1;

    private const int MaxPaletteColors = 8;

    protected override void BuildBoard()
    {
        int n = Mathf.Max(2, gridSize);

        // Clear any existing tiles.
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(gridContainer.GetChild(i).gameObject);
            }
        }

        _tiles = new LinkLockTile[n, n];
        _nodeColor = new int[n, n];
        _pathColor = new int[n, n];
        for (int yy = 0; yy < n; yy++)
        {
            for (int xx = 0; xx < n; xx++)
            {
                _nodeColor[xx, yy] = -1;
                _pathColor[xx, yy] = -1;
            }
        }

        float center = (n - 1) * tileSpacing * 0.5f;

        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                LinkLockTile tile = CreateTile(col, row, center);
                _tiles[col, row] = tile;
            }
        }

        // Decide how many colour pairs to place.
        int maxByCells = (n * n) / 2;
        _numColors = Mathf.Clamp(2 + difficulty, 2, Mathf.Min(MaxPaletteColors, maxByCells));

        _nodeA = new Vector2Int[_numColors];
        _nodeB = new Vector2Int[_numColors];
        _paths = new List<Vector2Int>[_numColors];
        _connected = new bool[_numColors];
        _activeColor = -1;

        // Pick distinct random cells for the 2 * _numColors node endpoints.
        List<Vector2Int> cells = new List<Vector2Int>(n * n);
        for (int yy = 0; yy < n; yy++)
        {
            for (int xx = 0; xx < n; xx++)
            {
                cells.Add(new Vector2Int(xx, yy));
            }
        }
        Shuffle(cells);

        int idx = 0;
        for (int c = 0; c < _numColors; c++)
        {
            Vector2Int a = cells[idx++];
            Vector2Int b = cells[idx++];
            _nodeA[c] = a;
            _nodeB[c] = b;
            _connected[c] = false;
            _paths[c] = null;
            _nodeColor[a.x, a.y] = c;
            _nodeColor[b.x, b.y] = c;
            _tiles[a.x, a.y].SetNode(c);
            _tiles[b.x, b.y].SetNode(c);
        }

        UpdateStatus();
    }

    private LinkLockTile CreateTile(int col, int row, float center)
    {
        GameObject go = Object.Instantiate(tilePrefab, gridContainer);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = new Vector2(col * tileSpacing - center, -(row * tileSpacing) + center);
        rt.sizeDelta = new Vector2(tileSpacing, tileSpacing); // Explicit size to prevent sprite bleed

        LinkLockTile tile = go.GetComponent<LinkLockTile>();
        tile.Init(col, row);
        tile.SetNodeSprite(nodeSprite);
        tile.OnClicked += OnTileClicked;
        return tile;
    }

    private void OnTileClicked(LinkLockTile tile)
    {
        if (!CanAcceptInput()) return;

        if (tile.isNode)
        {
            // Clicking the matching far endpoint while a path is active completes it.
            if (_activeColor == tile.colorID && _paths[_activeColor] != null && _paths[_activeColor].Count > 0)
            {
                List<Vector2Int> path = _paths[_activeColor];
                Vector2Int cell = new Vector2Int(tile.x, tile.y);
                if (cell != path[0] && IsAdjacent(path[path.Count - 1], cell))
                {
                    path.Add(cell);
                    UpdatePathWires(_activeColor);
                    CompleteColor(_activeColor);
                    return;
                }
            }

            // Otherwise (re)start this colour's path from the clicked node.
            StartPath(tile);
            return;
        }

        if (_activeColor < 0) return;
        TryExtend(tile);
    }

    private void StartPath(LinkLockTile nodeTile)
    {
        int color = nodeTile.colorID;
        ClearPath(color);
        _connected[color] = false;
        _activeColor = color;
        _paths[color] = new List<Vector2Int> { new Vector2Int(nodeTile.x, nodeTile.y) };
        UpdatePathWires(color);
        UpdateStatus();
    }

    private void TryExtend(LinkLockTile tile)
    {
        List<Vector2Int> path = _paths[_activeColor];
        if (path == null || path.Count == 0) return;

        Vector2Int last = path[path.Count - 1];
        Vector2Int cell = new Vector2Int(tile.x, tile.y);
        if (!IsAdjacent(last, cell)) return;

        // Target must be an empty cell (nodes are handled in OnTileClicked).
        if (_nodeColor[cell.x, cell.y] != -1) return;
        if (_pathColor[cell.x, cell.y] != -1) return;

        _pathColor[cell.x, cell.y] = _activeColor;
        path.Add(cell);
        UpdatePathWires(_activeColor);
    }

    private void UpdatePathWires(int color)
    {
        List<Vector2Int> path = _paths[color];
        if (path == null) return;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int curr = path[i];
            Vector2Int? prev = (i > 0) ? (Vector2Int?)path[i - 1] : null;
            Vector2Int? next = (i < path.Count - 1) ? (Vector2Int?)path[i + 1] : null;

            DetermineWire(prev, curr, next, out Sprite sprite, out float rotation);
            _tiles[curr.x, curr.y].SetWire(color, sprite, rotation);
        }
    }

    private void DetermineWire(Vector2Int? prev, Vector2Int curr, Vector2Int? next, out Sprite sprite, out float rotation)
    {
        sprite = null;
        rotation = 0;

        if (wireSprites == null || wireSprites.Length < 2) return;

        if (prev.HasValue && next.HasValue)
        {
            Vector2Int d1 = curr - prev.Value;
            Vector2Int d2 = next.Value - curr;

            if (d1 == d2)
            {
                // Straight
                sprite = wireSprites[0];
                rotation = (d1.x != 0) ? 90f : 0f;
            }
            else
            {
                // Corner
                sprite = wireSprites[1];
                // Directions relative to curr:
                Vector2Int inDir = prev.Value - curr;
                Vector2Int outDir = next.Value - curr;

                // Sum of dirs tells us which quadrant the corner is in
                Vector2Int sum = inDir + outDir;
                if (sum == new Vector2Int(1, 1)) rotation = 180f;      // Right + Top
                else if (sum == new Vector2Int(1, -1)) rotation = 90f; // Right + Bottom
                else if (sum == new Vector2Int(-1, -1)) rotation = 0f; // Left + Bottom
                else if (sum == new Vector2Int(-1, 1)) rotation = 270f; // Left + Top
            }
        }
        else if (prev.HasValue || next.HasValue)
        {
            // Endpoints
            sprite = wireSprites[0];
            Vector2Int d = prev.HasValue ? (curr - prev.Value) : (next.Value - curr);
            rotation = (d.x != 0) ? 90f : 0f;
        }
    }

    private void CompleteColor(int color)
    {
        _connected[color] = true;
        _activeColor = -1;
        
        // Pulse the endpoints
        _tiles[_nodeA[color].x, _nodeA[color].y].GetComponent<GridTile>()?.Pulse(1.4f, 0.3f);
        _tiles[_nodeB[color].x, _nodeB[color].y].GetComponent<GridTile>()?.Pulse(1.4f, 0.3f);

        UpdateStatus();
        CheckSolved();
    }

    private void ClearPath(int color)
    {
        if (_paths[color] == null) return;
        foreach (Vector2Int cell in _paths[color])
        {
            if (_nodeColor[cell.x, cell.y] != -1) continue; // never clear node cells
            _pathColor[cell.x, cell.y] = -1;
            _tiles[cell.x, cell.y].Clear();
        }
        _paths[color] = null;
    }

    private void ClearAllUnconnectedPaths()
    {
        for (int c = 0; c < _numColors; c++)
        {
            if (!_connected[c]) ClearPath(c);
        }
    }

    private void CheckSolved()
    {
        for (int c = 0; c < _numColors; c++)
        {
            if (!_connected[c]) return;
        }
        if (statusText != null) statusText.text = "SOLVED!";
        Solve();
    }

    private void UpdateStatus()
    {
        if (statusText == null) return;
        int done = 0;
        for (int c = 0; c < _numColors; c++)
        {
            if (_connected[c]) done++;
        }
        statusText.text = "CONNECTED: " + done + "/" + _numColors;
    }

    public override void AutoSolveChunk()
    {
        for (int c = 0; c < _numColors; c++)
        {
            if (_connected[c]) continue;
            if (TryAutoConnect(c)) break;
        }
    }

    private bool TryAutoConnect(int color)
    {
        ClearPath(color);
        List<Vector2Int> route = Bfs(color);
        if (route == null)
        {
            // Free up partial / unfinished paths and try once more.
            ClearAllUnconnectedPaths();
            route = Bfs(color);
            if (route == null) return false;
        }

        foreach (Vector2Int cell in route)
        {
            if (_nodeColor[cell.x, cell.y] == color) continue; // skip the two node cells
            _pathColor[cell.x, cell.y] = color;
        }
        _paths[color] = route;
        UpdatePathWires(color);
        CompleteColor(color);
        return true;
    }

    // Shortest path from nodeA to nodeB over empty cells (goal node allowed).
    private List<Vector2Int> Bfs(int color)
    {
        int n = _tiles.GetLength(0);
        Vector2Int start = _nodeA[color];
        Vector2Int goal = _nodeB[color];

        bool[,] visited = new bool[n, n];
        Vector2Int[,] cameFrom = new Vector2Int[n, n];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
        };

        bool found = false;
        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            if (cur == goal)
            {
                found = true;
                break;
            }
            foreach (Vector2Int d in dirs)
            {
                Vector2Int nb = cur + d;
                if (nb.x < 0 || nb.x >= n || nb.y < 0 || nb.y >= n) continue;
                if (visited[nb.x, nb.y]) continue;

                bool isGoal = nb == goal;
                bool passable = isGoal || (_nodeColor[nb.x, nb.y] == -1 && _pathColor[nb.x, nb.y] == -1);
                if (!passable) continue;

                visited[nb.x, nb.y] = true;
                cameFrom[nb.x, nb.y] = cur;
                queue.Enqueue(nb);
            }
        }

        if (!found) return null;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int node = goal;
        path.Add(node);
        while (node != start)
        {
            node = cameFrom[node.x, node.y];
            path.Add(node);
        }
        path.Reverse();
        return path;
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
