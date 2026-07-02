using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mirror Maze (UI overlay version). Route a light beam from an EMITTER border
// cell to a RECEIVER border cell by rotating diagonal mirrors between '/' and
// '\'. Clicking a mirror toggles its orientation; the beam is re-traced and the
// puzzle solves when it reaches the receiver. Generation lays a guaranteed
// staircase solution, then scrambles the mirrors so it starts unsolved. Logic
// ported from the world-space MirrorBeam.cs and adapted to UI GridTiles.
public class MirrorMazePuzzle : BasePuzzle
{
    [Header("Mirror Maze")]
    public int gridSize = 5;
    public GameObject mirrorPrefab;
    public GameObject beamPrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 60f;

    [Header("Art")]
    public Sprite[] floorFrames;    // empty board cell
    public Sprite[] mirrorFrames;   // rotatable diagonal mirror
    public Sprite[] emitterFrames;  // beam source
    public Sprite[] receiverFrames; // beam target
    public Sprite[] prismFrames;    // splits beam 3-way
    public Sprite[] activatorFrames; // lights up and acts as secondary emitter
    public Sprite[] beamFrames;     // beam segment overlay (kept tinted)
    
    [Header("Old Sprites (Deprecated)")]
    public Sprite floorSprite;
    public Sprite mirrorSprite;
    public Sprite emitterSprite;
    public Sprite receiverSprite;
    public Sprite prismSprite;
    public Sprite activatorSprite;
    public Sprite beamSprite;

    public float beamStepDelay = 0.005f;

    private const int SLASH = 0;      // '/'
    private const int BACKSLASH = 1;  // '\'

    private const int FLOOR = 0;
    private const int EMITTER = 1;
    private const int RECEIVER = 2;
    private const int MIRROR = 3;
    private const int PRISM = 4;
    private const int ACTIVATOR = 5;
    private const int FILTER = 6;
    private const int ENERGY_NODE = 7;

    private static readonly Color FloorColor = new Color(0.20f, 0.23f, 0.32f, 1f);
    private static readonly Color MirrorColor = new Color(0.90f, 0.92f, 1f, 1f);
    private static readonly Color PrismColor = new Color(0.8f, 0.8f, 1f, 1f);
    private static readonly Color EmitterColor = new Color(0f, 0.95f, 1f, 1f);
    private static readonly Color ReceiverColor = new Color(1f, 0.75f, 0.20f, 1f);
    private static readonly Color ActivatorOff = new Color(0.4f, 0.4f, 0.4f, 1f);
    private static readonly Color ActivatorOn = new Color(1f, 0.95f, 0.2f, 1f);
    private static readonly Color BeamOn = new Color(0f, 0.95f, 1f, 0.85f);
    private static readonly Color BeamSolved = new Color(0.61f, 1f, 0.18f, 0.95f);

    private int boardSize;
    private GridTile[,] cellTiles; // [y, x]
    private int[,] orient;         // [y, x]
    private readonly List<GridTile> interactableTiles = new List<GridTile>();
    private readonly List<GridTile> requiredEndpoints = new List<GridTile>();
    private readonly HashSet<Vector2Int> claimedEnergyNodes = new HashSet<Vector2Int>();

    private Vector2Int emitterPos;
    private Vector2Int emitDir;
    private Coroutine beamCoroutine;

    private struct BeamHead
    {
        public Vector2Int pos;
        public Vector2Int dir;
        public Color color;
        public BeamHead(Vector2Int p, Vector2Int d, Color col) { pos = p; dir = d; color = col; }
    }

    private RectTransform beamContainer;

    private void SetupAnimation(GridTile tile, Sprite[] frames, Sprite fallback, bool loopAnim = true)
    {
        if (frames != null && frames.Length > 0)
        {
            UISpriteSheetAnimator anim = tile.GetComponent<UISpriteSheetAnimator>();
            if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
            anim.PlaySet(frames, 10f, loopAnim);
            tile.SetColor(Color.white);
        }
        else if (fallback != null)
        {
            tile.SetSprite(fallback);
            tile.SetColor(Color.white);
        }
    }

    // ----------------------------------------------------------------- build
    protected override void BuildBoard()
    {
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }

        boardSize = Mathf.Max(5, gridSize);
        cellTiles = new GridTile[boardSize, boardSize];
        orient = new int[boardSize, boardSize];
        interactableTiles.Clear();
        requiredEndpoints.Clear();
        claimedEnergyNodes.Clear();

        // Emitter on left, Receiver on right
        int ery = rng.Next(1, boardSize - 1);
        emitterPos = new Vector2Int(0, ery);
        emitDir = new Vector2Int(1, 0);

        int try_y = rng.Next(1, boardSize - 1);
        Vector2Int receiverPos = new Vector2Int(boardSize - 1, try_y);

        // Generation: Emitter -> Mirror1 -> Activator -> Mirror2 -> Receiver
        int mx1 = rng.Next(1, boardSize / 2);
        int mx2 = rng.Next(boardSize / 2 + 1, boardSize - 1);
        int ay = (ery + try_y) / 2;

        Dictionary<Vector2Int, (int type, int orient)> plan = new Dictionary<Vector2Int, (int, int)>();
        
        int vSign1 = ay > ery ? 1 : -1;
        plan[new Vector2Int(mx1, ery)] = (MIRROR, vSign1 == 1 ? BACKSLASH : SLASH);
        plan[new Vector2Int(mx1, ay)] = (ACTIVATOR, 0);
        
        int vSign2 = try_y > ay ? 1 : -1;
        plan[new Vector2Int(mx2, ay)] = (MIRROR, vSign2 == 1 ? BACKSLASH : SLASH);
        plan[new Vector2Int(mx2, try_y)] = (MIRROR, vSign2 == 1 ? SLASH : BACKSLASH);

        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int> { emitterPos, receiverPos };
        for (int x = 0; x <= mx1; x++) pathSet.Add(new Vector2Int(x, ery));
        for (int y = Mathf.Min(ery, ay); y <= Mathf.Max(ery, ay); y++) pathSet.Add(new Vector2Int(mx1, y));
        for (int x = mx1; x <= mx2; x++) pathSet.Add(new Vector2Int(x, ay));
        for (int y = Mathf.Min(ay, try_y); y <= Mathf.Max(ay, try_y); y++) pathSet.Add(new Vector2Int(mx2, y));
        for (int x = mx2; x < boardSize; x++) pathSet.Add(new Vector2Int(x, try_y));

        // Place a Color Filter on the horizontal path segment between Mirror 1 and Mirror 2
        int filterX = (mx1 + mx2) / 2;
        Vector2Int filterPos = new Vector2Int(filterX, ay);
        if (!plan.ContainsKey(filterPos))
        {
            plan[filterPos] = (FILTER, 0);
        }

        int prismCount = difficulty > 2 ? 1 : 0;
        int guard = 0;
        while (prismCount > 0 && guard++ < 100)
        {
            Vector2Int p = new Vector2Int(rng.Next(1, boardSize - 1), rng.Next(1, boardSize - 1));
            if (pathSet.Contains(p) || plan.ContainsKey(p)) continue;
            plan[p] = (PRISM, 0);
            prismCount--;
        }

        // Place 1-2 optional Energy Nodes (Side Objectives)
        int energyNodeCount = difficulty > 1 ? 2 : 1;
        guard = 0;
        while (energyNodeCount > 0 && guard++ < 100)
        {
            Vector2Int p = new Vector2Int(rng.Next(1, boardSize - 1), rng.Next(1, boardSize - 1));
            if (pathSet.Contains(p) || plan.ContainsKey(p)) continue;
            plan[p] = (ENERGY_NODE, 0);
            energyNodeCount--;
        }

        float center = (boardSize - 1) * tileSpacing / 2f;
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                GameObject go = Instantiate(mirrorPrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(x * tileSpacing - center, -(y * tileSpacing) + center);

                GridTile gt = go.GetComponent<GridTile>();
                Vector2Int gp = new Vector2Int(x, y);
                cellTiles[y, x] = gt;

                if (gp == emitterPos)
                {
                    gt.Init(x, y, EMITTER);
                    gt.SetColor(EmitterColor);
                    SetupAnimation(gt, emitterFrames, emitterSprite);
                }
                else if (gp == receiverPos)
                {
                    gt.Init(x, y, RECEIVER);
                    gt.SetColor(ReceiverColor);
                    SetupAnimation(gt, receiverFrames, receiverSprite);
                    requiredEndpoints.Add(gt);
                }
                else if (plan.TryGetValue(gp, out var pInfo))
                {
                    gt.Init(x, y, pInfo.type);
                    orient[y, x] = pInfo.orient;
                    if (pInfo.type == MIRROR)
                    {
                        gt.SetColor(MirrorColor);
                        SetupAnimation(gt, mirrorFrames, mirrorSprite);
                        gt.SetRotation(pInfo.orient == BACKSLASH ? 90f : 0f);
                        gt.OnClicked += HandleClick;
                        interactableTiles.Add(gt);
                    }
                    else if (pInfo.type == PRISM)
                    {
                        gt.SetColor(PrismColor);
                        SetupAnimation(gt, prismFrames, prismSprite);
                    }
                    else if (pInfo.type == ACTIVATOR)
                    {
                        gt.SetColor(ActivatorOff);
                        SetupAnimation(gt, activatorFrames, activatorSprite);
                        requiredEndpoints.Add(gt);
                    }
                    else if (pInfo.type == FILTER)
                    {
                        gt.SetColor(new Color(1f, 0.85f, 0f, 1f)); // Gold/Orange Filter
                        SetupAnimation(gt, prismFrames, prismSprite); // Use prism frames
                    }
                    else if (pInfo.type == ENERGY_NODE)
                    {
                        gt.SetColor(new Color(1f, 0.2f, 0.6f, 1f)); // Magenta/Pink Energy Node
                        SetupAnimation(gt, activatorFrames, activatorSprite);
                    }
                }
                else
                {
                    gt.Init(x, y, FLOOR);
                    gt.SetColor(FloorColor);
                    SetupAnimation(gt, floorFrames, floorSprite, false); // Floor doesn't loop
                }
            }
        }

        GameObject beamGo = new GameObject("BeamLayer", typeof(RectTransform));
        beamContainer = (RectTransform)beamGo.transform;
        beamContainer.SetParent(gridContainer, false);
        beamContainer.anchoredPosition = Vector2.zero;
        beamContainer.localScale = Vector3.one;

        Scramble();
        Retrace();
    }

    private void Scramble()
    {
        if (interactableTiles.Count == 0) return;
        int safety = 0;
        HashSet<GridTile> hit = new HashSet<GridTile>();
        do
        {
            foreach (var m in interactableTiles)
            {
                int o = rng.Next(0, 2);
                orient[m.y, m.x] = o;
                m.SetRotation(o == BACKSLASH ? 90f : 0f);
            }
            safety++;
        }
        while (TraceBeamStatic(hit) && safety < 60);
    }

    private Vector2Int Reflect(Vector2Int d, int o)
    {
        return o == BACKSLASH ? new Vector2Int(d.y, d.x) : new Vector2Int(-d.y, -d.x);
    }

    private Vector2Int RotateLeft(Vector2Int d) => new Vector2Int(d.y, -d.x);
    private Vector2Int RotateRight(Vector2Int d) => new Vector2Int(-d.y, d.x);

    private bool TraceBeamStatic(HashSet<GridTile> triggered)
    {
        triggered.Clear();
        List<BeamHead> heads = new List<BeamHead> { new BeamHead(emitterPos, emitDir, EmitterColor) };
        HashSet<(Vector2Int, Vector2Int, Color)> visited = new HashSet<(Vector2Int, Vector2Int, Color)>();

        int limit = boardSize * boardSize * 4;
        while (heads.Count > 0 && limit-- > 0)
        {
            List<BeamHead> nextHeads = new List<BeamHead>();
            foreach (var h in heads)
            {
                Vector2Int nextPos = h.pos + h.dir;
                if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= boardSize || nextPos.y >= boardSize) continue;
                if (visited.Contains((nextPos, h.dir, h.color))) continue;
                visited.Add((nextPos, h.dir, h.color));

                GridTile tile = cellTiles[nextPos.y, nextPos.x];
                if (tile.type == MIRROR)
                    nextHeads.Add(new BeamHead(nextPos, Reflect(h.dir, orient[nextPos.y, nextPos.x]), h.color));
                else if (tile.type == PRISM)
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                    nextHeads.Add(new BeamHead(nextPos, RotateLeft(h.dir), h.color));
                    nextHeads.Add(new BeamHead(nextPos, RotateRight(h.dir), h.color));
                }
                else if (tile.type == FILTER)
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, ReceiverColor));
                }
                else if (tile.type == ACTIVATOR)
                {
                    triggered.Add(tile);
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
                else if (tile.type == RECEIVER)
                {
                    if (h.color == ReceiverColor)
                        triggered.Add(tile);
                }
                else if (tile.type == ENERGY_NODE)
                {
                    triggered.Add(tile); // track triggering, but do not propagate
                }
                else
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
            }
            heads = nextHeads;
        }

        foreach (var req in requiredEndpoints)
        {
            if (!triggered.Contains(req)) return false;
        }
        return true;
    }

    private void Retrace()
    {
        if (beamCoroutine != null) StopCoroutine(beamCoroutine);
        
        if (beamContainer != null)
        {
            // Use DestroyImmediate to avoid overlapping segments during rapid clicks (flicker)
            for (int i = beamContainer.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(beamContainer.GetChild(i).gameObject);
            }
        }
        
        foreach (var endpoint in requiredEndpoints)
        {
            if (endpoint.type == ACTIVATOR) endpoint.SetColor(ActivatorOff);
        }

        foreach (var tile in cellTiles)
        {
            if (tile != null && tile.type == ENERGY_NODE)
            {
                tile.SetColor(new Color(1f, 0.2f, 0.6f, 1f)); // Reset to off pink
            }
        }

        List<BeamHead> heads = new List<BeamHead> { new BeamHead(emitterPos, emitDir, EmitterColor) };
        HashSet<GridTile> triggered = new HashSet<GridTile>();
        HashSet<(Vector2Int, Vector2Int, Color)> visited = new HashSet<(Vector2Int, Vector2Int, Color)>();

        float center = (boardSize - 1) * tileSpacing / 2f;
        int limit = boardSize * boardSize * 4;
        
        while (heads.Count > 0 && limit-- > 0)
        {
            List<BeamHead> nextHeads = new List<BeamHead>();
            foreach (var h in heads)
            {
                Vector2Int nextPos = h.pos + h.dir;
                if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= boardSize || nextPos.y >= boardSize) continue;
                if (visited.Contains((nextPos, h.dir, h.color))) continue;
                visited.Add((nextPos, h.dir, h.color));

                if (beamPrefab != null)
                {
                    GameObject b = Instantiate(beamPrefab, beamContainer);
                    RectTransform rt = (RectTransform)b.transform;
                    rt.anchoredPosition = new Vector2(nextPos.x * tileSpacing - center, -(nextPos.y * tileSpacing) + center);
                    rt.sizeDelta = new Vector2(tileSpacing * 0.6f, tileSpacing * 0.6f);
                    GridTile bt = b.GetComponent<GridTile>();
                    if (bt != null)
                    {
                        bt.Init(nextPos.x, nextPos.y, 9);
                        bt.SetColor(h.color);
                        SetupAnimation(bt, beamFrames, beamSprite);
                        bt.Image.raycastTarget = false;
                    }
                }

                GridTile tile = cellTiles[nextPos.y, nextPos.x];
                if (tile.type == MIRROR)
                    nextHeads.Add(new BeamHead(nextPos, Reflect(h.dir, orient[nextPos.y, nextPos.x]), h.color));
                else if (tile.type == PRISM)
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                    nextHeads.Add(new BeamHead(nextPos, RotateLeft(h.dir), h.color));
                    nextHeads.Add(new BeamHead(nextPos, RotateRight(h.dir), h.color));
                }
                else if (tile.type == FILTER)
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, ReceiverColor));
                }
                else if (tile.type == ACTIVATOR)
                {
                    triggered.Add(tile);
                    tile.SetColor(ActivatorOn);
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
                else if (tile.type == RECEIVER)
                {
                    if (h.color == ReceiverColor)
                        triggered.Add(tile);
                }
                else if (tile.type == ENERGY_NODE)
                {
                    triggered.Add(tile);
                    tile.SetColor(new Color(1f, 0.5f, 0.8f, 1f)); // Glowing active magenta
                    
                    Vector2Int pos = new Vector2Int(tile.x, tile.y);
                    if (!claimedEnergyNodes.Contains(pos))
                    {
                        claimedEnergyNodes.Add(pos);
                        timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 5f);
                        Debug.Log("Energy Node Charged! +5 Seconds Bonus!");
                        StartCoroutine(PulseMarker(tile.transform as RectTransform));
                    }
                }
                else
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
            }
            heads = nextHeads;
        }

        bool allRequiredTriggered = true;
        foreach (var req in requiredEndpoints)
        {
            if (!triggered.Contains(req))
            {
                allRequiredTriggered = false;
                break;
            }
        }

        if (allRequiredTriggered)
        {
            foreach (Transform child in beamContainer)
            {
                GridTile bt = child.GetComponent<GridTile>();
                if (bt != null) bt.SetColor(BeamSolved);
            }
            if (!isSolved) Solve();
        }
    }

    private IEnumerator AnimateBeam()
    {
        yield break;
    }

    private void HandleClick(GridTile t)
    {
        if (!CanAcceptInput()) return;
        orient[t.y, t.x] ^= 1;
        t.SetRotation(orient[t.y, t.x] == BACKSLASH ? 90f : 0f);
        Retrace();
    }

    private IEnumerator PulseMarker(RectTransform marker)
    {
        if (marker == null) yield break;
        Vector3 originalScale = marker.localScale;
        Vector3 peakScale = originalScale * 1.3f;
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

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;
        int m = interactableTiles.Count;
        if (m == 0) { Retrace(); return; }

        int[] sol = FindSolution();
        if (sol == null) return;

        for (int i = 0; i < m; i++)
        {
            GridTile t = interactableTiles[i];
            if (orient[t.y, t.x] != sol[i])
            {
                orient[t.y, t.x] = sol[i];
                t.SetRotation(sol[i] == BACKSLASH ? 90f : 0f);
                Retrace();
                return;
            }
        }
    }

    private int[] FindSolution()
    {
        int m = interactableTiles.Count;
        int[] saved = new int[m];
        for (int i = 0; i < m; i++) saved[i] = orient[interactableTiles[i].y, interactableTiles[i].x];

        int[] result = null;
        HashSet<GridTile> hit = new HashSet<GridTile>();

        if (m <= 15)
        {
            for (int c = 0; c < (1 << m); c++)
            {
                for (int b = 0; b < m; b++) orient[interactableTiles[b].y, interactableTiles[b].x] = (c >> b) & 1;
                if (TraceBeamStatic(hit))
                {
                    result = new int[m];
                    for (int b = 0; b < m; b++) result[b] = (c >> b) & 1;
                    break;
                }
            }
        }
        else
        {
            int tries = 2000;
            while (tries-- > 0)
            {
                for (int b = 0; b < m; b++) orient[interactableTiles[b].y, interactableTiles[b].x] = rng.Next(0, 2);
                if (TraceBeamStatic(hit))
                {
                    result = new int[m];
                    for (int b = 0; b < m; b++) result[b] = orient[interactableTiles[b].y, interactableTiles[b].x];
                    break;
                }
            }
        }

        for (int i = 0; i < m; i++) orient[interactableTiles[i].y, interactableTiles[i].x] = saved[i];
        return result;
    }
}
