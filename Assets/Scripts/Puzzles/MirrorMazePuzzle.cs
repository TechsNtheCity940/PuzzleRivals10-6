using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mirror Maze Puzzle Controller — thin orchestrator using Generator + Tracer.
/// Board generation: MirrorMazeGenerator.Generate()
/// Beam logic: MirrorBeamTracer (static trace + animated trace)
/// Group management: MirrorGroupManager (handles mirror/prism groups)
/// This class handles: UI setup, input, state machine, bot integration.
/// </summary>
public class MirrorMazePuzzle : BasePuzzle
{
    [Header("Mirror Maze")]
    public int gridSize = 5;
    public GameObject mirrorPrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 60f;

    [Header("Art (assigned at runtime from BoardData)")]
    public Sprite[] floorFrames;
    public Sprite[] mirrorFrames;
    public Sprite[] emitterFrames;
    public Sprite[] receiverFrames;
    public Sprite[] prismFrames;
    public Sprite[] activatorFrames;
    public Sprite[] beamFrames;
    public Sprite beamSprite;

    // Runtime board data (populated by Generator)
    private MirrorMazeGenerator.BoardPlan _plan;
    private MirrorBeamTracer _beamTracer;
    private MirrorGroupManager _groupManager;

    // UI tiles
    private GridTile[,] _cellTiles;

    // State
    private readonly Dictionary<GridTile, Coroutine> _rotateTweens = new Dictionary<GridTile, Coroutine>();
    private readonly Dictionary<GridTile, float> _mirrorAngle = new Dictionary<GridTile, float>();
    private Coroutine _animateCoroutine;

    // BoardData struct for passing to Tracer
    public struct BoardData
    {
        public int boardSize;
        public float tileSpacing;
        public Vector2Int emitterPos;
        public Vector2Int emitDir;
        public Vector2Int receiverPos;
        public GridTile[,] cellTiles;
        public int[,] orient;
        public List<GridTile> requiredEndpoints;
        public RectTransform gridContainer;
        public Sprite[] prismFrames;
        public Sprite[] activatorFrames;
        public Sprite[] receiverFrames;
        public Sprite beamSprite;
    }

    private BoardData _boardData;

    protected override void BuildBoard()
    {
        ClearContainer();

        // 1. Generate board plan (with guaranteed solvable + scrambled state)
        _plan = MirrorMazeGenerator.Generate(gridSize, difficulty, rng);

        // 2. Instantiate UI tiles from plan
        _cellTiles = new GridTile[_plan.boardSize, _plan.boardSize];
        _boardData = new BoardData
        {
            boardSize = _plan.boardSize,
            tileSpacing = tileSpacing,
            emitterPos = _plan.emitterPos,
            emitDir = _plan.emitDir,
            receiverPos = _plan.receiverPos,
            cellTiles = _cellTiles,
            orient = new int[_plan.boardSize, _plan.boardSize],
            requiredEndpoints = new List<GridTile>(),
            gridContainer = gridContainer,
            prismFrames = prismFrames,
            activatorFrames = activatorFrames,
            receiverFrames = receiverFrames,
            beamSprite = beamSprite
        };

        float center = (_plan.boardSize - 1) * tileSpacing / 2f;

        for (int y = 0; y < _plan.boardSize; y++)
        {
            for (int x = 0; x < _plan.boardSize; x++)
            {
                GameObject go = Instantiate(mirrorPrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(x * tileSpacing - center, -(y * tileSpacing) + center);
                rt.sizeDelta = new Vector2(tileSpacing, tileSpacing);

                GridTile tile = go.GetComponent<GridTile>();
                tile.Init(x, y, -1);
                _cellTiles[y, x] = tile;

                // Determine type from plan
                int type = MirrorMazeGenerator.FLOOR;
                if (_plan.plan.TryGetValue(new Vector2Int(x, y), out int planType))
                    type = planType;

                if (new Vector2Int(x, y) == _plan.emitterPos)
                    type = MirrorMazeGenerator.EMITTER;
                else if (new Vector2Int(x, y) == _plan.receiverPos)
                    type = MirrorMazeGenerator.RECEIVER;

                tile.type = type;
                SetupTileVisuals(tile, type);

                // Apply initial rotation from scrambled states
                float angle = 0f;
                if (type == MirrorMazeGenerator.MIRROR || type == MirrorMazeGenerator.PRISM)
                {
                    // Find which group this tile belongs to
                    int groupIdx = FindGroupIndex(x, y);
                    if (groupIdx >= 0 && groupIdx < _plan.scrambledStates.Length)
                    {
                        int state = _plan.scrambledStates[groupIdx];
                        if (type == MirrorMazeGenerator.MIRROR)
                            angle = (state == MirrorMazeGenerator.BACKSLASH) ? 90f : 0f;
                        else
                            angle = state * 90f;
                        _boardData.orient[y, x] = state;
                    }
                }

                _mirrorAngle[tile] = angle;
                tile.SetRotation(angle);

                if (type == MirrorMazeGenerator.MIRROR || type == MirrorMazeGenerator.PRISM)
                    tile.OnClicked += (UITile t) => HandleClick((GridTile)t);

                // Track endpoints for win condition
                if (type == MirrorMazeGenerator.RECEIVER || type == MirrorMazeGenerator.ACTIVATOR)
                    _boardData.requiredEndpoints.Add(tile);
            }
        }

        // 3. Initialize beam tracer
        _beamTracer = gameObject.AddComponent<MirrorBeamTracer>();
        _beamTracer.beamStepDelay = 0.01f;
        _beamTracer.beamThickness = 12f;
        _beamTracer.emitterColor = new Color(0f, 0.95f, 1f, 1f);
        _beamTracer.receiverColor = new Color(1f, 0.75f, 0.20f, 1f);
        _beamTracer.beamColor = new Color(0f, 0.95f, 1f, 0.9f);
        _beamTracer.beamSolvedColor = new Color(0.2f, 1f, 0.4f, 1f);
        _beamTracer.mirrorActiveColor = new Color(0f, 1f, 1f, 1f);
        _beamTracer.beamSprite = beamSprite;
        _beamTracer.Initialize(_boardData);

        // 4. Initialize group manager
        _groupManager = new MirrorGroupManager(_plan, _boardData, rng);

        // 5. Initial trace
        Retrace();
    }

    private void ClearContainer()
    {
        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }
        if (_beamTracer != null) Destroy(_beamTracer);
        _rotateTweens.Clear();
        _mirrorAngle.Clear();
    }

    private void SetupTileVisuals(GridTile tile, int type)
    {
        var anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
        anim.playOnEnable = false;

        switch (type)
        {
            case MirrorMazeGenerator.EMITTER:
                anim.PlaySet(emitterFrames, 10f, true);
                break;
            case MirrorMazeGenerator.RECEIVER:
                tile.SetSprite(receiverFrames != null && receiverFrames.Length > 0 ? receiverFrames[0] : null);
                tile.SetColor(Color.white);
                break;
            case MirrorMazeGenerator.MIRROR:
                tile.SetSprite(mirrorFrames != null && mirrorFrames.Length > 0 ? mirrorFrames[0] : null);
                tile.SetColor(new Color(0.90f, 0.92f, 1f, 1f));
                break;
            case MirrorMazeGenerator.PRISM:
                tile.SetSprite(prismFrames != null && prismFrames.Length > 0 ? prismFrames[0] : null);
                tile.SetColor(new Color(0.8f, 0.8f, 1f, 1f));
                break;
            case MirrorMazeGenerator.ACTIVATOR:
                tile.SetSprite(activatorFrames != null && activatorFrames.Length > 0 ? activatorFrames[0] : null);
                tile.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f));
                break;
            case MirrorMazeGenerator.FILTER:
                tile.SetSprite(prismFrames != null && prismFrames.Length > 0 ? prismFrames[0] : null);
                tile.SetColor(new Color(1f, 0.85f, 0f, 1f));
                break;
            case MirrorMazeGenerator.ENERGY_NODE:
                tile.SetSprite(activatorFrames != null && activatorFrames.Length > 0 ? activatorFrames[0] : null);
                tile.SetColor(new Color(1f, 0.2f, 0.6f, 1f));
                break;
            default: // FLOOR
                tile.SetSprite(floorFrames != null && floorFrames.Length > 0 ? floorFrames[0] : null);
                tile.SetColor(new Color(0.07f, 0.07f, 0.07f, 1f));
                break;
        }
    }

    private int FindGroupIndex(int x, int y)
    {
        for (int i = 0; i < _plan.groups.Count; i++)
        {
            foreach (var pos in _plan.groups[i])
            {
                if (pos.x == x && pos.y == y) return i;
            }
        }
        return -1;
    }

    private void HandleClick(GridTile tile)
    {
        if (!CanAcceptInput()) return;

        // Use group manager to find group index
        int groupIdx = _groupManager.GetGroupIndex(tile);
        if (groupIdx < 0) return;

        var group = _plan.groups[groupIdx];
        int type = _cellTiles[tile.y, tile.x].type;
        int maxStates = _groupManager.GetMaxStates(groupIdx);

        // Advance group state
        int newState = (_boardData.orient[group[0].y, group[0].x] + 1) % maxStates;

        // Update all tiles in the group using group manager
        _groupManager.SetGroupState(groupIdx, newState);

        // Update visuals with tweening
        foreach (var pos in group)
        {
            var t = _cellTiles[pos.y, pos.x];
            float angle = (type == MirrorMazeGenerator.MIRROR)
                ? (newState == MirrorMazeGenerator.BACKSLASH ? 90f : 0f)
                : newState * 90f;

            _mirrorAngle[t] = angle;

            if (_rotateTweens.TryGetValue(t, out var running) && running != null)
                StopCoroutine(running);
            _rotateTweens[t] = StartCoroutine(RotateTween(t, t.transform.localEulerAngles.z, angle, 0.18f));
        }

        Retrace();
    }

    private IEnumerator RotateTween(GridTile tile, float from, float to, float duration)
    {
        float elapsed = 0f;
        Vector3 baseScale = Vector3.one;
        while (elapsed < duration)
        {
            if (tile == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Lerp(from, to, t);
            tile.SetRotation(angle);

            // Squash/stretch
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
            tile.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * (2f - s), baseScale.z);
            yield return null;
        }
        if (tile != null) { tile.SetRotation(to); tile.transform.localScale = baseScale; }
        _rotateTweens[tile] = null;
    }

    private void Retrace()
    {
        if (_animateCoroutine != null) StopCoroutine(_animateCoroutine);
        _animateCoroutine = StartCoroutine(AnimateRetrace());
    }

    private IEnumerator AnimateRetrace()
    {
        yield return _beamTracer.AnimateTrace(() =>
        {
            // Check win condition after animation
            var triggered = _beamTracer.TraceStatic();
            bool allTriggered = true;
            foreach (var req in _boardData.requiredEndpoints)
                if (!triggered.Contains(req)) { allTriggered = false; break; }

            if (allTriggered && !isSolved) Solve();
        });
    }

    // --- Brain vs Brain ---

    public override void ApplyAttack(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Freeze:
                freezeTimer = Mathf.Max(freezeTimer, PuzzleConfig.Instance?.freezeDuration ?? 2.5f);
                break;
            case AttackType.Scramble:
                if (!isSolved)
                {
                    // Re-scramble: pick new random scrambled states using group manager
                    for (int i = 0; i < _plan.groups.Count; i++)
                    {
                        int type = _cellTiles[_plan.groups[i][0].y, _plan.groups[i][0].x].type;
                        int maxStates = _groupManager.GetMaxStates(i);
                        int newState = rng.Next(0, maxStates);
                        _groupManager.SetGroupState(i, newState, false);

                        foreach (var pos in _plan.groups[i])
                        {
                            _boardData.orient[pos.y, pos.x] = newState;
                            float angle = (type == MirrorMazeGenerator.MIRROR)
                                ? (newState == MirrorMazeGenerator.BACKSLASH ? 90f : 0f)
                                : newState * 90f;
                            var t = _cellTiles[pos.y, pos.x];
                            _mirrorAngle[t] = angle;
                            t.SetRotation(angle);
                        }
                    }
                    Retrace();
                }
                break;
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;

        // Step one group toward solution using group manager
        for (int i = 0; i < _plan.groups.Count; i++)
        {
            if (_groupManager.AdvanceGroupTowardTarget(i))
            {
                // Update visuals for the group that was advanced
                var group = _plan.groups[i];
                int type = _cellTiles[group[0].y, group[0].x].type;
                int newState = _groupManager.GetGroupState(i);
                
                foreach (var pos in group)
                {
                    _boardData.orient[pos.y, pos.x] = newState;
                    float angle = (type == MirrorMazeGenerator.MIRROR)
                        ? (newState == MirrorMazeGenerator.BACKSLASH ? 90f : 0f)
                        : newState * 90f;
                    var t = _cellTiles[pos.y, pos.x];
                    _mirrorAngle[t] = angle;
                    t.SetRotation(angle);
                    
                    if (_rotateTweens.TryGetValue(t, out var running) && running != null)
                        StopCoroutine(running);
                    _rotateTweens[t] = StartCoroutine(RotateTween(t, t.transform.localEulerAngles.z, angle, 0.18f));
                }
                
                Retrace();
                return;
            }
        }
    }
}