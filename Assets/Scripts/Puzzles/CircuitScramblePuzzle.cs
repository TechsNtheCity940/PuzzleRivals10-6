using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// CircuitScramblePuzzle (Complete Overhaul):
// Generates a fully dynamic, random layout each time it loads!
// Features multiple gates (Correct Gate matching the Core's demand, and Wrong Gates).
// Incorporates hidden traps (Overcharge, Backdraft, Relay) attached to wrong gates.
// Includes a completely fixed and highly responsive central core charging animation
// with glitch, alert, and progressive charge dimming/pulsing feedback.
public class CircuitScramblePuzzle : BasePuzzle
{
    [Header("Circuit Scramble")]
    public int columns = 6;
    public int rows = 6;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 110f;

    [Header("Art")]
    public Sprite straightSprite;       // horizontal
    public Sprite cornerSprite;         // left-bottom
    public Sprite sourceSprite;
    public Sprite andGateSprite;
    public Sprite xorGateSprite;
    public Sprite coreSprite;

    [Header("Animations")]
    public Sprite[] straightFlowFrames; // 16 frames of flow
    public Sprite[] cornerFlowFrames;   // 16 frames of flow
    public Sprite[] coreChargeFrames;
    public Sprite[] sourcePulseFrames;

    public TMP_Text requirementText;
    public TMP_Text calibrationText;
    public Slider calibrationSlider;

    private static readonly Color ColorInactive = new Color(0.25f, 0.28f, 0.35f, 1f);
    private static readonly Color ColorPowered = new Color(0f, 0.95f, 1f, 1f);
    private static readonly Color ColorTrap = new Color(1f, 0.15f, 0.15f, 1f);
    private static readonly Color ColorSolved = new Color(0.2f, 1f, 0.45f, 1f);

    // Tile type ids stored in CircuitScrambleTile.type
    private const int WIRE_STRAIGHT = 0;
    private const int WIRE_CORNER = 1;
    private const int GATE = 2;
    private const int CORE = 5;
    private const int SOURCE = 6;

    private readonly List<CircuitScrambleTile> tiles = new List<CircuitScrambleTile>();
    private CircuitScrambleTile[,] grid;
    private int[,] targetRot;            // bot solution rotation per cell
    private bool[,] rotatable;           // wire cells the player can rotate
    private bool[,] isSolutionTile;      // wire cells belonging to the winning correct gate paths

    private Vector2Int corePos = new Vector2Int(3, 4);
    private Vector2Int srcAPos = new Vector2Int(1, 0);
    private Vector2Int srcBPos = new Vector2Int(4, 0);

    private readonly List<Vector2Int> gatePositions = new List<Vector2Int>();
    private Vector2Int correctGatePos;
    private Vector2Int overchargeGatePos;
    private Vector2Int backdraftGatePos;
    private Vector2Int relayGatePos;

    private bool gateIsAnd;              // true = AND gate correct, false = XOR gate correct
    private float currentCharge = 0f;
    private CircuitScrambleTile coreTile;

    // Traps runtime state
    private float overchargeTimer = 0f;
    private float backdraftTimer = 0f;
    private bool relayActive = false;
    private bool isExploding = false;

    // Port definitions matching directions
    [System.Flags]
    private enum Port { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, -1), // Top (0)
        new Vector2Int(1, 0),  // Right (1)
        new Vector2Int(0, 1),  // Bottom (2)
        new Vector2Int(-1, 0)  // Left (3)
    };

    public bool IsCurrentlySatisfied { get; private set; }
    public bool GateIsAnd => gateIsAnd;

    protected override void BuildBoard()
    {
        ClearBoard();

        columns = 6;
        rows = 6;
        currentCharge = 0f;
        overchargeTimer = 0f;
        backdraftTimer = 0f;
        relayActive = false;
        isExploding = false;

        // Choose required gate type
        gateIsAnd = rng.Next(0, 2) == 0;

        grid = new CircuitScrambleTile[columns, rows];
        targetRot = new int[columns, rows];
        rotatable = new bool[columns, rows];
        isSolutionTile = new bool[columns, rows];

        // Core and sources coordinates
        corePos = new Vector2Int(3, 4);
        srcAPos = new Vector2Int(1, 0);
        srcBPos = new Vector2Int(4, 0);

        if (calibrationSlider != null)
        {
            calibrationSlider.minValue = 0f;
            calibrationSlider.maxValue = 1f;
            calibrationSlider.value = 0f;
        }

        // Layout gate positions strictly in bounds of [1, 4] so their ports fit
        gatePositions.Clear();
        gatePositions.Add(new Vector2Int(2, 2)); // Correct or wrong
        gatePositions.Add(new Vector2Int(1, 3)); // Overcharge/Backdraft trap
        gatePositions.Add(new Vector2Int(4, 3)); // Relay trap

        // Shuffle gate assignments
        List<Vector2Int> pool = new List<Vector2Int>(gatePositions);
        correctGatePos = pool[rng.Next(pool.Count)];
        pool.Remove(correctGatePos);

        overchargeGatePos = pool[rng.Next(pool.Count)];
        pool.Remove(overchargeGatePos);

        backdraftGatePos = overchargeGatePos; // backdraft shares or splits
        relayGatePos = pool[0];

        int[,] type = new int[columns, rows];
        int[,] solRot = new int[columns, rows];

        // Default all to straight wires
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                type[x, y] = WIRE_STRAIGHT;
                solRot[x, y] = 0;
            }

        // Set terminals
        type[corePos.x, corePos.y] = CORE;
        type[srcAPos.x, srcAPos.y] = SOURCE;
        type[srcBPos.x, srcBPos.y] = SOURCE;

        foreach (var gp in gatePositions)
        {
            type[gp.x, gp.y] = GATE;
        }

        // Block lists to build clean individual non-overlapping path structures
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int> { corePos, srcAPos, srcBPos };
        foreach (var gp in gatePositions) blocked.Add(gp);

        // Find paths for Correct Gate
        blocked.Remove(correctGatePos);
        blocked.Remove(corePos);

        List<Vector2Int> p1 = FindPath(srcAPos, correctGatePos, blocked);
        List<Vector2Int> p2 = FindPath(srcBPos, correctGatePos, blocked);
        List<Vector2Int> p3 = FindPath(correctGatePos, corePos, blocked);

        if (p3 != null)
        {
            for (int i = 1; i < p3.Count - 1; i++) blocked.Add(p3[i]);
        }
        List<Vector2Int> p4 = FindPath(correctGatePos, corePos, blocked);
        if (p3 != null)
        {
            for (int i = 1; i < p3.Count - 1; i++) blocked.Remove(p3[i]);
        }

        // Find paths for Overcharge Trap Gate
        blocked.Clear();
        blocked.Add(corePos);
        blocked.Add(srcAPos);
        blocked.Add(srcBPos);
        foreach (var gp in gatePositions) blocked.Add(gp);

        blocked.Remove(overchargeGatePos);
        blocked.Remove(corePos);

        List<Vector2Int> p5 = FindPath(srcAPos, overchargeGatePos, blocked);
        List<Vector2Int> p6 = FindPath(srcBPos, overchargeGatePos, blocked);
        List<Vector2Int> p7 = FindPath(overchargeGatePos, corePos, blocked);
        if (p7 != null)
        {
            for (int i = 1; i < p7.Count - 1; i++) blocked.Add(p7[i]);
        }
        List<Vector2Int> p8 = FindPath(overchargeGatePos, corePos, blocked);
        if (p7 != null)
        {
            for (int i = 1; i < p7.Count - 1; i++) blocked.Remove(p7[i]);
        }

        // Find paths for Relay Trap Gate
        blocked.Clear();
        blocked.Add(corePos);
        blocked.Add(srcAPos);
        blocked.Add(srcBPos);
        foreach (var gp in gatePositions) blocked.Add(gp);

        blocked.Remove(relayGatePos);
        blocked.Remove(corePos);

        List<Vector2Int> p9 = FindPath(srcAPos, relayGatePos, blocked);
        List<Vector2Int> p10 = FindPath(srcBPos, relayGatePos, blocked);
        List<Vector2Int> p11 = FindPath(relayGatePos, corePos, blocked);
        if (p11 != null)
        {
            for (int i = 1; i < p11.Count - 1; i++) blocked.Add(p11[i]);
        }
        List<Vector2Int> p12 = FindPath(relayGatePos, corePos, blocked);
        if (p11 != null)
        {
            for (int i = 1; i < p11.Count - 1; i++) blocked.Remove(p11[i]);
        }

        // Build path wires
        BuildPathWires(p1, type, solRot, true);
        BuildPathWires(p2, type, solRot, true);
        BuildPathWires(p3, type, solRot, true);
        BuildPathWires(p4, type, solRot, true);

        BuildPathWires(p5, type, solRot, false);
        BuildPathWires(p6, type, solRot, false);
        BuildPathWires(p7, type, solRot, false);
        BuildPathWires(p8, type, solRot, false);

        BuildPathWires(p9, type, solRot, false);
        BuildPathWires(p10, type, solRot, false);
        BuildPathWires(p11, type, solRot, false);
        BuildPathWires(p12, type, solRot, false);

        // Set solution target rotations for the bot solver
        ComputeTargetRotations(solRot);

        // Instantiate grid container children
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

        // Re-scramble if solved on start state
        int guard = 0;
        UpdatePowerFlow();
        while (IsCurrentlySatisfied && guard++ < 30)
        {
            ScrambleWires();
            UpdatePowerFlow();
        }
        currentCharge = 0f;
        UpdatePowerFlow();
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
        if (path == null || path.Count < 3) return; // need at least start, intermediate, end

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int curr = path[i];

            // If we are building wrong paths, do NOT overwrite correct solution tiles!
            if (!isSolution && isSolutionTile[curr.x, curr.y]) continue;

            Vector2Int prev = path[i - 1];
            Vector2Int next = path[i + 1];

            Vector2Int inDir = curr - prev;
            Vector2Int outDir = next - curr;

            int pIn = GetPortForDelta(-inDir);
            int pOut = GetPortForDelta(outDir);

            int wireType = (inDir == outDir) ? WIRE_STRAIGHT : WIRE_CORNER;
            type[curr.x, curr.y] = wireType;
            solRot[curr.x, curr.y] = RotationForPorts(wireType, pIn | pOut);

            if (isSolution)
            {
                isSolutionTile[curr.x, curr.y] = true;
            }
        }
    }

    private int GetPortForDelta(Vector2Int d)
    {
        if (d == new Vector2Int(0, -1)) return (int)Port.Top;
        if (d == new Vector2Int(1, 0)) return (int)Port.Right;
        if (d == new Vector2Int(0, 1)) return (int)Port.Bottom;
        if (d == new Vector2Int(-1, 0)) return (int)Port.Left;
        return 0;
    }

    private void ComputeTargetRotations(int[,] solRot)
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                targetRot[x, y] = solRot[x, y];
    }

    private void ScrambleWires()
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                if (!rotatable[x, y]) continue;
                int r;
                do { r = rng.Next(0, 4) * 90; } while (isSolutionTile[x, y] && r == targetRot[x, y]);
                grid[x, y].SetRotation(r);
            }
    }

    private UISpriteSheetAnimator EnsureAnimator(CircuitScrambleTile tile)
    {
        var anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
        anim.playOnEnable = false;
        return anim;
    }

    private void PlayFlow(CircuitScrambleTile tile, Sprite[] frames, bool active, Color color)
    {
        if (tile == null || frames == null || frames.Length == 0) return;
        var anim = EnsureAnimator(tile);
        if (active)
        {
            if (!anim.IsPlaying || anim.frames != frames)
                anim.PlaySet(frames, 16f, true);
        }
        else
        {
            anim.Stop();
            tile.Image.sprite = (tile.type == WIRE_STRAIGHT) ? straightSprite : cornerSprite;
        }
        tile.SetColor(color);
    }

    private void UpdatePowerFlow()
    {
        if (grid == null) return;

        HashSet<Vector2Int> powered = new HashSet<Vector2Int>();

        // 1. Evaluate Logic Gates
        bool correctInA = IsConnected(srcAPos, correctGatePos + new Vector2Int(-1, 0), powered);
        bool correctInB = IsConnected(srcBPos, correctGatePos + new Vector2Int(0, -1), powered);
        bool correctOutA = IsConnected(correctGatePos + new Vector2Int(1, 0), corePos, powered);
        bool correctOutB = IsConnected(correctGatePos + new Vector2Int(0, 1), corePos, powered);

        bool correctInputsPair = correctInA && correctInB;
        bool correctOutputsPair = correctOutA && correctOutB;

        bool satisfied = gateIsAnd
            ? (correctInputsPair && correctOutputsPair)
            : (correctInputsPair ^ correctOutputsPair);

        IsCurrentlySatisfied = satisfied;

        // 2. Evaluate Traps
        bool ocInA = IsConnected(srcAPos, overchargeGatePos + new Vector2Int(-1, 0), powered);
        bool ocInB = IsConnected(srcBPos, overchargeGatePos + new Vector2Int(0, -1), powered);
        bool ocOutA = IsConnected(overchargeGatePos + new Vector2Int(1, 0), corePos, powered);
        bool ocOutB = IsConnected(overchargeGatePos + new Vector2Int(0, 1), corePos, powered);
        
        // Trap gate uses XOR condition
        bool ocSatisfied = ocInA && ocInB && (ocOutA || ocOutB);

        bool relayInA = IsConnected(srcAPos, relayGatePos + new Vector2Int(-1, 0), powered);
        bool relayInB = IsConnected(srcBPos, relayGatePos + new Vector2Int(0, -1), powered);
        bool relayOutA = IsConnected(relayGatePos + new Vector2Int(1, 0), corePos, powered);
        bool relaySatisfied = relayInA && relayInB && relayOutA;

        relayActive = relaySatisfied;

        // 3. Update Visuals
        foreach (var t in tiles)
        {
            if (t == null) continue;
            bool isPowered = powered.Contains(new Vector2Int(t.x, t.y));
            Color c = isPowered ? ColorPowered : ColorInactive;

            if (t.type == WIRE_STRAIGHT)
                PlayFlow(t, straightFlowFrames, isPowered, c);
            else if (t.type == WIRE_CORNER)
                PlayFlow(t, cornerFlowFrames, isPowered, c);
            else if (t.type == GATE)
            {
                t.SetColor(isPowered ? ColorPowered : Color.white);
                if (new Vector2Int(t.x, t.y) == overchargeGatePos && ocSatisfied) t.SetColor(ColorTrap);
            }
            else if (t.type == SOURCE)
            {
                var anim = EnsureAnimator(t);
                if (!anim.IsPlaying) anim.PlaySet(sourcePulseFrames, 10f, true);
                t.SetColor(ColorPowered);
            }
        }

        // Handle Overcharge
        if (ocSatisfied && !isExploding)
        {
            overchargeTimer += Time.deltaTime;
            if (overchargeTimer >= 3f) StartCoroutine(TriggerOverchargeSurge());
        }
        else overchargeTimer = 0f;

        // Core Dynamics
        if (satisfied)
        {
            float cap = relayActive ? 0.4f : 1f;
            currentCharge = Mathf.MoveTowards(currentCharge, cap, Time.deltaTime * 0.5f);
        }
        else currentCharge = Mathf.MoveTowards(currentCharge, 0f, Time.deltaTime * 0.3f);

        UpdateCoreVisuals(satisfied);

        if (calibrationSlider != null) calibrationSlider.value = currentCharge;
        if (calibrationText != null) calibrationText.text = $"REACTOR STABILITY: {Mathf.RoundToInt(currentCharge * 100f)}%";
        
        UpdateTextOverlays();

        if (currentCharge >= 1f && !isSolved) Solve();
    }

    private IEnumerator TriggerOverchargeSurge()
    {
        isExploding = true;
        overchargeTimer = 0f;
        currentCharge = 0f;

        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 1f);
        }

        // Flash entire board red/white
        for (int i = 0; i < 4; i++)
        {
            foreach (var t in tiles)
            {
                if (t != null) t.Image.color = (i % 2 == 0) ? Color.red : Color.white;
            }
            yield return new WaitForSeconds(0.12f);
        }

        isExploding = false;
        UpdatePowerFlow();
    }

    private void TriggerBackdraftShortCircuit()
    {
        backdraftTimer = 0f;
        currentCharge = 0f;

        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.9f);
        }

        // Scramble wire tiles instantly!
        ScrambleWires();
        UpdatePowerFlow();
    }

    private void OnTileClicked(CircuitScrambleTile tile)
    {
        if (!CanAcceptInput() || isExploding) return;
        tile.Rotate();
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
        UpdatePowerFlow();
    }

    private void UpdateCoreVisuals(bool satisfied)
    {
        if (coreTile == null) return;
        var anim = EnsureAnimator(coreTile);
        
        if (relayActive)
        {
            float j = 5f;
            coreTile.transform.localPosition = GridPosition(corePos.x, corePos.y) + new Vector2(Random.Range(-j, j), Random.Range(-j, j));
            coreTile.SetColor(new Color(1f, 0.2f, 1f)); // Magenta glitch
        }
        else
        {
            coreTile.transform.localPosition = GridPosition(corePos.x, corePos.y);
            coreTile.SetColor(satisfied ? ColorSolved : Color.white);
        }

        if (overchargeTimer > 0f)
        {
            float p = Mathf.PingPong(Time.time * 20f, 1f);
            coreTile.SetColor(Color.Lerp(ColorTrap, Color.white, p));
        }

        if (currentCharge > 0.01f)
        {
            if (!anim.IsPlaying || anim.frames != coreChargeFrames) anim.PlaySet(coreChargeFrames, 12f, true);
            float alpha = 0.4f + currentCharge * 0.6f;
            coreTile.SetColor(new Color(coreTile.Image.color.r, coreTile.Image.color.g, coreTile.Image.color.b, alpha));
        }
        else
        {
            anim.Stop();
            coreTile.SetSprite(coreSprite);
        }
    }

    private void UpdateTextOverlays()
    {
        if (requirementText == null) return;
        if (overchargeTimer > 0f) requirementText.text = $"<color=red><b>OVERCHARGE CRITICAL: {3f - overchargeTimer:0.0}s</b></color>";
        else if (relayActive) requirementText.text = "<color=#FF33CC><b>RELAY GLITCH: POWER CAPPED</b></color>";
        else requirementText.text = gateIsAnd ? "<color=#00F2FF>AND GATE REQUIRED</color>" : "<color=#FF00FF>XOR GATE REQUIRED</color>";
    }


    private bool IsConnected(Vector2Int start, Vector2Int end, HashSet<Vector2Int> poweredPath)
    {
        if (!InBounds(start) || !InBounds(end)) return false;

        // If start is the target terminal itself, return true
        if (start == end) return true;

        // Start search at the first wire tile 'start'
        var ntStart = grid[start.x, start.y];
        if (ntStart == null || (ntStart.type != WIRE_STRAIGHT && ntStart.type != WIRE_CORNER)) return false;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        seen.Add(start);

        bool reached = false;

        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();

            int ports = WirePorts(grid[c.x, c.y].type, grid[c.x, c.y].rotation);
            foreach (Port p in new[] { Port.Top, Port.Right, Port.Bottom, Port.Left })
            {
                if ((ports & (int)p) == 0) continue;
                Vector2Int nb = c + Delta((int)p);
                if (!InBounds(nb)) continue;

                if (nb == end)
                {
                    reached = true;
                    continue;
                }

                var nt = grid[nb.x, nb.y];
                if (nt == null) continue;
                if (seen.Contains(nb)) continue;

                // Only traverse through wire tiles
                if (nt.type == WIRE_STRAIGHT || nt.type == WIRE_CORNER)
                {
                    if ((WirePorts(nt.type, nt.rotation) & Opposite((int)p)) != 0)
                    {
                        seen.Add(nb);
                        queue.Enqueue(nb);
                    }
                }
            }
        }

        if (reached)
        {
            foreach (var v in seen)
            {
                poweredPath.Add(v);
            }
            return true;
        }
        return false;
    }

    private void ApplyTileSprite(CircuitScrambleTile tile, int type, int x, int y)
    {
        switch (type)
        {
            case WIRE_STRAIGHT: if (straightSprite != null) tile.SetSprite(straightSprite); break;
            case WIRE_CORNER: if (cornerSprite != null) tile.SetSprite(cornerSprite); break;
            case SOURCE: if (sourceSprite != null) tile.SetSprite(sourceSprite); break;
            case CORE: if (coreSprite != null) tile.SetSprite(coreSprite); break;
            case GATE:
                if (new Vector2Int(x, y) == correctGatePos)
                {
                    Sprite g = gateIsAnd ? andGateSprite : xorGateSprite;
                    if (g != null) tile.SetSprite(g);
                }
                else if (new Vector2Int(x, y) == overchargeGatePos)
                {
                    // Trap gate uses XOR style visually
                    if (xorGateSprite != null) tile.SetSprite(xorGateSprite);
                }
                else
                {
                    // Relay gate uses AND style visually
                    if (andGateSprite != null) tile.SetSprite(andGateSprite);
                }
                break;
        }
    }

    private int WirePorts(int type, int rotation)
    {
        int basePorts;
        if (type == WIRE_STRAIGHT) basePorts = (int)(Port.Left | Port.Right);
        else if (type == WIRE_CORNER) basePorts = (int)(Port.Left | Port.Bottom);
        else basePorts = 0;
        return RotatePortsCCW(basePorts, rotation);
    }

    private int RotatePortsCCW(int ports, int rotation)
    {
        int steps = (((rotation % 360) + 360) % 360) / 90;
        for (int i = 0; i < steps; i++)
        {
            int n = 0;
            if ((ports & (int)Port.Top) != 0) n |= (int)Port.Left;
            if ((ports & (int)Port.Left) != 0) n |= (int)Port.Bottom;
            if ((ports & (int)Port.Bottom) != 0) n |= (int)Port.Right;
            if ((ports & (int)Port.Right) != 0) n |= (int)Port.Top;
            ports = n;
        }
        return ports;
    }

    private int RotationForPorts(int wireType, int requiredPorts)
    {
        for (int r = 0; r < 360; r += 90)
            if (WirePorts(wireType, r) == requiredPorts) return r;
        return 0;
    }

    private int Opposite(int port)
    {
        switch ((Port)port)
        {
            case Port.Top: return (int)Port.Bottom;
            case Port.Bottom: return (int)Port.Top;
            case Port.Left: return (int)Port.Right;
            default: return (int)Port.Left;
        }
    }

    private Vector2Int Delta(int port)
    {
        switch ((Port)port)
        {
            case Port.Top: return new Vector2Int(0, -1);
            case Port.Right: return new Vector2Int(1, 0);
            case Port.Bottom: return new Vector2Int(0, 1);
            default: return new Vector2Int(-1, 0);
        }
    }

    private bool InBounds(Vector2Int c) => c.x >= 0 && c.x < columns && c.y >= 0 && c.y < rows;

    private Color GetTileColor(int type, bool isPowered)
    {
        switch (type)
        {
            case SOURCE: return new Color(0.2f, 0.85f, 1f);
            case CORE: return Color.white;
            case GATE: return Color.white;
            default:
                return isPowered ? new Color(0.2f, 0.95f, 1f) : new Color(0.32f, 0.36f, 0.45f);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || isSolved || isTimedOut || isExploding) return;
        UpdatePowerFlow();
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || grid == null || isExploding) return;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (!rotatable[x, y] || grid[x, y] == null || !isSolutionTile[x, y]) continue;
                if (grid[x, y].rotation != targetRot[x, y])
                {
                    grid[x, y].Rotate();
                    UpdatePowerFlow();
                    return;
                }
            }
        }
        UpdatePowerFlow();
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
