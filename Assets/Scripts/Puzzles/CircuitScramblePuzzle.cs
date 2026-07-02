using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Enhanced CircuitScramblePuzzle: Charge the central node from power sources on the left column.
// The central node dynamically changes its power requirement every few seconds (AND, XOR, or Direct power).
// Traps exist on the board; routing power into a Trap causes a Short Circuit (screenshake + rapid charge drain).
// Tiles are rotated to build pathways and route power through the required logic gates.
public class CircuitScramblePuzzle : BasePuzzle
{
    [Header("Circuit Scramble")]
    public int columns = 5;
    public int rows = 5;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 110f;

    [Header("Art")]
    public Sprite tileSprite;          // themed circuit tile art
    public Sprite solvedTileSprite;    // optional charged/solved look
    public TMP_Text requirementText;
    public TMP_Text calibrationText;
    public Slider calibrationSlider;

    private readonly List<CircuitScrambleTile> tiles = new List<CircuitScrambleTile>();
    private CircuitScrambleTile[,] grid;

    private int centerX;
    private int centerY;
    private float currentCharge = 0f;
    private float requirementTimer = 8f;
    private int activeRequirement = 0; // 0: AND, 1: XOR, 2: Direct

    private float shakeTimer = 0f;
    private Vector3 originalGridPosition;

    [System.Flags]
    private enum Port { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

    private struct FlowState
    {
        public int x;
        public int y;
        public bool passedAND;
        public bool passedXOR;
        public bool passedTrap;
    }

    protected override void BuildBoard()
    {
        ClearBoard();

        // Enforce 5x5 board to guarantee a clear center tile at (2, 2)
        columns = 5;
        rows = 5;
        centerX = 2;
        centerY = 2;

        grid = new CircuitScrambleTile[columns, rows];
        currentCharge = 0f;
        requirementTimer = 8f;
        activeRequirement = rng.Next(0, 3);

        if (calibrationSlider != null)
        {
            calibrationSlider.minValue = 0f;
            calibrationSlider.maxValue = 1f;
            calibrationSlider.value = 0f;
        }

        if (gridContainer != null)
        {
            originalGridPosition = gridContainer.localPosition;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    GameObject go = Instantiate(tilePrefab, gridContainer);
                    RectTransform rt = (RectTransform)go.transform;
                    rt.anchoredPosition = GridPosition(x, y);

                    CircuitScrambleTile tile = go.GetComponent<CircuitScrambleTile>();
                    if (tile == null) continue;

                    int type = 0;
                    int rot = rng.Next(0, 4) * 90;

                    if (x == centerX && y == centerY)
                    {
                        type = 5; // Central Node
                        rot = 0;  // Center does not rotate
                    }
                    else if (x == 0)
                    {
                        type = 6; // Power Source (always active)
                        rot = 0;  // Sources do not rotate
                    }
                    else
                    {
                        // Randomly distribute gates, traps, and wires
                        int rand = rng.Next(0, 100);
                        if (rand < 20) type = 2;      // AND Gate
                        else if (rand < 40) type = 3; // XOR Gate
                        else if (rand < 55) type = 4; // Short Circuit Trap
                        else if (rand < 75) type = 1; // Corner Wire
                        else type = 0;                // Straight Wire
                    }

                    tile.Init(type, x, y, rot);

                    if (tileSprite != null)
                    {
                        tile.SetSprite(tileSprite);
                    }

                    // Power sources and Central Node cannot be rotated/clicked
                    if (type != 5 && type != 6)
                    {
                        tile.OnClicked += OnTileClicked;
                    }

                    tiles.Add(tile);
                    grid[x, y] = tile;
                }
            }
        }

        UpdatePowerFlow();
    }

    private void OnTileClicked(CircuitScrambleTile tile)
    {
        if (!CanAcceptInput()) return;
        
        tile.Rotate();

        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
        }

        UpdatePowerFlow();
    }

    private void UpdatePowerFlow()
    {
        if (grid == null) return;

        bool hasANDPower = false;
        bool hasXORPower = false;
        bool hasDirectPower = false;
        bool hitTrap = false;

        // Trace power flow starting from leftmost column (Power Sources)
        HashSet<Vector2Int> poweredCells = new HashSet<Vector2Int>();
        Queue<FlowState> queue = new Queue<FlowState>();
        HashSet<(int, int, int)> visited = new HashSet<(int, int, int)>(); // (x, y, incomingPort)

        // Queue all initial power source outputs
        for (int y = 0; y < rows; y++)
        {
            CircuitScrambleTile source = grid[0, y];
            if (source != null)
            {
                poweredCells.Add(new Vector2Int(0, y));
                queue.Enqueue(new FlowState
                {
                    x = 0,
                    y = y,
                    passedAND = false,
                    passedXOR = false,
                    passedTrap = false
                });
            }
        }

        while (queue.Count > 0)
        {
            FlowState cur = queue.Dequeue();
            CircuitScrambleTile tile = grid[cur.x, cur.y];
            if (tile == null) continue;

            Port curPorts = GetRotatedPorts(tile.type, tile.rotation, cur.x, cur.y);

            // Check neighbor directions (Top, Right, Bottom, Left)
            CheckFlowDirection(cur, curPorts, Port.Top, 0, -1, Port.Bottom, queue, visited, poweredCells);
            CheckFlowDirection(cur, curPorts, Port.Right, 1, 0, Port.Left, queue, visited, poweredCells);
            CheckFlowDirection(cur, curPorts, Port.Bottom, 0, 1, Port.Top, queue, visited, poweredCells);
            CheckFlowDirection(cur, curPorts, Port.Left, -1, 0, Port.Right, queue, visited, poweredCells);

            // If we reached the Central Node, analyze the path properties
            if (cur.x == centerX && cur.y == centerY)
            {
                if (cur.passedAND) hasANDPower = true;
                if (cur.passedXOR) hasXORPower = true;
                hasDirectPower = true;
            }

            if (tile.type == 4) // Trap
            {
                hitTrap = true;
            }
        }

        // Apply colors to highlight powered vs unpowered tiles
        foreach (CircuitScrambleTile t in tiles)
        {
            if (t == null) continue;

            bool isPowered = poweredCells.Contains(new Vector2Int(t.x, t.y));
            t.Image.color = GetTileColor(t.type, isPowered);
        }

        // Apply charging / decay / short-circuit logic
        bool isCharging = false;

        if (hitTrap)
        {
            // Short Circuit Trap is active!
            isCharging = false;
            currentCharge = Mathf.Max(0f, currentCharge - Time.deltaTime * 0.25f); // Rapid drain

            if (shakeTimer <= 0f)
            {
                shakeTimer = 0.4f; // Trigger screenshake
            }

            if (requirementText != null)
            {
                requirementText.text = "<color=red>SHORT CIRCUIT DETECTED! TRAP TRIGGERED!</color>";
            }
        }
        else
        {
            // Evaluate requirement conditions
            switch (activeRequirement)
            {
                case 0: // AND Gate required
                    isCharging = hasANDPower;
                    if (requirementText != null)
                    {
                        requirementText.text = "REQUIRED POWER: <color=green>AND GATE POWER</color>";
                    }
                    break;
                case 1: // XOR Gate required
                    isCharging = hasXORPower;
                    if (requirementText != null)
                    {
                        requirementText.text = "REQUIRED POWER: <color=purple>XOR GATE POWER</color>";
                    }
                    break;
                case 2: // Direct / Any power accepted
                    isCharging = hasDirectPower;
                    if (requirementText != null)
                    {
                        requirementText.text = "REQUIRED POWER: <color=cyan>ANY DIRECT CONNECTION</color>";
                    }
                    break;
            }

            if (isCharging)
            {
                // Charge up node!
                currentCharge = Mathf.Min(1f, currentCharge + Time.deltaTime * 0.15f);
            }
            else
            {
                // Slowly decay calibration over time
                currentCharge = Mathf.Max(0f, currentCharge - Time.deltaTime * 0.03f);
            }
        }

        // Update Slider and Text
        if (calibrationSlider != null)
        {
            calibrationSlider.value = currentCharge;
        }

        if (calibrationText != null)
        {
            calibrationText.text = $"CHARGE CALIBRATION: {Mathf.RoundToInt(currentCharge * 100f)}%";
        }

        // Handle Solve Condition
        if (currentCharge >= 1.0f)
        {
            Solve();
        }
    }

    private void CheckFlowDirection(FlowState cur, Port curPorts, Port outPort, int dx, int dy, Port inPort, Queue<FlowState> queue, HashSet<(int, int, int)> visited, HashSet<Vector2Int> poweredCells)
    {
        if ((curPorts & outPort) == 0) return;

        int nx = cur.x + dx;
        int ny = cur.y + dy;

        if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
        {
            CircuitScrambleTile neighbor = grid[nx, ny];
            if (neighbor != null)
            {
                Port neighborPorts = GetRotatedPorts(neighbor.type, neighbor.rotation, nx, ny);
                if ((neighborPorts & inPort) != 0)
                {
                    // Check visited state to prevent cyclic loops
                    var visitKey = (nx, ny, (int)inPort);
                    if (!visited.Contains(visitKey))
                    {
                        visited.Add(visitKey);
                        poweredCells.Add(new Vector2Int(nx, ny));

                        queue.Enqueue(new FlowState
                        {
                            x = nx,
                            y = ny,
                            passedAND = cur.passedAND || (neighbor.type == 2),
                            passedXOR = cur.passedXOR || (neighbor.type == 3),
                            passedTrap = cur.passedTrap || (neighbor.type == 4)
                        });
                    }
                }
            }
        }
    }

    private Port GetRotatedPorts(int type, int rotation, int x, int y)
    {
        if (x == centerX && y == centerY)
        {
            return Port.Top | Port.Right | Port.Bottom | Port.Left;
        }

        int basePorts = 0;
        switch (type)
        {
            case 0: // Straight wire
                basePorts = (int)(Port.Left | Port.Right);
                break;
            case 1: // Corner wire
                basePorts = (int)(Port.Left | Port.Top);
                break;
            case 2: // AND Gate (one input on Left, two outputs on Top & Right)
                basePorts = (int)(Port.Left | Port.Top | Port.Right);
                break;
            case 3: // XOR Gate (one input on Left, one output on Top)
                basePorts = (int)(Port.Left | Port.Top);
                break;
            case 4: // Short Circuit Trap
                basePorts = (int)(Port.Left | Port.Right);
                break;
            case 6: // Power Source (always outputs Right)
                basePorts = (int)Port.Right;
                break;
            default:
                basePorts = (int)(Port.Left | Port.Right);
                break;
        }

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

    private Color GetTileColor(int type, bool isPowered)
    {
        if (type == 5) // Central Node
        {
            return isPowered ? new Color(1f, 0.84f, 0f) : new Color(0.4f, 0.4f, 0.4f); // Glowing Gold vs Solid Gray
        }

        if (type == 6) // Power Source
        {
            return new Color(0f, 0.85f, 1f); // Vibrant blue source
        }

        if (isPowered)
        {
            switch (type)
            {
                case 2: return new Color(0f, 1f, 0.4f); // Active green AND
                case 3: return new Color(0.85f, 0.2f, 1f); // Active purple XOR
                case 4: return new Color(1f, 0.2f, 0.2f); // Alert red Trap
                default: return new Color(0f, 0.9f, 1f); // Active cyan wire
            }
        }
        else
        {
            // Darker colors for inactive states
            switch (type)
            {
                case 2: return new Color(0.1f, 0.4f, 0.2f);
                case 3: return new Color(0.4f, 0.1f, 0.4f);
                case 4: return new Color(0.4f, 0.1f, 0.1f);
                default: return new Color(0.25f, 0.28f, 0.35f);
            }
        }
    }

    private void ChangeRequirement()
    {
        requirementTimer = 8f;
        activeRequirement = rng.Next(0, 3);

        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxUiHover, 0.4f);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isRunning || isSolved || isTimedOut) return;

        // Update active requirement timer
        requirementTimer -= Time.deltaTime;
        if (requirementTimer <= 0f)
        {
            ChangeRequirement();
        }

        // Dynamic flow calculations
        UpdatePowerFlow();

        // Screen Shake during Short Circuits
        if (shakeTimer > 0f && gridContainer != null)
        {
            shakeTimer -= Time.deltaTime;
            float magnitude = 8f;
            float dx = Random.Range(-1f, 1f) * magnitude;
            float dy = Random.Range(-1f, 1f) * magnitude;
            gridContainer.localPosition = originalGridPosition + new Vector3(dx, dy, 0f);

            if (shakeTimer <= 0f)
            {
                gridContainer.localPosition = originalGridPosition;
            }
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;

        // AutoSolve by rotating a random tile that is not currently aligned
        List<CircuitScrambleTile> unaligned = new List<CircuitScrambleTile>();
        foreach (CircuitScrambleTile t in tiles)
        {
            if (t != null && t.type != 5 && t.type != 6)
            {
                // If it's a trap, rotate it away from active paths, or just rotate random tiles to complete circuits
                unaligned.Add(t);
            }
        }

        if (unaligned.Count > 0)
        {
            CircuitScrambleTile target = unaligned[rng.Next(unaligned.Count)];
            target.Rotate();
            UpdatePowerFlow();
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
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null) tiles[i].OnClicked -= OnTileClicked;
        }

        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContainer.GetChild(i).gameObject);
            }
        }

        tiles.Clear();
    }
}
