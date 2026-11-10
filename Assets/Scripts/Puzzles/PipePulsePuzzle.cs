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

    [Header("Source / Receiver Animation")]
    public Sprite[] sourceFrames;   // looping inflow animation at the StartMarker
    public Sprite[] receiverFrames; // empty(0) -> full(last) at the EndMarker
    public float sourceFps = 10f;
    public float receiverFps = 12f;
    private UISpriteSheetAnimator receiverAnim;
    private bool receiverActive;

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

        // Only the Cross-Junction (type 3) still needs a second overlay arm. The
        // elbow (type 1) now uses dedicated curved art (pipeAnimations[1]) whose
        // openings are pre-centred on the top/right edges, so it connects cleanly
        // with the straight pieces under any tile rotation.
        // The Cross-Junction (type 3) and the Elbow (type 1) both need a second
        // overlay arm. Every other shape uses a single overlay.
        Transform t2 = tile.transform.Find("PipeOverlay2");
        bool needsSecondArm = (tile.type == 3 || tile.type == 1);
        if (t2 != null && !needsSecondArm)
        {
            Destroy(t2.gameObject);
            t2 = null;
        }

        if (tile.type == 3) // Cross-Junction: two FULL straight pipes crossed.
        {
            targetImg.type = UnityEngine.UI.Image.Type.Simple;
            targetImg.fillAmount = 1f;
            targetImg.rectTransform.localRotation = Quaternion.identity;
            SetupAnimatorForType(anim, targetImg, 0, isFilled);

            UnityEngine.UI.Image targetImg2 = EnsureSecondArm(tile, ref t2, 90f);
            if (targetImg2 != null)
            {
                targetImg2.type = UnityEngine.UI.Image.Type.Simple;
                targetImg2.fillAmount = 1f;
                UISpriteSheetAnimator anim2 = targetImg2.GetComponent<UISpriteSheetAnimator>();
                if (anim2 == null) anim2 = targetImg2.gameObject.AddComponent<UISpriteSheetAnimator>();
                SetupAnimatorForType(anim2, targetImg2, 0, isFilled);
            }
        }
        else if (tile.type == 1) // Elbow: two STRAIGHT half-arms forming Top + Right.
        {
            // Reuse the straight pipe art so the elbow's openings, brass flanges,
            // glass style and water animation are pixel-identical to the straight
            // pieces it connects to. Each arm is the straight sprite clipped to its
            // right half; one stays horizontal (Right opening) and the cloned arm is
            // rotated +90 degrees (Top opening). At tile rotation 0 this is Top|Right,
            // matching GetRotatedPorts(1, 0).
            ConfigureHalfArm(targetImg, anim, isFilled);

            UnityEngine.UI.Image arm2 = EnsureSecondArm(tile, ref t2, 90f);
            if (arm2 != null)
            {
                UISpriteSheetAnimator anim2 = arm2.GetComponent<UISpriteSheetAnimator>();
                if (anim2 == null) anim2 = arm2.gameObject.AddComponent<UISpriteSheetAnimator>();
                ConfigureHalfArm(arm2, anim2, isFilled);
            }
        }
        else
        {
            // Straight (0) and T-Junction (2) render as a single full-size sprite.
            // Tile rotation orients the piece; the overlay stays at identity local
            // rotation so it follows the tile.
            targetImg.type = UnityEngine.UI.Image.Type.Simple;
            targetImg.fillAmount = 1f;
            targetImg.rectTransform.localRotation = Quaternion.identity;
            SetupAnimatorForType(anim, targetImg, tile.type, isFilled);
        }
    }

    // Configures an Image as a half-length straight-pipe arm: the straight sprite
    // clipped to its right half (slightly past centre so the bend corner fills).
    // The second arm is rotated +90 degrees by EnsureSecondArm to make the bend.
    private void ConfigureHalfArm(UnityEngine.UI.Image img, UISpriteSheetAnimator anim, bool isFilled)
    {
        img.type = UnityEngine.UI.Image.Type.Filled;
        img.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        img.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Right;
        img.fillAmount = 0.55f;
        // NOTE: do not touch localRotation here. The main arm keeps the tile's
        // identity rotation; the cloned second arm is rotated +90 by EnsureSecondArm.
        SetupAnimatorForType(anim, img, 0, isFilled); // always straight art (type 0)
    }

    // Ensures a "PipeOverlay2" arm exists (cloned from the main overlay) and gives
    // it the supplied local Z rotation. Returns its Image (or null if unavailable).
    private UnityEngine.UI.Image EnsureSecondArm(GridTile tile, ref Transform t2, float localZ)
    {
        if (t2 == null && tile.OverlayImage != null)
        {
            GameObject go2 = Instantiate(tile.OverlayImage.gameObject, tile.transform);
            go2.name = "PipeOverlay2";
            t2 = go2.transform;
        }
        if (t2 == null) return null;
        t2.localPosition = Vector3.zero;
        t2.localRotation = Quaternion.Euler(0f, 0f, localZ);
        return t2.GetComponent<UnityEngine.UI.Image>();
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
    // Sets the always-on looping flow at the source (StartMarker) and resets the
    // receiver (EndMarker) to its empty state.
    private void SetupSourceReceiver()
    {
        if (startMarker != null && sourceFrames != null && sourceFrames.Length > 0)
        {
            var img = startMarker.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = Color.white;
                var anim = img.GetComponent<UISpriteSheetAnimator>();
                if (anim == null) anim = img.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.PlaySet(sourceFrames, sourceFps, true); // always flowing
            }
        }

        receiverActive = false;
        if (endMarker != null && receiverFrames != null && receiverFrames.Length > 0)
        {
            var img = endMarker.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = Color.white;
                receiverAnim = img.GetComponent<UISpriteSheetAnimator>();
                if (receiverAnim == null) receiverAnim = img.gameObject.AddComponent<UISpriteSheetAnimator>();
                receiverAnim.Stop();
                img.sprite = receiverFrames[0]; // empty drain
            }
        }
    }

    // Drives the receiver: when flow first reaches the end, play the fill animation
    // once and hold on the full (last) frame.
    private void UpdateReceiver(bool reachedEnd)
    {
        if (receiverFrames == null || receiverFrames.Length == 0 || endMarker == null) return;
        var img = endMarker.GetComponent<UnityEngine.UI.Image>();
        if (img == null) return;

        if (reachedEnd && !receiverActive)
        {
            receiverActive = true;
            if (receiverAnim != null) receiverAnim.PlaySet(receiverFrames, receiverFps, false, () => {
                // Animation complete - sprite is already at last frame
            });
            else img.sprite = receiverFrames[receiverFrames.Length - 1];
        }
        else if (!reachedEnd && receiverActive)
        {
            receiverActive = false;
            if (receiverAnim != null) receiverAnim.Stop();
            img.sprite = receiverFrames[0]; // back to empty
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

        // Reservoirs disabled: single-pipe (straight/elbow) tiles cannot form the
        // 3-way junction a reservoir tap requires. -1 disables all reservoir logic.
        topReservoirCol = -1;
        bottomReservoirCol = -1;

        if (reservoirSlider != null)
        {
            reservoirSlider.minValue = 0f;
            reservoirSlider.maxValue = columns * rows;
            reservoirSlider.value = 0;
        }

        float originX = (columns - 1) * tileSpacing * 0.5f;
        float originY = (rows - 1) * tileSpacing * 0.5f;

        // Position markers. The source/receiver art is ~2:1, so size the markers
        // wider than tall to avoid squashing.
        Vector2 markerSize = new Vector2(tileSpacing * 1.0f, tileSpacing * 0.55f);
        if (startMarker != null)
        {
            startMarker.anchoredPosition = new Vector2(-originX - tileSpacing * 0.75f, originY - startRow * tileSpacing);
            startMarker.sizeDelta = markerSize;
        }
        if (endMarker != null)
        {
            endMarker.anchoredPosition = new Vector2(originX + tileSpacing * 0.75f, originY - endRow * tileSpacing);
            endMarker.sizeDelta = markerSize;
        }

        SetupSourceReceiver();

        // 1. Generate ONE solvable path from left to right. A single, non-self-
        // intersecting path means every path tile needs exactly two ports
        // (entry + exit) -> only straights and elbows are ever required.
        List<Vector2Int> path1 = GeneratePath(startRow, endRow);

        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>(path1);

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

                        shape = GetShapeForPorts(requiredPorts, out targetRot);
                    }
                    else
                    {
                        // Decoration tiles are single-pipe only: straight (0) or elbow (1).
                        shape = rng.Next(0, 2);
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
                    tile.OnClicked += (UITile t) => OnTileClicked((GridTile)t);

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
        float angle = tile.transform.localEulerAngles.z;
        int currentRot = Mathf.RoundToInt(angle) % 360;
        if (currentRot < 0) currentRot += 360;
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

        UpdateReceiver(flowData.reachedEnd);

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
        float angle = tile.transform.localEulerAngles.z;
        int rotation = Mathf.RoundToInt(angle) % 360;
        if (rotation < 0) rotation += 360;
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

    private int GetShapeForPorts(Port requiredPorts, out int rotation)
    {
        // 1. First search for an exact match
        for (int shape = 0; shape < 4; shape++)
        {
            for (int r = 0; r < 360; r += 90)
            {
                if (GetRotatedPorts(shape, r) == requiredPorts)
                {
                    rotation = r;
                    return shape;
                }
            }
        }

        // 2. If no exact match, find the shape that contains all required ports with minimal extra ports
        for (int shape = 0; shape < 4; shape++)
        {
            for (int r = 0; r < 360; r += 90)
            {
                Port rp = GetRotatedPorts(shape, r);
                if ((rp & requiredPorts) == requiredPorts)
                {
                    rotation = r;
                    return shape;
                }
            }
        }

        // Fallback to straight horizontal
        rotation = 0;
        return 0;
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
                int current = Mathf.RoundToInt(tile.transform.localEulerAngles.z) % 360;
                if (current < 0) current += 360;
                
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

    protected override void ClearBoard()
    {
        if (grid != null)
        {
            foreach (var tile in grid)
                if (tile != null) tile.OnClicked -= (UITile t) => OnTileClicked((GridTile)t);
        }

        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }

        if (topReservoirMarker != null)
        {
            Destroy(topReservoirMarker.gameObject);
            topReservoirMarker = null;
        }
        if (bottomReservoirMarker != null)
        {
            Destroy(bottomReservoirMarker.gameObject);
            bottomReservoirMarker = null;
        }
    }

    private IEnumerator PulseMarker(RectTransform marker)
    {
        if (marker == null) yield break;
        Vector3 originalScale = marker.localScale;
        Vector3 peakScale = originalScale * 1.4f;
        float duration = 0.3f;
        float half = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            if (marker == null) yield break;
            elapsed += Time.deltaTime;
            marker.localScale = Vector3.Lerp(originalScale, peakScale, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            if (marker == null) yield break;
            elapsed += Time.deltaTime;
            marker.localScale = Vector3.Lerp(peakScale, originalScale, elapsed / half);
            yield return null;
        }
        if (marker != null) marker.localScale = originalScale;
    }
}

