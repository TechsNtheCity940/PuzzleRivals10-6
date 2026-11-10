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
    public Sprite portalSprite; // neon vortex icon for warp sockets
    public Sprite batterySprite; // neon battery icon for the bonus cell
    public Sprite[] wireSprites; // fallback static sprites: 0 Straight, 1 Corner
    public Sprite[] straightFlowFrames; // animated electrical-flow straight segment
    public Sprite[] cornerFlowFrames;   // animated electrical-flow corner segment
    public float wireFlowFps = 14f;

    private LinkLockTile[,] _tiles;
    private int[,] _nodeColor; // -1 empty, else colorID of a node at this cell
    private int[,] _pathColor; // -1 empty, else colorID of a path segment at this cell
    private Vector2Int[] _nodeA;
    private Vector2Int[] _nodeB;
    private List<Vector2Int>[] _paths;
    private bool[] _connected;
    private int _numColors;
    private int _activeColor = -1;

    private Vector2Int portalAPos = new Vector2Int(-1, -1);
    private Vector2Int portalBPos = new Vector2Int(-1, -1);
    private Vector2Int batteryPos = new Vector2Int(-1, -1);
    private bool batteryClaimed = false;

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

        // Place Warp Sockets (Quantum Bridge Portals) & Battery Cell from remaining empty cells
        portalAPos = new Vector2Int(-1, -1);
        portalBPos = new Vector2Int(-1, -1);
        batteryPos = new Vector2Int(-1, -1);
        batteryClaimed = false;

        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int yy = 0; yy < n; yy++)
        {
            for (int xx = 0; xx < n; xx++)
            {
                if (_nodeColor[xx, yy] == -1)
                {
                    emptyCells.Add(new Vector2Int(xx, yy));
                }
            }
        }
        Shuffle(emptyCells);

        if (emptyCells.Count >= 3)
        {
            portalAPos = emptyCells[0];
            portalBPos = emptyCells[1];
            batteryPos = emptyCells[2];

            _tiles[portalAPos.x, portalAPos.y].SetPortal(0);
            _tiles[portalBPos.x, portalBPos.y].SetPortal(1);
            _tiles[batteryPos.x, batteryPos.y].SetBattery();
        }

        UpdateStatus();
    }

    private LinkLockTile CreateTile(int col, int row, float center)
    {
        GameObject go = Object.Instantiate(tilePrefab, gridContainer);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = new Vector2(col * tileSpacing - center, -(row * tileSpacing) + center);
        rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

        LinkLockTile tile = go.GetComponent<LinkLockTile>();
        tile.Init(col, row);
        tile.SetNodeSprite(nodeSprite);
        tile.portalSprite = portalSprite;
        tile.batterySprite = batterySprite;
        tile.OnClicked += (UITile t) => OnTileClicked((LinkLockTile)t);
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

        // Quantum Bridge Sockets Warp teleport logic
        if (cell == portalAPos && portalBPos.x != -1)
        {
            if (_pathColor[portalBPos.x, portalBPos.y] == -1)
            {
                _pathColor[portalBPos.x, portalBPos.y] = _activeColor;
                path.Add(portalBPos);
                _tiles[portalBPos.x, portalBPos.y].Pulse(1.2f, 0.15f);
            }
        }
        else if (cell == portalBPos && portalAPos.x != -1)
        {
            if (_pathColor[portalAPos.x, portalAPos.y] == -1)
            {
                _pathColor[portalAPos.x, portalAPos.y] = _activeColor;
                path.Add(portalAPos);
                _tiles[portalAPos.x, portalAPos.y].Pulse(1.2f, 0.15f);
            }
        }

        UpdatePathWires(_activeColor);
    }

    private void UpdatePathWires(int color)
    {
        List<Vector2Int> path = _paths[color];
        if (path == null) return;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int curr = path[i];

            // Never draw a wire stub over a node endpoint: keep its glowing hex
            // icon visible. The adjacent wire cell overshoots its bounds and laps
            // into the node cell, so the connection still reads as continuous.
            if (_nodeColor[curr.x, curr.y] != -1) continue;

            Vector2Int? prev = (i > 0) ? (Vector2Int?)path[i - 1] : null;
            Vector2Int? next = (i < path.Count - 1) ? (Vector2Int?)path[i + 1] : null;

            DetermineWire(prev, curr, next, out bool isCorner, out float rotation);
            Sprite[] frames = isCorner ? cornerFlowFrames : straightFlowFrames;
            Sprite fallback = (wireSprites != null && wireSprites.Length >= 2)
                ? (isCorner ? wireSprites[1] : wireSprites[0]) : null;
            _tiles[curr.x, curr.y].SetWire(color, frames, fallback, wireFlowFps, rotation);
        }
    }

    // Computes which wire sprite + rotation to render for a cell, working entirely
    // in SCREEN space so it matches the generated art exactly:
    //   wireSprites[0] (straight) at 0deg = horizontal bar (connects LEFT & RIGHT)
    //   wireSprites[1] (corner)   at 0deg = elbow connecting LEFT & DOWN(screen)
    // Grid y grows downward on screen (tiles use -(row*spacing)), so a grid delta
    // (0,+1) points DOWN and (0,-1) points UP on screen.
    private void DetermineWire(Vector2Int? prev, Vector2Int curr, Vector2Int? next, out bool isCorner, out float rotation)
    {
        isCorner = false;
        rotation = 0;

        // Collect the screen-space connection directions for this cell. Warp
        // (non-adjacent) neighbours are skipped so a warp endpoint renders as a
        // straight stub toward its real adjacent neighbour.
        bool left = false, right = false, up = false, down = false;
        AddDir(prev, curr, ref left, ref right, ref up, ref down);
        AddDir(next, curr, ref left, ref right, ref up, ref down);

        int count = (left ? 1 : 0) + (right ? 1 : 0) + (up ? 1 : 0) + (down ? 1 : 0);

        // Endpoint / stub: render a straight aligned to the single connection.
        if (count <= 1)
        {
            rotation = (left || right) ? 0f : 90f;
            return;
        }

        // Two opposite directions -> straight.
        if ((left && right) || (up && down))
        {
            rotation = (left && right) ? 0f : 90f;
            return;
        }

        // Two perpendicular directions -> corner. Base (0deg) = LEFT+DOWN.
        // Each +90deg CCW rotates LEFT->DOWN->RIGHT->UP.
        isCorner = true;
        if (left && down) rotation = 0f;
        else if (down && right) rotation = 90f;
        else if (right && up) rotation = 180f;
        else /* up && left */ rotation = 270f;
    }

    // Adds the screen-space direction from curr toward an adjacent neighbour.
    private void AddDir(Vector2Int? neighbor, Vector2Int curr, ref bool left, ref bool right, ref bool up, ref bool down)
    {
        if (!neighbor.HasValue) return;
        if (!IsAdjacent(curr, neighbor.Value)) return; // skip warp jumps
        Vector2Int d = neighbor.Value - curr;          // grid delta
        if (d.x == 1) right = true;
        else if (d.x == -1) left = true;
        else if (d.y == 1) down = true;                // grid +y is screen-down
        else if (d.y == -1) up = true;
    }

    private void CompleteColor(int color)
    {
        _connected[color] = true;
        _activeColor = -1;
        
        // Pulse the endpoints
        _tiles[_nodeA[color].x, _nodeA[color].y].Pulse(1.4f, 0.3f);
        _tiles[_nodeB[color].x, _nodeB[color].y].Pulse(1.4f, 0.3f);

        // Check if the path contains the batteryPos and claim the reward
        if (batteryPos.x != -1 && !batteryClaimed)
        {
            List<Vector2Int> path = _paths[color];
            if (path != null && path.Contains(batteryPos))
            {
                batteryClaimed = true;
                _tiles[batteryPos.x, batteryPos.y].batteryCharged = true;
                _tiles[batteryPos.x, batteryPos.y].RefreshVisual();
                _tiles[batteryPos.x, batteryPos.y].Pulse(1.5f, 0.3f);

                timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 5f);
                Debug.Log("LinkLock Battery Charged! +5 Seconds Bonus!");
            }
        }

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

                // Portal teleportation traversing inside pathfinding
                if (nb == portalAPos && portalBPos.x != -1)
                {
                    if (!visited[portalBPos.x, portalBPos.y])
                    {
                        visited[nb.x, nb.y] = true;
                        cameFrom[nb.x, nb.y] = cur;

                        visited[portalBPos.x, portalBPos.y] = true;
                        cameFrom[portalBPos.x, portalBPos.y] = nb; // Portal B came from Portal A
                        queue.Enqueue(portalBPos);
                    }
                }
                else if (nb == portalBPos && portalAPos.x != -1)
                {
                    if (!visited[portalAPos.x, portalAPos.y])
                    {
                        visited[nb.x, nb.y] = true;
                        cameFrom[nb.x, nb.y] = cur;

                        visited[portalAPos.x, portalAPos.y] = true;
                        cameFrom[portalAPos.x, portalAPos.y] = nb; // Portal A came from Portal B
                        queue.Enqueue(portalAPos);
                    }
                }
                else
                {
                    visited[nb.x, nb.y] = true;
                    cameFrom[nb.x, nb.y] = cur;
                    queue.Enqueue(nb);
                }
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
