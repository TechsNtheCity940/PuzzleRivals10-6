using UnityEngine;
using System.Collections.Generic;

// Mirror Maze ("MirrorBeam") - route a light beam from the EMITTER to the
// RECEIVER by rotating diagonal mirrors. Clicking a mirror flips it between the
// two diagonal orientations ('/' and '\'). The beam is traced from the emitter,
// reflecting off mirrors, and the puzzle is solved when it reaches the receiver.
//
// World-space procedural grid (sprites + BoxCollider2D + OnMouseDown), matching
// the project's puzzle conventions (see MazeRush / ComboRush). Generation lays
// out a guaranteed staircase solution, then scrambles the mirrors so the player
// (or the AI bot via BotSolve) must rotate them back.
public class MirrorBeam : PuzzleBase
{
    [Header("Board")]
    [Tooltip("Idle board background tile variants (sliced board_tile sheet).")]
    public Sprite[] boardTiles;

    [Header("Mirror")]
    [Tooltip("Angled mirror idle sprite cells (sliced angled_mirror sheet). Uses the first cell.")]
    public Sprite[] mirrorSprites;

    [Header("Emitter (animated, loop)")]
    public Sprite[] emitterFrames;
    public float emitterFps = 10f;

    [Header("Receiver (animated, loop)")]
    public Sprite[] receiverFrames;
    public float receiverFps = 10f;

    [Header("Beam (animated, loop) - optional sprite path")]
    public Sprite[] beamFrames;
    public float beamFps = 14f;

    private const float SPACING = 0.9f;
    private const float CELL_SIZE = 0.86f;
    private const int SLASH = 0;      // '/'
    private const int BACKSLASH = 1;  // '\'

    private static readonly Color BeamOn = new Color(0.0f, 0.95f, 1.0f, 1.0f);
    private static readonly Color BeamSolved = new Color(0.61f, 1.0f, 0.18f, 1.0f);

    private int gridSize = 5;
    private MirrorCell[,] cellNodes; // [y, x]
    private readonly List<MirrorCell> mirrorCells = new List<MirrorCell>();

    private Vector2Int emitterPos;
    private Vector2Int emitDir;
    private Vector2Int receiverPos;

    private LineRenderer beamLine;
    private Transform beamSpriteParent;
    private readonly List<Vector2Int> beamPath = new List<Vector2Int>();
    private readonly List<Vector2Int> scratchPath = new List<Vector2Int>();

    public override void Setup(int d)
    {
        base.Setup(d);
        gridSize = Mathf.Clamp(5 + (d - 1), 5, 7);
        Build();
    }

    // ----------------------------------------------------------------- build
    private void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        cellNodes = new MirrorCell[gridSize, gridSize];
        mirrorCells.Clear();

        // --- choose a guaranteed staircase path: emitter on the left edge
        // heading right, one vertical bend, then right into the receiver.
        int ery = Random.Range(0, gridSize);
        emitterPos = new Vector2Int(0, ery);
        emitDir = new Vector2Int(1, 0);

        int ty = Random.Range(0, gridSize);
        while (ty == ery) ty = Random.Range(0, gridSize);
        receiverPos = new Vector2Int(gridSize - 1, ty);

        int mx = Random.Range(1, gridSize - 1); // turn column, strictly inside
        int vSign = ty > ery ? 1 : -1;          // grid y is downward
        int turnOrient = vSign == 1 ? BACKSLASH : SLASH; // both bends share this

        // --- collect the solution path cells (to keep decoys clear of them).
        HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();
        for (int x = 0; x <= mx; x++) pathSet.Add(new Vector2Int(x, ery));      // segment A
        int yStep = vSign;
        for (int y = ery; y != ty + yStep; y += yStep) pathSet.Add(new Vector2Int(mx, y)); // B
        for (int x = mx; x < gridSize; x++) pathSet.Add(new Vector2Int(x, ty)); // segment C

        // --- record where the two solution mirrors live.
        Dictionary<Vector2Int, int> mirrorPlan = new Dictionary<Vector2Int, int>
        {
            { new Vector2Int(mx, ery), turnOrient },
            { new Vector2Int(mx, ty),  turnOrient }
        };

        // --- decoy mirrors on free cells (off the solution path) for variety.
        int decoys = Mathf.Clamp(difficulty, 1, 3);
        int guard = 0;
        while (decoys > 0 && guard < 200)
        {
            guard++;
            Vector2Int p = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            if (p == emitterPos || p == receiverPos) continue;
            if (pathSet.Contains(p)) continue;
            if (mirrorPlan.ContainsKey(p)) continue;
            mirrorPlan[p] = Random.Range(0, 2);
            decoys--;
        }

        // --- build the grid.
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                cellNodes[y, x] = MakeCell(x, y, mirrorPlan);

        SetupBeamRenderer();

        // --- scramble mirror orientations so the puzzle starts unsolved.
        Scramble();
        Retrace();
    }

    private void Scramble()
    {
        int safety = 0;
        do
        {
            foreach (var m in mirrorCells)
            {
                m.orientation = Random.Range(0, 2);
                UpdateMirrorVisual(m);
            }
            safety++;
        }
        while (mirrorCells.Count > 0 && TraceBeam(scratchPath) && safety < 50);
    }

    private MirrorCell MakeCell(int x, int y, Dictionary<Vector2Int, int> mirrorPlan)
    {
        Vector2Int gp = new Vector2Int(x, y);

        GameObject go = new GameObject("Cell_" + x + "_" + y);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = CellWorld(x, y);

        GameObject baseGo = new GameObject("Base");
        baseGo.transform.SetParent(go.transform, false);
        SpriteRenderer baseR = baseGo.AddComponent<SpriteRenderer>();
        baseR.sortingOrder = 1;
        if (boardTiles != null && boardTiles.Length > 0)
        {
            Sprite tile = boardTiles[Random.Range(0, boardTiles.Length)];
            ApplySprite(baseGo.transform, baseR, tile, SPACING);
        }
        baseR.color = new Color(0.85f, 0.9f, 1.0f, 1.0f);

        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        SpriteRenderer iconR = iconGo.AddComponent<SpriteRenderer>();
        iconR.sortingOrder = 5;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(SPACING, SPACING);

        MirrorCell mc = go.AddComponent<MirrorCell>();
        mc.baseRenderer = baseR;
        mc.iconRenderer = iconR;
        mc.Init(x, y);
        mc.OnClicked += HandleClick;

        if (gp == emitterPos)
        {
            Sprite first = (emitterFrames != null && emitterFrames.Length > 0) ? emitterFrames[0] : null;
            ApplySprite(iconGo.transform, iconR, first, CELL_SIZE * 0.92f);
            iconR.color = Color.white;
            AddLoopAnimator(iconGo, emitterFrames, emitterFps);
        }
        else if (gp == receiverPos)
        {
            Sprite first = (receiverFrames != null && receiverFrames.Length > 0) ? receiverFrames[0] : null;
            ApplySprite(iconGo.transform, iconR, first, CELL_SIZE * 0.92f);
            iconR.color = Color.white;
            AddLoopAnimator(iconGo, receiverFrames, receiverFps);
        }
        else if (mirrorPlan.TryGetValue(gp, out int orient))
        {
            mc.isMirror = true;
            mc.orientation = orient;
            Sprite ms = (mirrorSprites != null && mirrorSprites.Length > 0) ? mirrorSprites[0] : null;
            ApplySprite(iconGo.transform, iconR, ms, CELL_SIZE * 0.82f);
            iconR.color = Color.white;
            mirrorCells.Add(mc);
            UpdateMirrorVisual(mc);
        }

        return mc;
    }

    private void AddLoopAnimator(GameObject host, Sprite[] frames, float fps)
    {
        if (frames == null || frames.Length == 0) return;
        SpriteSheetAnimator anim = host.AddComponent<SpriteSheetAnimator>();
        anim.playOnAwake = true;
        anim.destroyOnComplete = false;
        anim.SetFrames(frames, fps, true);
    }

    private void SetupBeamRenderer()
    {
        GameObject beamGo = new GameObject("BeamLayer");
        beamGo.transform.SetParent(transform, false);

        beamLine = beamGo.AddComponent<LineRenderer>();
        beamLine.useWorldSpace = false;
        beamLine.alignment = LineAlignment.View;
        beamLine.numCapVertices = 4;
        beamLine.numCornerVertices = 4;
        beamLine.widthMultiplier = 0.12f;
        beamLine.textureMode = LineTextureMode.Stretch;
        beamLine.material = new Material(Shader.Find("Sprites/Default"));
        beamLine.sortingOrder = 8;
        beamLine.positionCount = 0;

        GameObject spritesGo = new GameObject("BeamSprites");
        spritesGo.transform.SetParent(beamGo.transform, false);
        beamSpriteParent = spritesGo.transform;
    }

    // -------------------------------------------------------------- geometry
    private Vector3 CellWorld(int x, int y)
    {
        float startX = -(gridSize - 1) * SPACING / 2f;
        float startY = (gridSize - 1) * SPACING / 2f;
        return new Vector3(startX + x * SPACING, startY - y * SPACING, 0f);
    }

    private void ApplySprite(Transform t, SpriteRenderer sr, Sprite sp, float target)
    {
        sr.sprite = sp;
        if (sp == null) return;
        float s = Mathf.Max(sp.bounds.size.x, sp.bounds.size.y);
        if (s > 0.0001f)
            t.localScale = Vector3.one * (target / s);
    }

    private void UpdateMirrorVisual(MirrorCell mc)
    {
        if (mc == null || mc.iconRenderer == null) return;
        mc.iconRenderer.transform.localRotation =
            Quaternion.Euler(0f, 0f, mc.orientation == BACKSLASH ? 90f : 0f);
    }

    // ------------------------------------------------------------ beam logic
    private Vector2Int Reflect(Vector2Int d, int orient)
    {
        // '\' : (dx,dy) -> (dy,dx);  '/' : (dx,dy) -> (-dy,-dx)
        return orient == BACKSLASH
            ? new Vector2Int(d.y, d.x)
            : new Vector2Int(-d.y, -d.x);
    }

    private bool TraceBeam(List<Vector2Int> path)
    {
        path.Clear();
        Vector2Int pos = emitterPos;
        Vector2Int dir = emitDir;
        path.Add(pos);

        int max = gridSize * gridSize * 4 + 16;
        for (int i = 0; i < max; i++)
        {
            Vector2Int next = pos + dir;
            if (next.x < 0 || next.y < 0 || next.x >= gridSize || next.y >= gridSize)
                return false;

            path.Add(next);
            if (next == receiverPos)
                return true;

            MirrorCell mc = cellNodes[next.y, next.x];
            if (mc != null && mc.isMirror)
                dir = Reflect(dir, mc.orientation);

            pos = next;
        }
        return false;
    }

    private void Retrace()
    {
        bool reached = TraceBeam(beamPath);
        DrawBeam(beamPath, reached);

        if (reached && !isSolved)
        {
            PlaySolve();
            Solve(90 + difficulty * 35);
        }
    }

    private void DrawBeam(List<Vector2Int> path, bool reached)
    {
        if (beamLine == null) return;

        Color c = reached ? BeamSolved : BeamOn;
        beamLine.startColor = c;
        beamLine.endColor = c;
        beamLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 p = CellWorld(path[i].x, path[i].y);
            p.z = -0.2f;
            beamLine.SetPosition(i, p);
        }

        // Optional traveling-beam sprite tiles on pass-through cells.
        if (beamSpriteParent != null)
        {
            for (int i = beamSpriteParent.childCount - 1; i >= 0; i--)
                Destroy(beamSpriteParent.GetChild(i).gameObject);

            if (beamFrames != null && beamFrames.Length > 0)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    Vector2Int gp = path[i];
                    if (gp == emitterPos || gp == receiverPos) continue;
                    MirrorCell mc = cellNodes[gp.y, gp.x];
                    if (mc != null && mc.isMirror) continue;

                    Vector2Int dirCell = i + 1 < path.Count ? path[i + 1] - gp
                                       : (i > 0 ? gp - path[i - 1] : emitDir);
                    SpawnBeamSprite(gp, dirCell, c);
                }
            }
        }
    }

    private void SpawnBeamSprite(Vector2Int gp, Vector2Int dirCell, Color c)
    {
        GameObject go = new GameObject("Beam_" + gp.x + "_" + gp.y);
        go.transform.SetParent(beamSpriteParent, false);
        Vector3 lp = CellWorld(gp.x, gp.y);
        lp.z = -0.15f;
        go.transform.localPosition = lp;

        Vector3 a = CellWorld(gp.x, gp.y);
        Vector3 b = CellWorld(gp.x + dirCell.x, gp.y + dirCell.y);
        float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, ang);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 7;
        sr.color = c;
        ApplySprite(go.transform, sr, beamFrames[0], SPACING);

        SpriteSheetAnimator anim = go.AddComponent<SpriteSheetAnimator>();
        anim.playOnAwake = true;
        anim.destroyOnComplete = false;
        anim.SetFrames(beamFrames, beamFps, true);
    }

    // -------------------------------------------------------------- input
    private void HandleClick(int x, int y)
    {
        if (!inputEnabled || isSolved) return;
        MirrorCell mc = cellNodes[y, x];
        if (mc == null || !mc.isMirror) return;

        mc.orientation ^= 1;
        UpdateMirrorVisual(mc);
        PlayRotate();
        Retrace();
    }

    // AIBot calls this once via SendMessage. Brute-force the mirror orientations
    // until the beam connects (a solution is guaranteed to exist).
    public void BotSolve()
    {
        if (isSolved) return;

        int m = mirrorCells.Count;
        if (m == 0)
        {
            Retrace();
            return;
        }

        if (m <= 16)
        {
            int combos = 1 << m;
            for (int c = 0; c < combos; c++)
            {
                for (int b = 0; b < m; b++)
                    mirrorCells[b].orientation = (c >> b) & 1;

                if (TraceBeam(scratchPath))
                {
                    for (int b = 0; b < m; b++)
                        UpdateMirrorVisual(mirrorCells[b]);
                    Retrace();
                    return;
                }
            }
        }
        else
        {
            int safety = 600;
            while (!isSolved && safety-- > 0)
            {
                mirrorCells[Random.Range(0, m)].orientation ^= 1;
                UpdateMirrorVisual(mirrorCells[Random.Range(0, m)]);
                Retrace();
            }
        }

        for (int b = 0; b < m; b++)
            UpdateMirrorVisual(mirrorCells[b]);
        Retrace();
    }

    // -------------------------------------------------------------- audio
    private void PlayRotate()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.55f);
    }

    private void PlaySolve()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.8f);
    }
}
