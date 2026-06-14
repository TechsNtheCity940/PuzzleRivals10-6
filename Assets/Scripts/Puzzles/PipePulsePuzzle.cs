using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Updated PipePulsePuzzle: A flow-based puzzle where tiles must be rotated to
// connect an Emitter on the left to a Receiver on the right. Uses a port-based
// connection system (Top, Right, Bottom, Left) and BFS to trace liquid flow.
public class PipePulsePuzzle : BasePuzzle
{
    [Header("Pipe Pulse")]
    public int columns = 5;
    public int rows = 5;
    public GameObject pipePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 110f;
    public Slider reservoirSlider;

    [Serializable]
    public struct PipeAnimationSet
    {
        public Sprite[] emptyFrames;
        public Sprite[] flowFrames;
        public Sprite fallback;
    }

    [Header("Art")]
    public Sprite basePlateSprite;
    public PipeAnimationSet[] pipeAnimations;
    public Sprite[] pipeSprites; // 0: Straight, 1: Elbow, 2: T-Junction
    public Color emptyColor = Color.white;
    public Color flowColor = new Color(0.3f, 0.6f, 1f);
    public Color winColor = Color.cyan;

    private GridTile[,] grid;
    private int startRow;
    private int endRow;
    private int[] targetRotations; // Target rotations to ensure solvability
    private bool solvingSequence;

    private RectTransform topReservoirMarker;
    private RectTransform bottomReservoirMarker;
    private bool topReservoirFilled;
    private bool bottomReservoirFilled;
    private bool topReservoirBonusClaimed;
    private bool bottomReservoirBonusClaimed;
    private int topReservoirCol;
    private int bottomReservoirCol;

    [Flags]
    private enum Port { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

    private void SetupTileAnimation(GridTile tile, bool isFilled)
    {
        // Set background plate
        if (basePlateSprite != null) tile.Image.sprite = basePlateSprite;

        UnityEngine.UI.Image targetImg = tile.OverlayImage != null ? tile.OverlayImage : tile.Image;
        UISpriteSheetAnimator anim = targetImg.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = targetImg.gameObject.AddComponent<UISpriteSheetAnimator>();

        // Clean up secondary overlay if it's not a Cross-Junction anymore (e.g. from pooling/rebuilding)
        Transform t2 = tile.transform.Find("PipeOverlay2");
        if (t2 != null && tile.type != 3)
        {
            Destroy(t2.gameObject);
        }

        if (tile.type == 3) // Cross-Junction
        {
            // First, animate the main targetImg with straight pipe animations (type 0)
            SetupAnimatorForType(anim, targetImg, 0, isFilled);

            // Ensure we have a second overlay for the vertical crossing pipe
            UnityEngine.UI.Image targetImg2 = null;
            if (t2 == null && tile.OverlayImage != null)
            {
                GameObject go2 = Instantiate(tile.OverlayImage.gameObject, tile.transform);
                go2.name = "PipeOverlay2";
                t2 = go2.transform;
                t2.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            if (t2 != null)
            {
                targetImg2 = t2.GetComponent<UnityEngine.UI.Image>();
                UISpriteSheetAnimator anim2 = targetImg2.GetComponent<UISpriteSheetAnimator>();
                if (anim2 == null) anim2 = targetImg2.gameObject.AddComponent<UISpriteSheetAnimator>();
                SetupAnimatorForType(anim2, targetImg2, 0, isFilled);
            }
        }
        else
        {
            // Standard shape animation (0, 1, 2)
            SetupAnimatorForType(anim, targetImg, tile.type, isFilled);
        }
    }

    private void SetupAnimatorForType(UISpriteSheetAnimator anim, UnityEngine.UI.Image targetImg, int shapeType, bool isFilled)
    {
        if (pipeAnimations != null && shapeType < pipeAnimations.Length)
        {
            var animSet = pipeAnimations[shapeType];
            Sprite[] frames = isFilled ? animSet.flowFrames : animSet.emptyFrames;
            
            if (frames != null && frames.Length > 0)
            {
                if (anim.frames != frames)
                {
                    anim.PlaySet(frames, 12f, true);
                }
                targetImg.color = Color.white;
            }
            else
            {
                anim.Stop();
                if (animSet.fallback != null) targetImg.sprite = animSet.fallback;
                else if (pipeSprites != null && shapeType < pipeSprites.Length) 
                    targetImg.sprite = pipeSprites[shapeType];
                
                targetImg.color = isFilled ? flowColor : emptyColor;
            }
        }
        else
        {
            anim.Stop();
            if (pipeSprites != null && shapeType < pipeSprites.Length)
                targetImg.sprite = pipeSprites[shapeType];
            targetImg.color = isFilled ? flowColor : emptyColor;
        }
    }
    public RectTransform startMarker;
    public RectTransform endMarker;

    protected override void BuildBoard()
    {
        ClearBoard();
        grid = new GridTile[columns, rows];
        targetRotations = new int[columns * rows];
        solvingSequence = false;

        startRow = rng.Next(0, rows);
        endRow = rng.Next(0, rows);

        topReservoirBonusClaimed = false;
        bottomReservoirBonusClaimed = false;

        // Choose columns for reservoirs (avoiding the left/rightmost columns where emitter/receiver are)
        if (columns > 2)
        {
            topReservoirCol = rng.Next(1, columns - 1);
            bottomReservoirCol = rng.Next(1, columns - 1);
            if (topReservoirCol == bottomReservoirCol && columns > 3)
            {
                bottomReservoirCol = (topReservoirCol + 1) % (columns - 2) + 1;
            }
        }
        else
        {
            topReservoirCol = 0;
            bottomReservoirCol = columns - 1;
        }

        if (reservoirSlider != null)
        {
            reservoirSlider.minValue = 0f;
            reservoirSlider.maxValue = columns * rows;
            reservoirSlider.value = 0;
        }

        float originX = (columns - 1) * tileSpacing * 0.5f;
        float originY = (rows - 1) * tileSpacing * 0.5f;

        // Position markers
        if (startMarker != null)
        {
            startMarker.anchoredPosition = new Vector2(-originX - tileSpacing * 0.7f, originY - startRow * tileSpacing);
            startMarker.sizeDelta = new Vector2(tileSpacing * 0.6f, tileSpacing * 0.6f);
        }
        if (endMarker != null)
        {
            endMarker.anchoredPosition = new Vector2(originX + tileSpacing * 0.7f, originY - endRow * tileSpacing);
            endMarker.sizeDelta = new Vector2(tileSpacing * 0.6f, tileSpacing * 0.6f);
        }

        // Spawn Reservoir Markers dynamically using endMarker as a template
        if (endMarker != null && gridContainer != null)
        {
            topReservoirMarker = Instantiate(endMarker, gridContainer.parent);
            topReservoirMarker.name = "TopReservoirMarker";
            topReservoirMarker.anchoredPosition = new Vector2(topReservoirCol * tileSpacing - originX, originY + tileSpacing * 0.8f);
            topReservoirMarker.sizeDelta = new Vector2(tileSpacing * 0.6f, tileSpacing * 0.6f);
            topReservoirMarker.localRotation = Quaternion.Euler(0f, 0f, -90f); // Rotate pointing down/into the reservoir

            UnityEngine.UI.Image topImg = topReservoirMarker.GetComponent<UnityEngine.UI.Image>();
            if (topImg != null) topImg.color = emptyColor;

            bottomReservoirMarker = Instantiate(endMarker, gridContainer.parent);
            bottomReservoirMarker.name = "BottomReservoirMarker";
            bottomReservoirMarker.anchoredPosition = new Vector2(bottomReservoirCol * tileSpacing - originX, -originY - tileSpacing * 0.8f);
            bottomReservoirMarker.sizeDelta = new Vector2(tileSpacing * 0.6f, tileSpacing * 0.6f);
            bottomReservoirMarker.localRotation = Quaternion.Euler(0f, 0f, 90f); // Rotate pointing up/into the reservoir

            UnityEngine.UI.Image bottomImg = bottomReservoirMarker.GetComponent<UnityEngine.UI.Image>();
            if (bottomImg != null) bottomImg.color = emptyColor;
        }

        // 1. Generate 2 solvable paths from left to right
        List<Vector2Int> path1 = GeneratePath(startRow, endRow);
        List<Vector2Int> path2 = GeneratePath(startRow, endRow); 
        
        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>(path1);
        pathSet.UnionWith(path2);

        // 2. Instantiate and configure tiles
        if (pipePrefab != null && gridContainer != null)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    GameObject go = Instantiate(pipePrefab, gridContainer);
                    RectTransform rt = (RectTransform)go.transform;
                    rt.anchoredPosition = new Vector2(x * tileSpacing - originX, originY - y * tileSpacing);
                    rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

                    GridTile tile = go.GetComponent<GridTile>();
                    if (tile == null) continue;

                    int shape = 0;
                    int targetRot = 0;

                    if (pathSet.Contains(new Vector2Int(x, y)))
                    {
                        Port requiredPorts = Port.None;

                        if (path1.Contains(new Vector2Int(x, y)))
                        {
                            int idx = path1.FindIndex(p => p.x == x && p.y == y);
                            Vector2Int prev = (idx == 0) ? new Vector2Int(-1, startRow) : path1[idx - 1];
                            Vector2Int next = (idx == path1.Count - 1) ? new Vector2Int(columns, endRow) : path1[idx + 1];

                            requiredPorts |= GetDirectionPort(prev.x - x, prev.y - y);
                            requiredPorts |= GetDirectionPort(next.x - x, next.y - y);
                        }

                        if (path2.Contains(new Vector2Int(x, y)))
                        {
                            int idx = path2.FindIndex(p => p.x == x && p.y == y);
                            Vector2Int prev = (idx == 0) ? new Vector2Int(-1, startRow) : path2[idx - 1];
                            Vector2Int next = (idx == path2.Count - 1) ? new Vector2Int(columns, endRow) : path2[idx + 1];

                            requiredPorts |= GetDirectionPort(prev.x - x, prev.y - y);
                            requiredPorts |= GetDirectionPort(next.x - x, next.y - y);
                        }

                        // Add connections to the reservoirs if path tiles are on the reservoir columns at the borders
                        if (x == topReservoirCol && y == 0)
                        {
                            requiredPorts |= Port.Top;
                        }
                        if (x == bottomReservoirCol && y == rows - 1)
                        {
                            requiredPorts |= Port.Bottom;
                        }

                        shape = GetShapeForPorts(requiredPorts, out targetRot);
                    }
                    else
                    {
                        // Random decoration tile including T-Junctions and Cross-Junctions
                        shape = rng.Next(0, 4);
                        targetRot = rng.Next(0, 4) * 90;
                    }

                    tile.Init(x, y, shape);

                    // Randomize starting rotation
                    int currentRot;
                    do { currentRot = rng.Next(0, 4) * 90; }
                    while (currentRot == targetRot && pathSet.Count > 1);

                    tile.SetRotation(currentRot);
                    grid[x, y] = tile;
                    targetRotations[x + y * columns] = targetRot;
                    tile.OnClicked += OnTileClicked;

                    SetupTileAnimation(tile, false);
                }
            }
        }

        UpdateFlow(false);
    }

    private void OnTileClicked(GridTile tile)
    {
        if (!CanAcceptInput() || isSolved || solvingSequence) return;
        
        RotateTile(tile);
        UpdateFlow(true);
    }

    private void RotateTile(GridTile tile)
    {
        int currentRot = (int)Mathf.Round(tile.transform.localEulerAngles.z);
        tile.SetRotation((currentRot + 90) % 360);
    }

    private void UpdateFlow(bool checkWin)
    {
        var flowData = TraceFlow();
        HashSet<GridTile> filled = flowData.filled;
        
        foreach (var tile in grid)
        {
            if (tile == null) continue;
            SetupTileAnimation(tile, filled.Contains(tile));
        }

        if (reservoirSlider != null) reservoirSlider.value = filled.Count;

        // Check reservoir filling
        topReservoirFilled = false;
        bottomReservoirFilled = false;

        if (grid != null && topReservoirCol >= 0 && topReservoirCol < columns)
        {
            GridTile topTile = grid[topReservoirCol, 0];
            if (topTile != null && filled.Contains(topTile))
            {
                if ((GetRotatedPorts(topTile) & Port.Top) != 0)
                {
                    topReservoirFilled = true;
                }
            }
        }

        if (grid != null && bottomReservoirCol >= 0 && bottomReservoirCol < columns)
        {
            GridTile bottomTile = grid[bottomReservoirCol, rows - 1];
            if (bottomTile != null && filled.Contains(bottomTile))
            {
                if ((GetRotatedPorts(bottomTile) & Port.Bottom) != 0)
                {
                    bottomReservoirFilled = true;
                }
            }
        }

        if (topReservoirMarker != null)
        {
            UnityEngine.UI.Image img = topReservoirMarker.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = topReservoirFilled ? flowColor : emptyColor;
        }

        if (bottomReservoirMarker != null)
        {
            UnityEngine.UI.Image img = bottomReservoirMarker.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = bottomReservoirFilled ? flowColor : emptyColor;
        }

        // Apply bonus time once per round
        if (topReservoirFilled && !topReservoirBonusClaimed)
        {
            topReservoirBonusClaimed = true;
            timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 5f);
            Debug.Log("Top Reservoir Filled! +5 Seconds Bonus!");
            StartCoroutine(PulseMarker(topReservoirMarker));
        }

        if (bottomReservoirFilled && !bottomReservoirBonusClaimed)
        {
            bottomReservoirBonusClaimed = true;
            timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 5f);
            Debug.Log("Bottom Reservoir Filled! +5 Seconds Bonus!");
            StartCoroutine(PulseMarker(bottomReservoirMarker));
        }

        if (checkWin && flowData.reachedEnd)
        {
            StartCoroutine(SolveSequence(flowData.path));
        }
    }

    private struct FlowResult
    {
        public HashSet<GridTile> filled;
        public List<GridTile> path;
        public bool reachedEnd;
    }

    private FlowResult TraceFlow()
    {
        HashSet<GridTile> filled = new HashSet<GridTile>();
        Queue<GridTile> queue = new Queue<GridTile>();
        Dictionary<GridTile, GridTile> parentMap = new Dictionary<GridTile, GridTile>();

        // Start from Emitter at (-1, startRow)
        GridTile startNode = grid[0, startRow];
        if (HasPort(startNode, Port.Left))
        {
            queue.Enqueue(startNode);
            filled.Add(startNode);
            parentMap[startNode] = null;
        }

        GridTile endNode = null;

        while (queue.Count > 0)
        {
            GridTile current = queue.Dequeue();
            Port currentPorts = GetRotatedPorts(current);

            // Check neighbors: Top, Right, Bottom, Left
            CheckNeighbor(current, currentPorts, Port.Top, 0, -1, Port.Bottom, queue, filled, parentMap);
            CheckNeighbor(current, currentPorts, Port.Right, 1, 0, Port.Left, queue, filled, parentMap);
            CheckNeighbor(current, currentPorts, Port.Bottom, 0, 1, Port.Top, queue, filled, parentMap);
            CheckNeighbor(current, currentPorts, Port.Left, -1, 0, Port.Right, queue, filled, parentMap);

            // Check if this tile connects to the Receiver exit (on the right)
            if (current.x == columns - 1 && current.y == endRow && (currentPorts & Port.Right) != 0)
            {
                endNode = current;
            }
        }

        List<GridTile> path = new List<GridTile>();
        if (endNode != null)
        {
            GridTile curr = endNode;
            while (curr != null)
            {
                path.Add(curr);
                curr = parentMap[curr];
            }
            path.Reverse();
        }

        return new FlowResult { filled = filled, path = path, reachedEnd = endNode != null };
    }

    private void CheckNeighbor(GridTile current, Port currentPorts, Port outPort, int dx, int dy, Port inPort, Queue<GridTile> queue, HashSet<GridTile> filled, Dictionary<GridTile, GridTile> parentMap)
    {
        if ((currentPorts & outPort) == 0) return;

        int nx = current.x + dx;
        int ny = current.y + dy;

        if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
        {
            GridTile neighbor = grid[nx, ny];
            if (neighbor != null && !filled.Contains(neighbor))
            {
                if ((GetRotatedPorts(neighbor) & inPort) != 0)
                {
                    filled.Add(neighbor);
                    parentMap[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private Port GetRotatedPorts(GridTile tile)
    {
        int rotation = (int)Mathf.Round(tile.transform.localEulerAngles.z);
        return GetRotatedPorts(tile.type, rotation);
    }

    private Port GetRotatedPorts(int shape, int rotation)
    {
        int basePorts = 0;
        switch (shape)
        {
            case 0: basePorts = (int)(Port.Left | Port.Right); break; // Straight (Horizontal)
            case 1: basePorts = (int)(Port.Top | Port.Right); break;  // Elbow
            case 2: basePorts = (int)(Port.Left | Port.Right | Port.Top); break; // T-Junction / Pump Splitter (3-way)
            case 3: basePorts = (int)(Port.Left | Port.Right | Port.Top | Port.Bottom); break; // Cross-Junction (4-way)
            default: basePorts = 0; break;
        }

        // Positive rotation in Unity Z is CCW
        int steps = ((rotation % 360 + 360) % 360) / 90;
        for (int i = 0; i < steps; i++)
        {
            int next = 0;
            if ((basePorts & (int)Port.Top) != 0) next |= (int)Port.Left;
            if ((basePorts & (int)Port.Left) != 0) next |= (int)Port.Bottom;
            if ((basePorts & (int)Port.Bottom) != 0) next |= (int)Port.Right;
            if ((basePorts & (int)Port.Right) != 0) next |= (int)Port.Top;
            basePorts = next;
        }
        return (Port)basePorts;
    }

    private bool HasPort(GridTile tile, Port port)
    {
        return (GetRotatedPorts(tile) & port) != 0;
    }

    private List<Vector2Int> GeneratePath(int startY, int endY)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        int curX = 0;
        int curY = startY;
        path.Add(new Vector2Int(curX, curY));

        int guard = 0;
        while (curX < columns - 1 && guard++ < 200)
        {
            int r = rng.Next(0, 5); // Bias towards moving right
            int nextX = curX;
            int nextY = curY;

            if (r <= 2) nextX++; // Right
            else if (r == 3 && curY > 0) nextY--; // Up
            else if (r == 4 && curY < rows - 1) nextY++; // Down

            Vector2Int pos = new Vector2Int(nextX, nextY);
            if (!path.Contains(pos))
            {
                curX = nextX;
                curY = nextY;
                path.Add(pos);
            }
        }
        
        // Connect to endRow on the last column
        guard = 0;
        while (curY != endY && guard++ < 50)
        {
            curY += (endY > curY) ? 1 : -1;
            Vector2Int pos = new Vector2Int(columns - 1, curY);
            if (!path.Contains(pos)) path.Add(pos);
        }

        return path;
    }

    private Port GetDirectionPort(int dx, int dy)
    {
        if (dx == 0 && dy == -1) return Port.Top;
        if (dx == 1 && dy == 0) return Port.Right;
        if (dx == 0 && dy == 1) return Port.Bottom;
        if (dx == -1 && dy == 0) return Port.Left;
        return Port.None;
    }

    private int GetShapeForPorts(Port p1, Port p2, out int rotation)
    {
        // Try Straight
        for (int r = 0; r < 360; r += 90)
            if (GetRotatedPorts(0, r) == (p1 | p2)) { rotation = r; return 0; }
        // Try Elbow
        for (int r = 0; r < 360; r += 90)
            if (GetRotatedPorts(1, r) == (p1 | p2)) { rotation = r; return 1; }
        // Fallback to T-Junction
        for (int r = 0; r < 360; r += 90)
        {
            Port rp = GetRotatedPorts(2, r);
            if ((rp & p1) != 0 && (rp & p2) != 0) { rotation = r; return 2; }
        }
        rotation = 0; return 0;
    }

    private IEnumerator SolveSequence(List<GridTile> path)
    {
        solvingSequence = true;
        foreach (var tile in path)
        {
            tile.SetColor(winColor);
            yield return new WaitForSeconds(0.1f);
        }
        Solve();
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || solvingSequence) return;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GridTile tile = grid[x, y];
                int target = targetRotations[x + y * columns];
                int current = (int)Mathf.Round(tile.transform.localEulerAngles.z);
                
                if (((current % 360 + 360) % 360) != ((target % 360 + 360) % 360))
                {
                    RotateTile(tile);
                    UpdateFlow(true);
                    return;
                }
            }
        }
    }

    private Vector2 GridPosition(int x, int y)
    {
        float originX = (columns - 1) * tileSpacing / 2f;
        float originY = (rows - 1) * tileSpacing / 2f;
        return new Vector2(x * tileSpacing - originX, originY - y * tileSpacing);
    }

    private void ClearBoard()
    {
        if (grid != null)
        {
            foreach (var tile in grid)
                if (tile != null) tile.OnClicked -= OnTileClicked;
        }

        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }
    }
}

