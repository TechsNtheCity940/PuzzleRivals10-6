using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Sprite[] mirrorFrames;   // (legacy animated sheet - no longer looped)
    public Sprite[] emitterFrames;  // beam source (always-on pulse)
    public Sprite[] receiverFrames; // beam target (dark->bright charge)
    public Sprite[] prismFrames;    // (legacy animated sheet - no longer looped)
    public Sprite[] activatorFrames; // lights up when hit
    public Sprite[] beamFrames;     // beam segment overlay (kept tinted)

    [Header("Clean static art")]
    public Sprite cleanMirrorSprite; // static mirror; rotated by transform, never frame-animated
    public Sprite cleanPrismSprite;  // static prism crystal; beam drawn separately
    
    [Header("Old Sprites (Deprecated)")]
    public Sprite floorSprite;
    public Sprite mirrorSprite;
    public Sprite emitterSprite;
    public Sprite receiverSprite;
    public Sprite prismSprite;
    public Sprite activatorSprite;
    public Sprite beamSprite;

    public float beamStepDelay = 0.01f;
    public float beamThickness = 12f;

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

    private static readonly Color FloorColor = new Color(0.07f, 0.07f, 0.07f, 1f); // Deep Charcoal #121212
    private static readonly Color MirrorColor = new Color(0.90f, 0.92f, 1f, 1f);
    private static readonly Color MirrorActiveColor = new Color(0f, 1f, 1f, 1f); // Neon Cyan glow rim
    private static readonly Color PrismColor = new Color(0.8f, 0.8f, 1f, 1f);
    private static readonly Color EmitterColor = new Color(0f, 0.95f, 1f, 1f);
    private static readonly Color ReceiverColor = new Color(1f, 0.75f, 0.20f, 1f);
    private static readonly Color ActivatorOff = new Color(0.4f, 0.4f, 0.4f, 1f);
    private static readonly Color ActivatorOn = new Color(1f, 0.95f, 0.2f, 1f);
    private static readonly Color BeamOn = new Color(0f, 0.95f, 1f, 0.9f);
    private static readonly Color BeamSolved = new Color(0.2f, 1f, 0.4f, 1f); // Lime Green

    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, -1), // Up (0)
        new Vector2Int(1, 0),  // Right (1)
        new Vector2Int(0, 1),  // Down (2)
        new Vector2Int(-1, 0)  // Left (3)
    };

    private static readonly Color[] LinkColors = new Color[] {
        new Color(1f, 0.2f, 0.6f, 1f),    // Neon pink
        new Color(0.2f, 0.8f, 1f, 1f),    // Neon cyan
        new Color(1f, 0.9f, 0.2f, 1f),    // Neon yellow
        new Color(0.3f, 1f, 0.3f, 1f)     // Neon green
    };

    private int boardSize;
    private GridTile[,] cellTiles; // [y, x]
    private int[,] orient;         // [y, x]
    private readonly List<GridTile> interactableTiles = new List<GridTile>();
    private readonly List<GridTile> requiredEndpoints = new List<GridTile>();
    private readonly HashSet<Vector2Int> claimedEnergyNodes = new HashSet<Vector2Int>();
    private readonly List<List<GridTile>> groups = new List<List<GridTile>>();

    private Vector2Int emitterPos;
    private Vector2Int emitDir;
    private Coroutine animateRetraceCoroutine;

    private readonly Dictionary<GridTile, Coroutine> rotateTweens = new Dictionary<GridTile, Coroutine>();
    private readonly Dictionary<GridTile, float> mirrorAngle = new Dictionary<GridTile, float>();

    private struct BeamHead
    {
        public Vector2Int pos;
        public Vector2Int dir;
        public Color color;
        public BeamHead(Vector2Int p, Vector2Int d, Color col) { pos = p; dir = d; color = col; }
    }

    private struct GridTileTemp
    {
        public int x;
        public int y;
        public int type;
        public GridTileTemp(int x, int y, int type) { this.x = x; this.y = y; this.type = type; }
    }

    private RectTransform beamContainer;

    private UISpriteSheetAnimator EnsureAnimator(GridTile tile)
    {
        var anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
        anim.playOnEnable = false; 
        return anim;
    }

    private void PlayLoop(GridTile tile, Sprite[] frames, float fps, Color? tint = null)
    {
        if (tile == null || frames == null || frames.Length == 0) return;
        var anim = EnsureAnimator(tile);
        if (!anim.IsPlaying || anim.frames != frames)
            anim.PlaySet(frames, fps, true);
        tile.SetColor(tint ?? Color.white);
    }

    private void ShowStatic(GridTile tile, Sprite[] frames, Color tint, Sprite fallback = null)
    {
        if (tile == null) return;
        var anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim != null) { anim.playOnEnable = false; anim.Stop(); }
        Sprite s = (frames != null && frames.Length > 0) ? frames[0] : fallback;
        if (s != null) tile.SetSprite(s);
        tile.SetColor(tint);
    }

    private Vector2Int GetPrismBackDir(int prismOrient)
    {
        switch (prismOrient % 4)
        {
            case 0: return new Vector2Int(0, 1);   // Down
            case 1: return new Vector2Int(-1, 0);  // Left
            case 2: return new Vector2Int(0, -1);  // Up
            default: return new Vector2Int(1, 0);  // Right
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

        boardSize = Mathf.Max(7, gridSize);

        if (gridContainer != null)
        {
            float containerMin = Mathf.Min(gridContainer.rect.width, gridContainer.rect.height);
            if (containerMin > 1f)
                tileSpacing = containerMin * 0.95f / boardSize;
        }

        cellTiles = new GridTile[boardSize, boardSize];
        orient = new int[boardSize, boardSize];
        interactableTiles.Clear();
        requiredEndpoints.Clear();
        claimedEnergyNodes.Clear();
        groups.Clear();

        // 1. Determine element distribution based on difficulty
        int numMirrors = 6 + Mathf.Clamp(difficulty, 0, 2);
        int numPrisms = (difficulty >= 2) ? 2 : 1;
        int numActivators = (difficulty >= 1) ? 2 : 1;
        int numLockedPairs = Mathf.Clamp(difficulty, 0, 2);

        bool generationSuccess = false;
        int maxGenerationAttempts = 500;

        Dictionary<Vector2Int, int> plan = new Dictionary<Vector2Int, int>();

        while (!generationSuccess && maxGenerationAttempts-- > 0)
        {
            plan.Clear();
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            int[,] tempTypes = new int[boardSize, boardSize];

            // Left edge emitter
            int ery = rng.Next(2, boardSize - 2);
            emitterPos = new Vector2Int(0, ery);
            emitDir = new Vector2Int(1, 0);
            occupied.Add(emitterPos);
            tempTypes[ery, 0] = EMITTER;

            // Right edge receiver
            int rry = rng.Next(2, boardSize - 2);
            Vector2Int receiverPos = new Vector2Int(boardSize - 1, rry);
            occupied.Add(receiverPos);
            tempTypes[rry, boardSize - 1] = RECEIVER;

            // Place Prisms
            int placedPrisms = 0;
            int prismGuard = 0;
            while (placedPrisms < numPrisms && prismGuard++ < 100)
            {
                int px = rng.Next(2, boardSize - 3);
                int py = rng.Next(1, boardSize - 2);
                Vector2Int pos = new Vector2Int(px, py);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = PRISM;
                    occupied.Add(pos);
                    tempTypes[py, px] = PRISM;
                    placedPrisms++;
                }
            }

            // Place early Color Filter
            int placedFilters = 0;
            int filterGuard = 0;
            while (placedFilters < 1 && filterGuard++ < 100)
            {
                int fx = rng.Next(1, boardSize - 3);
                int fy = rng.Next(1, boardSize - 2);
                Vector2Int pos = new Vector2Int(fx, fy);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = FILTER;
                    occupied.Add(pos);
                    tempTypes[fy, fx] = FILTER;
                    placedFilters++;
                }
            }

            // Place Activators (Endpoints)
            int placedActivators = 0;
            int actGuard = 0;
            while (placedActivators < numActivators && actGuard++ < 100)
            {
                int ax = rng.Next(1, boardSize - 2);
                int ay = rng.Next(1, boardSize - 2);
                Vector2Int pos = new Vector2Int(ax, ay);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = ACTIVATOR;
                    occupied.Add(pos);
                    tempTypes[ay, ax] = ACTIVATOR;
                    placedActivators++;
                }
            }

            // Place Energy Nodes (Bonus time)
            int numNodes = (difficulty > 1) ? 2 : 1;
            int placedNodes = 0;
            int nodeGuard = 0;
            while (placedNodes < numNodes && nodeGuard++ < 100)
            {
                int nx = rng.Next(1, boardSize - 2);
                int ny = rng.Next(1, boardSize - 2);
                Vector2Int pos = new Vector2Int(nx, ny);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = ENERGY_NODE;
                    occupied.Add(pos);
                    tempTypes[ny, nx] = ENERGY_NODE;
                    placedNodes++;
                }
            }

            // Place Mirrors
            int placedMirrors = 0;
            int mirrorGuard = 0;
            while (placedMirrors < numMirrors && mirrorGuard++ < 200)
            {
                int mx = rng.Next(1, boardSize - 2);
                int my = rng.Next(1, boardSize - 2);
                Vector2Int pos = new Vector2Int(mx, my);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = MIRROR;
                    occupied.Add(pos);
                    tempTypes[my, mx] = MIRROR;
                    placedMirrors++;
                }
            }

            if (placedMirrors < numMirrors || placedPrisms < numPrisms || placedActivators < numActivators)
                continue;

            // Run Solver Quality Verification
            List<GridTileTemp> tempInteractables = new List<GridTileTemp>();
            List<GridTileTemp> tempEndpoints = new List<GridTileTemp>();

            GridTileTemp receiverTemp = new GridTileTemp(receiverPos.x, receiverPos.y, RECEIVER);
            tempEndpoints.Add(receiverTemp);

            foreach (var kv in plan)
            {
                var gp = kv.Key;
                var type = kv.Value;
                if (type == MIRROR || type == PRISM)
                {
                    tempInteractables.Add(new GridTileTemp(gp.x, gp.y, type));
                }
                else if (type == ACTIVATOR)
                {
                    tempEndpoints.Add(new GridTileTemp(gp.x, gp.y, type));
                }
            }

            // Shuffle pool to build randomized linked groups
            List<GridTileTemp> pool = new List<GridTileTemp>(tempInteractables);
            for (int i = 0; i < pool.Count; i++)
            {
                int r = rng.Next(i, pool.Count);
                var temp = pool[i]; pool[i] = pool[r]; pool[r] = temp;
            }

            List<List<GridTileTemp>> tempGroups = new List<List<GridTileTemp>>();
            int pCreated = 0;
            while (pCreated < numLockedPairs && pool.Count >= 2)
            {
                var g1 = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1);
                var g2 = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1);
                tempGroups.Add(new List<GridTileTemp> { g1, g2 });
                pCreated++;
            }
            while (pool.Count > 0)
            {
                var g = pool[pool.Count - 1]; pool.RemoveAt(pool.Count - 1);
                tempGroups.Add(new List<GridTileTemp> { g });
            }

            int solutionCount = 0;
            List<int[]> solutions = new List<int[]>();
            int[] currentStates = new int[tempGroups.Count];
            int[,] tempOrient = new int[boardSize, boardSize];

            SearchCombinationsTemp(0, tempGroups, currentStates, ref solutionCount, solutions, tempOrient, tempEndpoints, receiverPos, tempTypes);

            if (solutionCount > 0 && solutionCount <= 4)
            {
                int[] selectedSol = solutions[rng.Next(0, solutions.Count)];
                int[] scrambledStates = new int[tempGroups.Count];
                bool isScrambledSolved = true;
                int safety = 0;

                while (isScrambledSolved && safety++ < 100)
                {
                    int diffClicks = 0;
                    for (int i = 0; i < tempGroups.Count; i++)
                    {
                        int maxS = (tempGroups[i][0].type == MIRROR) ? 2 : 4;
                        scrambledStates[i] = rng.Next(0, maxS);
                        if (scrambledStates[i] != selectedSol[i]) diffClicks++;
                    }

                    if (diffClicks >= 3)
                    {
                        for (int i = 0; i < tempGroups.Count; i++)
                        {
                            foreach (var tile in tempGroups[i])
                            {
                                tempOrient[tile.y, tile.x] = scrambledStates[i];
                            }
                        }
                        if (!TraceBeamStaticTemp(tempOrient, tempEndpoints, receiverPos, tempTypes))
                        {
                            isScrambledSolved = false;
                        }
                    }
                }

                if (!isScrambledSolved)
                {
                    generationSuccess = true;

                    float center = (boardSize - 1) * tileSpacing / 2f;
                    int pairCounter = 0;

                    for (int y = 0; y < boardSize; y++)
                    {
                        for (int x = 0; x < boardSize; x++)
                        {
                            GameObject go = Instantiate(mirrorPrefab, gridContainer);
                            RectTransform rt = (RectTransform)go.transform;
                            rt.anchoredPosition = new Vector2(x * tileSpacing - center, -(y * tileSpacing) + center);
                            rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

                            GridTile gt = go.GetComponent<GridTile>();
                            Vector2Int gp = new Vector2Int(x, y);
                            cellTiles[y, x] = gt;

                            if (gp == emitterPos)
                            {
                                gt.Init(x, y, EMITTER);
                                PlayLoop(gt, emitterFrames, 10f);
                                if (emitterFrames == null || emitterFrames.Length == 0)
                                    ShowStatic(gt, null, EmitterColor, emitterSprite);
                            }
                            else if (gp == receiverPos)
                            {
                                gt.Init(x, y, RECEIVER);
                                ShowStatic(gt, receiverFrames, ReceiverColor, receiverSprite);
                                requiredEndpoints.Add(gt);
                            }
                            else if (plan.TryGetValue(gp, out int type))
                            {
                                gt.Init(x, y, type);
                                if (type == MIRROR)
                                {
                                    ShowStatic(gt, mirrorFrames, MirrorColor, cleanMirrorSprite != null ? cleanMirrorSprite : mirrorSprite);
                                    interactableTiles.Add(gt);
                                }
                                else if (type == PRISM)
                                {
                                    ShowStatic(gt, prismFrames, PrismColor, cleanPrismSprite != null ? cleanPrismSprite : prismSprite);
                                    interactableTiles.Add(gt);
                                }
                                else if (type == ACTIVATOR)
                                {
                                    ShowStatic(gt, activatorFrames, ActivatorOff, activatorSprite);
                                    requiredEndpoints.Add(gt);
                                }
                                else if (type == FILTER)
                                {
                                    ShowStatic(gt, prismFrames, new Color(1f, 0.85f, 0f, 1f), prismSprite);
                                }
                                else if (type == ENERGY_NODE)
                                {
                                    ShowStatic(gt, activatorFrames, new Color(1f, 0.2f, 0.6f, 1f), activatorSprite);
                                }
                            }
                            else
                            {
                                gt.Init(x, y, FLOOR);
                                ShowStatic(gt, floorFrames, FloorColor, floorSprite);
                            }
                        }
                    }

                    foreach (var tempGrp in tempGroups)
                    {
                        List<GridTile> realGrp = new List<GridTile>();
                        foreach (var tempTile in tempGrp)
                        {
                            realGrp.Add(cellTiles[tempTile.y, tempTile.x]);
                        }
                        groups.Add(realGrp);

                        if (realGrp.Count > 1)
                        {
                            Color linkColor = LinkColors[pairCounter % LinkColors.Length];
                            pairCounter++;

                            foreach (var tile in realGrp)
                            {
                                if (tile.type == MIRROR)
                                {
                                    ShowStatic(tile, mirrorFrames, linkColor, cleanMirrorSprite != null ? cleanMirrorSprite : mirrorSprite);
                                }
                                else if (tile.type == PRISM)
                                {
                                    ShowStatic(tile, prismFrames, linkColor, cleanPrismSprite != null ? cleanPrismSprite : prismSprite);
                                }
                            }
                        }
                    }

                    for (int i = 0; i < groups.Count; i++)
                    {
                        int scrState = scrambledStates[i];
                        foreach (var tile in groups[i])
                        {
                            orient[tile.y, tile.x] = scrState;
                            float ang;
                            if (tile.type == MIRROR)
                            {
                                ang = (scrState == BACKSLASH) ? 90f : 0f;
                            }
                            else
                            {
                                ang = scrState * 90f;
                            }
                            mirrorAngle[tile] = ang;
                            tile.SetRotation(ang);

                            tile.OnClicked += HandleClick;
                        }
                    }
                }
            }
        }

        GameObject beamGo = new GameObject("BeamLayer", typeof(RectTransform));
        beamContainer = (RectTransform)beamGo.transform;
        beamContainer.SetParent(gridContainer, false);
        beamContainer.anchoredPosition = Vector2.zero;
        beamContainer.localScale = Vector3.one;
        beamContainer.SetAsLastSibling();

        Retrace();
    }

    private bool SearchCombinationsTemp(int groupIdx, List<List<GridTileTemp>> tempGroups, int[] currentStates, ref int solutionCount, List<int[]> solutions, int[,] tempOrient, List<GridTileTemp> tempEndpoints, Vector2Int receiverPos, int[,] tempTypes)
    {
        if (groupIdx >= tempGroups.Count)
        {
            if (TraceBeamStaticTemp(tempOrient, tempEndpoints, receiverPos, tempTypes))
            {
                solutionCount++;
                solutions.Add((int[])currentStates.Clone());
                if (solutionCount > 10) return false;
            }
            return true;
        }

        int maxStates = (tempGroups[groupIdx][0].type == MIRROR) ? 2 : 4;
        for (int s = 0; s < maxStates; s++)
        {
            currentStates[groupIdx] = s;
            foreach (var tile in tempGroups[groupIdx])
            {
                tempOrient[tile.y, tile.x] = s;
            }

            if (!SearchCombinationsTemp(groupIdx + 1, tempGroups, currentStates, ref solutionCount, solutions, tempOrient, tempEndpoints, receiverPos, tempTypes))
                return false;
        }
        return true;
    }

    private bool TraceBeamStaticTemp(int[,] tempOrient, List<GridTileTemp> tempEndpoints, Vector2Int receiverPos, int[,] tempTypes)
    {
        HashSet<Vector2Int> triggered = new HashSet<Vector2Int>();
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

                int type = tempTypes[nextPos.y, nextPos.x];

                if (type == MIRROR)
                    nextHeads.Add(new BeamHead(nextPos, Reflect(h.dir, tempOrient[nextPos.y, nextPos.x]), h.color));
                else if (type == PRISM)
                {
                    int prOrient = tempOrient[nextPos.y, nextPos.x] % 4;
                    Vector2Int backDir = GetPrismBackDir(prOrient);
                    foreach (Vector2Int d in DIRS)
                    {
                        if (d != backDir)
                        {
                            nextHeads.Add(new BeamHead(nextPos, d, h.color));
                        }
                    }
                }
                else if (type == FILTER)
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, ReceiverColor));
                }
                else if (type == ACTIVATOR)
                {
                    triggered.Add(nextPos);
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
                else if (type == RECEIVER)
                {
                    if (h.color == ReceiverColor)
                        triggered.Add(nextPos);
                }
                else if (type == ENERGY_NODE)
                {
                    triggered.Add(nextPos);
                }
                else
                {
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
            }
            heads = nextHeads;
        }

        foreach (var req in tempEndpoints)
        {
            if (!triggered.Contains(new Vector2Int(req.x, req.y))) return false;
        }
        return true;
    }

    private void Scramble() { }

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
                    int prOrient = orient[nextPos.y, nextPos.x] % 4;
                    Vector2Int backDir = GetPrismBackDir(prOrient);
                    foreach (Vector2Int d in DIRS)
                    {
                        if (d != backDir)
                        {
                            nextHeads.Add(new BeamHead(nextPos, d, h.color));
                        }
                    }
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
                    triggered.Add(tile);
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
        if (animateRetraceCoroutine != null) StopCoroutine(animateRetraceCoroutine);
        animateRetraceCoroutine = StartCoroutine(AnimateRetraceCoroutine());
    }

    private IEnumerator AnimateRetraceCoroutine()
    {
        if (beamContainer != null)
        {
            foreach (Transform child in beamContainer) Destroy(child.gameObject);
        }

        HashSet<GridTile> activeMirrors = new HashSet<GridTile>();

        foreach (var tile in cellTiles)
        {
            if (tile == null) continue;
            switch (tile.type)
            {
                case RECEIVER: ShowStatic(tile, receiverFrames, ReceiverColor, receiverSprite); break;
                case ACTIVATOR: ShowStatic(tile, activatorFrames, ActivatorOff, activatorSprite); break;
                case PRISM: ShowStatic(tile, prismFrames, PrismColor, cleanPrismSprite ?? prismSprite); break;
                case FILTER: ShowStatic(tile, prismFrames, new Color(1f, 0.85f, 0f, 1f), prismSprite); break;
                case ENERGY_NODE: ShowStatic(tile, activatorFrames, new Color(1f, 0.2f, 0.6f, 1f), activatorSprite); break;
                case MIRROR: 
                    // Check if it's a linked pair color
                    Color baseColor = MirrorColor;
                    foreach(var grp in groups) if (grp.Count > 1 && grp.Contains(tile)) baseColor = tile.Image.color;
                    tile.SetColor(baseColor); 
                    break;
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
                Vector2Int currentCoord = h.pos;
                Vector2Int nextCoord = currentCoord + h.dir;

                if (nextCoord.x < 0 || nextCoord.y < 0 || nextCoord.x >= boardSize || nextCoord.y >= boardSize) continue;
                if (visited.Contains((nextCoord, h.dir, h.color))) continue;
                visited.Add((nextCoord, h.dir, h.color));

                Vector2 pStart = new Vector2(currentCoord.x * tileSpacing - center, -(currentCoord.y * tileSpacing) + center);
                Vector2 pEnd = new Vector2(nextCoord.x * tileSpacing - center, -(nextCoord.y * tileSpacing) + center);
                DrawBeamSegment(pStart, pEnd, h.color);

                GridTile tile = cellTiles[nextCoord.y, nextCoord.x];
                if (tile.type == MIRROR)
                {
                    activeMirrors.Add(tile);
                    tile.SetColor(MirrorActiveColor);
                    nextHeads.Add(new BeamHead(nextCoord, Reflect(h.dir, orient[nextCoord.y, nextCoord.x]), h.color));
                }
                else if (tile.type == PRISM)
                {
                    PlayLoop(tile, prismFrames, 12f);
                    int prOrient = orient[nextCoord.y, nextCoord.x] % 4;
                    Vector2Int backDir = GetPrismBackDir(prOrient);
                    foreach (Vector2Int d in DIRS)
                    {
                        if (d != backDir) nextHeads.Add(new BeamHead(nextCoord, d, h.color));
                    }
                }
                else if (tile.type == FILTER)
                {
                    PlayLoop(tile, prismFrames, 12f, new Color(1f, 0.85f, 0f, 1f));
                    nextHeads.Add(new BeamHead(nextCoord, h.dir, ReceiverColor));
                }
                else if (tile.type == ACTIVATOR)
                {
                    triggered.Add(tile);
                    PlayLoop(tile, activatorFrames, 12f, ActivatorOn);
                    nextHeads.Add(new BeamHead(nextCoord, h.dir, h.color));
                }
                else if (tile.type == RECEIVER)
                {
                    if (h.color == ReceiverColor)
                    {
                        triggered.Add(tile);
                        PlayLoop(tile, receiverFrames, 12f, Color.white);
                    }
                }
                else if (tile.type == ENERGY_NODE)
                {
                    triggered.Add(tile);
                    PlayLoop(tile, activatorFrames, 12f, new Color(1f, 0.5f, 0.8f, 1f));
                    Vector2Int pos = new Vector2Int(tile.x, tile.y);
                    if (!claimedEnergyNodes.Contains(pos))
                    {
                        claimedEnergyNodes.Add(pos);
                        timeRemaining = Mathf.Min(puzzleTime, timeRemaining + 5f);
                        StartCoroutine(PulseMarker(tile.transform as RectTransform));
                    }
                }
                else
                {
                    nextHeads.Add(new BeamHead(nextCoord, h.dir, h.color));
                }
            }

            heads = nextHeads;
            yield return new WaitForSeconds(beamStepDelay);
        }

        bool allRequiredTriggered = true;
        foreach (var req in requiredEndpoints)
        {
            if (!triggered.Contains(req)) { allRequiredTriggered = false; break; }
        }

        if (allRequiredTriggered)
        {
            foreach (Transform child in beamContainer)
            {
                Image img = child.GetComponent<Image>();
                if (img != null) img.color = BeamSolved;
            }
            if (!isSolved) Solve();
        }

        animateRetraceCoroutine = null;
    }

    private void DrawBeamSegment(Vector2 start, Vector2 end, Color color)
    {
        GameObject go = new GameObject("BeamSegment", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(beamContainer, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = go.GetComponent<Image>();

        Vector2 diff = end - start;
        float dist = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(dist + 2f, beamThickness); // slight overlap to prevent gaps
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = start;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        img.sprite = beamSprite; 
        img.color = color;
        img.raycastTarget = false;
    }

    private void HandleClick(GridTile t)
    {
        if (!CanAcceptInput()) return;

        List<GridTile> grp = null;
        foreach (var g in groups) if (g.Contains(t)) { grp = g; break; }
        if (grp == null) grp = new List<GridTile> { t };

        foreach (var tile in grp)
        {
            if (tile.type == MIRROR)
            {
                orient[tile.y, tile.x] ^= 1;
                if (!mirrorAngle.TryGetValue(tile, out float cur)) cur = 0f;
                float target = cur + 90f;
                mirrorAngle[tile] = target;

                if (rotateTweens.TryGetValue(tile, out Coroutine running) && running != null) StopCoroutine(running);
                rotateTweens[tile] = StartCoroutine(RotateTween(tile, cur, target, 0.18f));
            }
            else if (tile.type == PRISM)
            {
                orient[tile.y, tile.x] = (orient[tile.y, tile.x] + 1) % 4;
                if (!mirrorAngle.TryGetValue(tile, out float cur)) cur = 0f;
                float target = cur + 90f;
                mirrorAngle[tile] = target;

                if (rotateTweens.TryGetValue(tile, out Coroutine running) && running != null) StopCoroutine(running);
                rotateTweens[tile] = StartCoroutine(RotateTween(tile, cur, target, 0.18f));
            }
        }

        Retrace();
    }

    private IEnumerator RotateTween(GridTile t, float from, float to, float duration)
    {
        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float t_ratio = Mathf.Clamp01(elapsed / duration);
            float z = Mathf.Lerp(from, to, t_ratio);
            t.SetRotation(z);
            
            // Squash and stretch
            float s = 1f + Mathf.Sin(t_ratio * Mathf.PI) * 0.15f;
            t.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * (2f - s), baseScale.z);

            yield return null;
        }
        if (t != null) { t.SetRotation(to); t.transform.localScale = baseScale; }
        rotateTweens[t] = null;
    }

    private IEnumerator PulseMarker(RectTransform marker)
    {
        if (marker == null) yield break;
        Vector3 originalScale = marker.localScale;
        Vector3 peakScale = originalScale * 1.4f;
        float duration = 0.4f;
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
        int gCount = groups.Count;
        if (gCount == 0) { Retrace(); return; }

        int[] sol = FindSolution();
        if (sol == null) return;

        for (int i = 0; i < gCount; i++)
        {
            var grp = groups[i];
            int curState = orient[grp[0].y, grp[0].x];
            if (curState != sol[i])
            {
                foreach (var tile in grp)
                {
                    orient[tile.y, tile.x] = sol[i];
                    float ang;
                    if (tile.type == MIRROR) ang = (sol[i] == BACKSLASH) ? 90f : 0f;
                    else ang = sol[i] * 90f;
                    mirrorAngle[tile] = ang;
                    tile.SetRotation(ang);
                }
                Retrace();
                return;
            }
        }
    }

    private int[] FindSolution()
    {
        int gCount = groups.Count;
        int[] saved = new int[gCount];
        for (int i = 0; i < gCount; i++) saved[i] = orient[groups[i][0].y, groups[i][0].x];

        int[] result = null;
        List<int[]> solutions = new List<int[]>();
        int[] currentStates = new int[gCount];
        int solutionCount = 0;

        SearchCombinationsReal(0, currentStates, ref solutionCount, solutions);

        if (solutions.Count > 0) result = solutions[0];
        for (int i = 0; i < gCount; i++) foreach (var tile in groups[i]) orient[tile.y, tile.x] = saved[i];
        return result;
    }

    private void SearchCombinationsReal(int groupIdx, int[] currentStates, ref int solutionCount, List<int[]> solutions)
    {
        if (groupIdx >= groups.Count)
        {
            HashSet<GridTile> triggered = new HashSet<GridTile>();
            if (TraceBeamStatic(triggered))
            {
                solutionCount++;
                solutions.Add((int[])currentStates.Clone());
            }
            return;
        }

        int maxStates = (groups[groupIdx][0].type == MIRROR) ? 2 : 4;
        for (int s = 0; s < maxStates; s++)
        {
            currentStates[groupIdx] = s;
            foreach (var tile in groups[groupIdx]) orient[tile.y, tile.x] = s;
            SearchCombinationsReal(groupIdx + 1, currentStates, ref solutionCount, solutions);
            if (solutionCount > 0) return;
        }
    }
}
