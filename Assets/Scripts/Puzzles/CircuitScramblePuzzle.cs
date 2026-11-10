// CircuitScramblePuzzle (Complete Overhaul):
// Generates a fully dynamic, random layout each time it loads!
// Features multiple gates (Correct Gate matching the Core's demand, and Wrong Gates).
// Incorporates hidden traps (Overcharge, Backdraft, Relay) attached to wrong gates.
// Includes a completely fixed and highly responsive central core charging animation
// with glitch, alert, and progressive charge dimming/pulsing feedback.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CircuitScramblePuzzle : BasePuzzle
{
   [Header("Circuit Scramble")]
   public int columns = 6;
   public int rows = 6;
   public GameObject tilePrefab;
   public RectTransform gridContainer;
   public float tileSpacing = 110f;

    [Header("Art")]
    public Sprite straightSprite;
    public Sprite cornerSprite;
    public Sprite sourceSprite;
    public Sprite andGateSprite;
    public Sprite xorGateSprite;
    public Sprite coreSprite;

    [Header("Animations")]
    public Sprite[] straightFlowFrames;
    public Sprite[] cornerFlowFrames;
    public Sprite[] coreChargeFrames;
    public Sprite[] sourcePulseFrames;

    public TMP_Text requirementText;
    public TMP_Text calibrationText;
    public Slider calibrationSlider;

    private static readonly Color ColorInactive = new Color(0.25f, 0.28f, 0.35f, 1f);
    private static readonly Color ColorPowered = new Color(0f, 0.95f, 1f, 1f);
    private static readonly Color ColorTrap = new Color(1f, 0.15f, 0.15f, 1f);
    private static readonly Color ColorSolved = new Color(0.2f, 1f, 0.45f, 1f);

    private const int WIRE_STRAIGHT = 0;
    private const int WIRE_CORNER = 1;
    private const int GATE = 2;
    private const int CORE = 5;
    private const int SOURCE = 6;

    private readonly List<CircuitScrambleTile> tiles = new List<CircuitScrambleTile>();
    private CircuitScrambleTile[,] grid;
    private int[,] targetRot;
    private bool[,] rotatable;
    private bool[,] isSolutionTile;

    // Internal access for CircuitScrambleTraps
    internal List<CircuitScrambleTile> tilesInternal => tiles;

    private Vector2Int corePos = new Vector2Int(3, 4);
    private Vector2Int srcAPos = new Vector2Int(1, 0);
    private Vector2Int srcBPos = new Vector2Int(4, 0);

    private readonly List<Vector2Int> gatePositions = new List<Vector2Int>();
    private Vector2Int correctGatePos;
    private Vector2Int overchargeGatePos;
    private Vector2Int backdraftGatePos;
    private Vector2Int relayGatePos;

    private bool gateIsAnd;
    private bool xorUsesSourceA;
    private int correctOutputDirection = 1;
    private CircuitScrambleTile coreTile;

    private CircuitScrambleTraps traps;
    private float currentCharge;
    private float flowRefreshTimer;
    private bool relayActive;

    [System.Flags]
    private enum Port { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0)
    };

    public bool IsCurrentlySatisfied { get; private set; }
    public bool GateIsAnd => gateIsAnd;
    public bool IsRelayActive => relayActive;

    protected override void Update()
    {
        base.Update();
        if (!isRunning || isSolved || isTimedOut || traps == null) return;

        flowRefreshTimer += Time.deltaTime;
        if (flowRefreshTimer >= 0.1f)
        {
            flowRefreshTimer = 0f;
            UpdatePowerFlow();
        }
    }

    // Internal access for CircuitScrambleTraps
    internal Vector2Int srcAPosInternal => srcAPos;
    internal Vector2Int srcBPosInternal => srcBPos;
    internal Vector2Int corePosInternal => corePos;
    internal Vector2Int correctGatePosInternal => correctGatePos;
    internal Vector2Int overchargeGatePosInternal => overchargeGatePos;
    internal Vector2Int backdraftGatePosInternal => backdraftGatePos;
    internal Vector2Int relayGatePosInternal => relayGatePos;
    internal bool gateIsAndInternal => gateIsAnd;

    protected override void BuildBoard()
    {
        ClearBoard();

        columns = 6;
        rows = 6;

        gateIsAnd = rng.Next(0, 2) == 0;
        xorUsesSourceA = rng.Next(0, 2) == 0;

        grid = new CircuitScrambleTile[columns, rows];
        targetRot = new int[columns, rows];
        rotatable = new bool[columns, rows];
        isSolutionTile = new bool[columns, rows];
        currentCharge = 0f;
        flowRefreshTimer = 0f;
        relayActive = false;
        IsCurrentlySatisfied = false;

        corePos = new Vector2Int(3, 4);
        srcAPos = new Vector2Int(1, 0);
        srcBPos = new Vector2Int(4, 0);

        if (calibrationSlider != null)
        {
            calibrationSlider.minValue = 0f;
            calibrationSlider.maxValue = 1f;
            calibrationSlider.value = 0f;
        }
        if (requirementText != null)
            requirementText.text = gateIsAnd ? "AND GATE: BOTH INPUTS REQUIRED" : "XOR GATE: ONE INPUT ONLY";
        if (calibrationText != null)
            calibrationText.text = "POWER 0%";

        gatePositions.Clear();
        gatePositions.Add(new Vector2Int(2, 2));
        gatePositions.Add(new Vector2Int(1, 3));
        gatePositions.Add(new Vector2Int(4, 3));

        List<Vector2Int> pool = new List<Vector2Int>(gatePositions);
        correctGatePos = pool[rng.Next(pool.Count)];
        pool.Remove(correctGatePos);

        overchargeGatePos = pool[rng.Next(pool.Count)];
        pool.Remove(overchargeGatePos);

        backdraftGatePos = overchargeGatePos;
        relayGatePos = pool[0];

        traps = new CircuitScrambleTraps(this, rng);
        traps.Initialize(overchargeGatePos, backdraftGatePos, relayGatePos, corePos, srcAPos, srcBPos, correctGatePos, gateIsAnd);

        int[,] type = new int[columns, rows];
        int[,] solRot = new int[columns, rows];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                type[x, y] = WIRE_STRAIGHT;
                solRot[x, y] = 0;
            }

        type[corePos.x, corePos.y] = CORE;
        type[srcAPos.x, srcAPos.y] = SOURCE;
        type[srcBPos.x, srcBPos.y] = SOURCE;

        foreach (var gp in gatePositions)
        {
            type[gp.x, gp.y] = GATE;
        }

        var baseBlocked = new HashSet<Vector2Int> { corePos, srcAPos, srcBPos };
        foreach (var gp in gatePositions) baseBlocked.Add(gp);

        // Route the two inputs on separate cells. Try both routing orders because
        // some gate placements need the second source to take the shorter corridor.
        var correctInputBlocked = new HashSet<Vector2Int>(baseBlocked);
        List<Vector2Int> cA;
        List<Vector2Int> cB;
        FindDisjointPaths(
            srcAPos, correctGatePos + new Vector2Int(-1, 0),
            srcBPos, correctGatePos + new Vector2Int(0, -1),
            correctInputBlocked, out cA, out cB);

        // Only one output route is needed. Reserving the input wires prevents
        // the output from creating accidental junctions that a 2-port tile
        // cannot represent.
        var correctOutputBlocked = new HashSet<Vector2Int>(baseBlocked);
        correctOutputBlocked.Remove(corePos);
        ReservePath(cA, correctOutputBlocked);
        ReservePath(cB, correctOutputBlocked);
        var cOut1 = FindPath(correctGatePos + new Vector2Int(1, 0), corePos, correctOutputBlocked);
        var cOut2 = FindPath(correctGatePos + new Vector2Int(0, 1), corePos, correctOutputBlocked);
        List<Vector2Int> correctOutputPath = null;
        if (cOut1 != null && cOut2 != null)
            correctOutputPath = rng.Next(0, 2) == 0 ? cOut1 : cOut2;
        else
            correctOutputPath = cOut1 ?? cOut2;
        correctOutputDirection = correctOutputPath == cOut1 ? 1 : 2;

        var ocInputBlocked = new HashSet<Vector2Int>(baseBlocked);
        List<Vector2Int> ocA;
        List<Vector2Int> ocB;
        FindDisjointPaths(
            srcAPos, overchargeGatePos + new Vector2Int(-1, 0),
            srcBPos, overchargeGatePos + new Vector2Int(0, -1),
            ocInputBlocked, out ocA, out ocB);
        var ocOutputBlocked = new HashSet<Vector2Int>(baseBlocked);
        ocOutputBlocked.Remove(corePos);
        ReservePath(ocA, ocOutputBlocked);
        ReservePath(ocB, ocOutputBlocked);
        var ocOut1 = FindPath(overchargeGatePos + new Vector2Int(1, 0), corePos, ocOutputBlocked);
        var ocOut2 = FindPath(overchargeGatePos + new Vector2Int(0, 1), corePos, ocOutputBlocked);
        List<Vector2Int> ocOutputPath = ocOut1 ?? ocOut2;

        var relayInputBlocked = new HashSet<Vector2Int>(baseBlocked);
        List<Vector2Int> rA;
        List<Vector2Int> rB;
        FindDisjointPaths(
            srcAPos, relayGatePos + new Vector2Int(-1, 0),
            srcBPos, relayGatePos + new Vector2Int(0, -1),
            relayInputBlocked, out rA, out rB);
        var relayOutputBlocked = new HashSet<Vector2Int>(baseBlocked);
        relayOutputBlocked.Remove(corePos);
        ReservePath(rA, relayOutputBlocked);
        ReservePath(rB, relayOutputBlocked);
        var rOut = FindPath(relayGatePos + new Vector2Int(1, 0), corePos, relayOutputBlocked) ??
                   FindPath(relayGatePos + new Vector2Int(0, 1), corePos, relayOutputBlocked);

        // Include the gate cells as path endpoints so the adjacent wire tiles
        // receive the correct corner/straight orientation.
        AppendEnd(cA, correctGatePos);
        AppendEnd(cB, correctGatePos);
        PrependStart(correctOutputPath, correctGatePos);
        AppendEnd(ocA, overchargeGatePos);
        AppendEnd(ocB, overchargeGatePos);
        PrependStart(ocOutputPath, overchargeGatePos);
        AppendEnd(rA, relayGatePos);
        AppendEnd(rB, relayGatePos);
        PrependStart(rOut, relayGatePos);

        if (gateIsAnd || xorUsesSourceA) BuildPathWires(cA, type, solRot, true);
        if (gateIsAnd || !xorUsesSourceA) BuildPathWires(cB, type, solRot, true);
        BuildPathWires(correctOutputPath, type, solRot, true);

        BuildPathWires(ocA, type, solRot, false);
        BuildPathWires(ocB, type, solRot, false);
        BuildPathWires(ocOutputPath, type, solRot, false);

        BuildPathWires(rA, type, solRot, false);
        BuildPathWires(rB, type, solRot, false);
        BuildPathWires(rOut, type, solRot, false);

        ComputeTargetRotations(solRot);

        if (gridContainer == null) return;

        float originX = (columns - 1) * tileSpacing / 2f;
        float originY = (rows - 1) * tileSpacing / 2f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject go = Instantiate(tilePrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(x * tileSpacing - originX, originY - y * tileSpacing);
                rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

                CircuitScrambleTile tile = go.GetComponent<CircuitScrambleTile>();
                if (tile == null) continue;

                int t = type[x, y];
                bool isWire = (t == WIRE_STRAIGHT || t == WIRE_CORNER);
                rotatable[x, y] = isWire;

                tile.Init(t, x, y, 0);
                ApplyTileSprite(tile, t, x, y);

                grid[x, y] = tile;
                tiles.Add(tile);

                if (isWire) tile.OnClicked += OnTileClicked;
            }
        }

        coreTile = grid[corePos.x, corePos.y];

        ScrambleWires();

        int guard = 0;
        UpdatePowerFlow();
        while (IsCurrentlySatisfied && guard++ < 30)
        {
            ScrambleWires();
            UpdatePowerFlow();
        }
        UpdatePowerFlow();
    }

    private void FindDisjointPaths(
        Vector2Int startA, Vector2Int endA,
        Vector2Int startB, Vector2Int endB,
        HashSet<Vector2Int> blocked,
        out List<Vector2Int> pathA, out List<Vector2Int> pathB)
    {
        pathA = null;
        pathB = null;
        int bestLength = int.MaxValue;

        List<Vector2Int> firstA = FindPath(startA, endA, blocked);
        if (firstA != null)
        {
            var reserved = new HashSet<Vector2Int>(blocked);
            ReservePath(firstA, reserved);
            List<Vector2Int> secondB = FindPath(startB, endB, reserved);
            if (secondB != null && firstA.Count + secondB.Count < bestLength)
            {
                pathA = firstA;
                pathB = secondB;
                bestLength = firstA.Count + secondB.Count;
            }
        }

        List<Vector2Int> firstB = FindPath(startB, endB, blocked);
        if (firstB != null)
        {
            var reserved = new HashSet<Vector2Int>(blocked);
            ReservePath(firstB, reserved);
            List<Vector2Int> secondA = FindPath(startA, endA, reserved);
            if (secondA != null && firstB.Count + secondA.Count < bestLength)
            {
                pathA = secondA;
                pathB = firstB;
            }
        }
    }

    private static void ReservePath(List<Vector2Int> path, HashSet<Vector2Int> blocked)
    {
        if (path == null) return;
        for (int i = 1; i < path.Count; i++)
            blocked.Add(path[i]);
    }

    private static void AppendEnd(List<Vector2Int> path, Vector2Int endpoint)
    {
        if (path != null && (path.Count == 0 || path[path.Count - 1] != endpoint))
            path.Add(endpoint);
    }

    private static void PrependStart(List<Vector2Int> path, Vector2Int endpoint)
    {
        if (path != null && (path.Count == 0 || path[0] != endpoint))
            path.Insert(0, endpoint);
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, HashSet<Vector2Int> blocked)
    {
        Queue<List<Vector2Int>> queue = new Queue<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(new List<Vector2Int> { start });
        visited.Add(start);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            Vector2Int curr = path[path.Count - 1];
            if (curr == end) return path;

            foreach (var d in DIRS)
            {
                Vector2Int next = curr + d;
                if (next.x < 0 || next.x >= columns || next.y < 0 || next.y >= rows) continue;
                if (blocked.Contains(next) && next != end) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                var newPath = new List<Vector2Int>(path) { next };
                queue.Enqueue(newPath);
            }
        }
        return null;
    }

    private void BuildPathWires(List<Vector2Int> path, int[,] type, int[,] solRot, bool isSolution)
    {
        if (path == null || path.Count < 3) return;

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int curr = path[i];
            if (!isSolution && isSolutionTile[curr.x, curr.y]) continue;

            Vector2Int prev = path[i - 1];
            Vector2Int next = path[i + 1];

            Vector2Int dirPrev = prev - curr;
            Vector2Int dirNext = next - curr;

            if (dirPrev.x == 0 && dirNext.x == 0)
            {
                // The straight sprite is authored horizontally; 90 degrees is vertical.
                type[curr.x, curr.y] = WIRE_STRAIGHT;
                solRot[curr.x, curr.y] = 90;
            }
            else if (dirPrev.y == 0 && dirNext.y == 0)
            {
                type[curr.x, curr.y] = WIRE_STRAIGHT;
                solRot[curr.x, curr.y] = 0;
            }
            else
            {
                type[curr.x, curr.y] = WIRE_CORNER;
                // The corner sprite is authored with left + bottom ports at 0 degrees.
                if (dirPrev.x == -1 && dirNext.y == -1) solRot[curr.x, curr.y] = 270; // Top-left
                else if (dirPrev.x == 1 && dirNext.y == -1) solRot[curr.x, curr.y] = 180; // Top-right
                else if (dirPrev.x == 1 && dirNext.y == 1) solRot[curr.x, curr.y] = 90; // Bottom-right
                else if (dirPrev.x == -1 && dirNext.y == 1) solRot[curr.x, curr.y] = 0; // Bottom-left
                else if (dirPrev.y == -1 && dirNext.x == 1) solRot[curr.x, curr.y] = 180; // Top-right (alt)
                else if (dirPrev.y == 1 && dirNext.x == 1) solRot[curr.x, curr.y] = 90; // Bottom-right (alt)
                else if (dirPrev.y == 1 && dirNext.x == -1) solRot[curr.x, curr.y] = 0; // Bottom-left (alt)
                else if (dirPrev.y == -1 && dirNext.x == -1) solRot[curr.x, curr.y] = 270; // Top-left (alt)
            }
            if (isSolution) isSolutionTile[curr.x, curr.y] = true;
        }
    }

    private void ComputeTargetRotations(int[,] solRot)
    {
        targetRot = new int[columns, rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                targetRot[x, y] = solRot[x, y];
    }

    private void ApplyTileSprite(CircuitScrambleTile tile, int type, int x, int y)
    {
        switch (type)
        {
            case WIRE_STRAIGHT:
                tile.SetSprite(straightSprite);
                break;
            case WIRE_CORNER:
                tile.SetSprite(cornerSprite);
                break;
            case SOURCE:
                tile.SetSprite(sourceSprite);
                break;
            case GATE:
                tile.SetSprite(gateIsAnd ? andGateSprite : xorGateSprite);
                break;
            case CORE:
                tile.SetSprite(coreSprite);
                break;
            default:
                tile.SetSprite(straightSprite);
                break;
        }
    }

    private int GetPortMask(CircuitScrambleTile tile)
    {
        if (tile == null) return 0;

        // Sources, gates and the core expose all four sides. Wire tiles expose
        // only the two sides represented by their current rotated sprite.
        if (tile.type == SOURCE || tile.type == GATE || tile.type == CORE)
            return (int)(Port.Top | Port.Right | Port.Bottom | Port.Left);

        int rotation = ((tile.rotation / 90) % 4 + 4) % 4;
        if (tile.type == WIRE_STRAIGHT)
            return rotation % 2 == 0
                ? (int)(Port.Left | Port.Right)
                : (int)(Port.Top | Port.Bottom);

        if (tile.type == WIRE_CORNER)
        {
            // The authored corner sprite starts with left + bottom ports.
            // Positive Unity Z rotation advances clockwise on the UI grid.
            switch (rotation)
            {
                case 0: return (int)(Port.Bottom | Port.Left);
                case 1: return (int)(Port.Right | Port.Bottom);
                case 2: return (int)(Port.Top | Port.Right);
                default: return (int)(Port.Top | Port.Left);
            }
        }

        return 0;
    }

    private bool HasPort(Vector2Int position, int direction)
    {
        if (position.x < 0 || position.x >= columns || position.y < 0 || position.y >= rows)
            return false;
        CircuitScrambleTile tile = grid[position.x, position.y];
        return tile != null && (GetPortMask(tile) & (1 << direction)) != 0;
    }

    private bool CanConnect(Vector2Int position, int direction)
    {
        Vector2Int next = position + DIRS[direction];
        return HasPort(position, direction) && HasPort(next, (direction + 2) % 4);
    }

    private HashSet<Vector2Int> FloodConnected(Vector2Int start, bool stopAtGates = true)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        if (start.x < 0 || start.x >= columns || start.y < 0 || start.y >= rows ||
            grid[start.x, start.y] == null)
            return visited;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            CircuitScrambleTile currentTile = grid[current.x, current.y];
            if (currentTile == null || currentTile.type == CORE ||
                (stopAtGates && currentTile.type == GATE))
                continue;

            for (int direction = 0; direction < DIRS.Length; direction++)
            {
                if (!CanConnect(current, direction)) continue;

                Vector2Int next = current + DIRS[direction];
                if (!visited.Add(next)) continue;

                CircuitScrambleTile nextTile = grid[next.x, next.y];
                if (nextTile != null && nextTile.type != CORE &&
                    (!stopAtGates || nextTile.type != GATE))
                    queue.Enqueue(next);
            }
        }

        return visited;
    }

    private bool IsInputConnected(HashSet<Vector2Int> powered, Vector2Int gate, int gateToInputDirection)
    {
        Vector2Int input = gate + DIRS[gateToInputDirection];
        return powered.Contains(input) && CanConnect(gate, gateToInputDirection);
    }

    private bool GateOutputReachesCore(Vector2Int gate, int outputDirection)
    {
        if (!CanConnect(gate, outputDirection)) return false;
        Vector2Int firstWire = gate + DIRS[outputDirection];
        return FloodConnected(firstWire).Contains(corePos);
    }

    internal void UpdatePowerFlow()
    {
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            tile.SetColor(ColorInactive);
            if (tile.OverlayImage != null)
                tile.OverlayImage.color = new Color(1f, 1f, 1f, 0f);
        }

        // Keep the two sources independent. This matters for XOR: one live
        // input must not be mistaken for two merely because the networks merge.
        HashSet<Vector2Int> poweredA = FloodConnected(srcAPos);
        HashSet<Vector2Int> poweredB = FloodConnected(srcBPos);
        HashSet<Vector2Int> powered = new HashSet<Vector2Int>(poweredA);
        powered.UnionWith(poweredB);

        // Include connected output lanes from all gates so the trap evaluator
        // can distinguish a fully wired decoy gate from an incomplete one.
        foreach (Vector2Int gate in gatePositions)
        {
            for (int direction = 1; direction <= 2; direction++)
            {
                if (CanConnect(gate, direction))
                    powered.UnionWith(FloodConnected(gate + DIRS[direction]));
            }
        }

        int solutionTileCount = 0;
        int poweredSolutionTileCount = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (!isSolutionTile[x, y]) continue;
                solutionTileCount++;
                if (powered.Contains(new Vector2Int(x, y)))
                    poweredSolutionTileCount++;
            }
        }

        float targetCharge = solutionTileCount > 0
            ? (float)poweredSolutionTileCount / solutionTileCount
            : 0f;
        currentCharge = Mathf.MoveTowards(currentCharge, targetCharge, Time.deltaTime * 3f);

        foreach (Vector2Int position in powered)
        {
            CircuitScrambleTile tile = grid[position.x, position.y];
            if (tile == null) continue;
            tile.SetColor(ColorPowered);
            if (tile.OverlayImage != null)
                tile.OverlayImage.color = new Color(1f, 1f, 1f, 0.8f);
        }

        bool inputA = IsInputConnected(poweredA, correctGatePos, 3); // left input
        bool inputB = IsInputConnected(poweredB, correctGatePos, 0); // top input
        bool inputsSatisfied = gateIsAnd ? (inputA && inputB) : (inputA ^ inputB);
        bool outputConnected = GateOutputReachesCore(correctGatePos, correctOutputDirection);

        IsCurrentlySatisfied = inputsSatisfied && outputConnected;

        float charge = currentCharge;
        bool trapSolved = IsCurrentlySatisfied;
        bool relay = false;
        if (traps != null)
            traps.UpdateTraps(powered, ref charge, ref trapSolved, ref relay);
        currentCharge = charge;
        relayActive = relay;
        IsCurrentlySatisfied = trapSolved;
        if (calibrationSlider != null)
            calibrationSlider.value = Mathf.Clamp01(currentCharge);
        if (calibrationText != null)
        {
            int percent = Mathf.RoundToInt(currentCharge * 100f);
            calibrationText.text = relayActive ? $"RELAY ACTIVE / POWER {percent}%" : $"POWER {percent}%";
        }

        if (IsCurrentlySatisfied)
        {
            if (CanConnect(correctGatePos, correctOutputDirection))
            {
                foreach (Vector2Int position in FloodConnected(correctGatePos + DIRS[correctOutputDirection]))
                {
                    CircuitScrambleTile tile = grid[position.x, position.y];
                    if (tile == null) continue;
                    tile.SetColor(ColorPowered);
                    if (tile.OverlayImage != null)
                        tile.OverlayImage.color = new Color(1f, 1f, 1f, 0.8f);
                }
            }

            if (!isSolved && CanAcceptInput())
                Solve();
        }

        if (coreTile != null)
            coreTile.SetColor(IsCurrentlySatisfied ? ColorSolved : Color.white);
    }

    private void ScrambleWires()
    {
        // Randomly rotate all rotatable wires
        foreach (var tile in tiles)
        {
            if (rotatable[tile.x, tile.y])
            {
                int randomRotation = rng.Next(0, 4) * 90; // 0, 90, 180, or 270 degrees
                tile.SetRotation(randomRotation);
            }
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || !CanAcceptInput()) return;

        // Bots align only the guaranteed solution route. Decoy routes stay
        // scrambled so the automated solver does not deliberately activate traps.
        foreach (var tile in tiles)
        {
            if (tile == null || !rotatable[tile.x, tile.y] ||
                !isSolutionTile[tile.x, tile.y])
                continue;

            int target = targetRot[tile.x, tile.y];
            if (tile.rotation == target) continue;

            tile.SetRotation(target);
            UpdatePowerFlow();
            return;
        }

        UpdatePowerFlow();
    }

    private void OnTileClicked(UITile tile)
    {
        var circuitTile = tile as CircuitScrambleTile;
        if (circuitTile == null) return;

        if (!CanAcceptInput() || traps.IsExploding) return;
        circuitTile.Rotate();
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
        UpdatePowerFlow();
    }

    /// <summary>
    /// Public wrapper for IsConnected used by CircuitScrambleTraps.
    /// Checks if a path exists between two positions through powered tiles.
    /// </summary>
    public bool IsConnectedPublic(Vector2Int start, Vector2Int end, HashSet<Vector2Int> powered)
    {
        return IsConnected(start, end, powered);
    }

    /// <summary>
    /// Checks if a path exists between start and end through powered tiles using BFS.
    /// </summary>
    private bool IsConnected(Vector2Int start, Vector2Int end, HashSet<Vector2Int> powered)
    {
        if (start == end) return true;
        if (!powered.Contains(start) || !powered.Contains(end)) return false;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) return true;

            for (int direction = 0; direction < DIRS.Length; direction++)
            {
                if (!CanConnect(current, direction)) continue;
                Vector2Int next = current + DIRS[direction];
                if (!powered.Contains(next) || !visited.Add(next)) continue;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    internal void ScrambleWiresInternal()
    {
        ScrambleWires();
    }
}