using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles beam tracing and animation for Mirror Maze.
/// Pure beam logic separated from board generation.
/// </summary>
public class MirrorBeamTracer : MonoBehaviour
{
    [Header("Beam Settings")]
    public float beamStepDelay = 0.01f;
    public float beamThickness = 12f;

    [Header("Colors")]
    public Color emitterColor = new Color(0f, 0.95f, 1f, 1f);
    public Color receiverColor = new Color(1f, 0.75f, 0.20f, 1f);
    public Color beamColor = new Color(0f, 0.95f, 1f, 0.9f);
    public Color beamSolvedColor = new Color(0.2f, 1f, 0.4f, 1f);
    public Color mirrorActiveColor = new Color(0f, 1f, 1f, 1f);

    [Header("References")]
    public Sprite beamSprite;
    public RectTransform beamContainer;

    // Board reference (set by MirrorMazePuzzle)
    private MirrorMazePuzzle.BoardData _board;
    private Coroutine _animateCoroutine;

    // State
    private HashSet<GridTile> _triggered = new HashSet<GridTile>();

    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
    };

    public void Initialize(MirrorMazePuzzle.BoardData board)
    {
        _board = board;
        if (beamContainer == null)
        {
            var go = new GameObject("BeamLayer", typeof(RectTransform));
            beamContainer = (RectTransform)go.transform;
            beamContainer.SetParent(_board.gridContainer, false);
            beamContainer.anchoredPosition = Vector2.zero;
            beamContainer.localScale = Vector3.one;
            beamContainer.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Traces the beam once (static check) and returns all triggered endpoints.
    /// </summary>
    public HashSet<GridTile> TraceStatic()
    {
        _triggered.Clear();
        var heads = new List<BeamHead> { new BeamHead(_board.emitterPos, _board.emitDir, emitterColor) };
        var visited = new HashSet<(Vector2Int, Vector2Int, Color)>();

        int limit = _board.boardSize * _board.boardSize * 4;
        while (heads.Count > 0 && limit-- > 0)
        {
            var nextHeads = new List<BeamHead>();
            foreach (var h in heads)
            {
                var nextPos = h.pos + h.dir;
                if (!InBounds(nextPos)) continue;
                if (visited.Contains((nextPos, h.dir, h.color))) continue;
                visited.Add((nextPos, h.dir, h.color));

                var tile = _board.cellTiles[nextPos.y, nextPos.x];
                if (tile == null) continue;

                switch (tile.type)
                {
                    case MirrorMazeGenerator.MIRROR:
                        nextHeads.Add(new BeamHead(nextPos, Reflect(h.dir, _board.orient[nextPos.y, nextPos.x]), h.color));
                        break;
                    case MirrorMazeGenerator.PRISM:
                        int prOrient = _board.orient[nextPos.y, nextPos.x] % 4;
                        var backDir = GetPrismBackDir(prOrient);
                        foreach (var d in DIRS)
                            if (d != backDir) nextHeads.Add(new BeamHead(nextPos, d, h.color));
                        break;
                    case MirrorMazeGenerator.FILTER:
                        nextHeads.Add(new BeamHead(nextPos, h.dir, receiverColor));
                        break;
                    case MirrorMazeGenerator.ACTIVATOR:
                    case MirrorMazeGenerator.ENERGY_NODE:
                        _triggered.Add(tile);
                        nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                        break;
                    case MirrorMazeGenerator.RECEIVER:
                        if (h.color == receiverColor) _triggered.Add(tile);
                        break;
                    default:
                        nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                        break;
                }
            }
            heads = nextHeads;
        }
        return _triggered;
    }

    /// <summary>
    /// Animates the beam trace step by step with visual feedback.
    /// </summary>
    public IEnumerator AnimateTrace(System.Action onComplete = null)
    {
        if (_animateCoroutine != null) StopCoroutine(_animateCoroutine);
        _animateCoroutine = StartCoroutine(AnimateRoutine(onComplete));
        yield return _animateCoroutine;
    }

    private IEnumerator AnimateRoutine(System.Action onComplete)
    {
        // Clear previous beam visuals
        foreach (Transform child in beamContainer) Destroy(child.gameObject);

        // Reset tile visuals
        for (int y = 0; y < _board.boardSize; y++)
        {
            for (int x = 0; x < _board.boardSize; x++)
            {
                var tile = _board.cellTiles[y, x];
                if (tile == null) continue;
                switch (tile.type)
                {
                    case MirrorMazeGenerator.RECEIVER: tile.SetColor(Color.white); break;
                    case MirrorMazeGenerator.ACTIVATOR: tile.SetColor(new Color(0.4f, 0.4f, 0.4f, 1f)); break;
                    case MirrorMazeGenerator.PRISM: tile.SetColor(new Color(0.8f, 0.8f, 1f, 1f)); break;
                    case MirrorMazeGenerator.FILTER: tile.SetColor(new Color(1f, 0.85f, 0f, 1f)); break;
                    case MirrorMazeGenerator.ENERGY_NODE: tile.SetColor(new Color(1f, 0.2f, 0.6f, 1f)); break;
                    case MirrorMazeGenerator.MIRROR:
                        tile.SetColor(new Color(0.90f, 0.92f, 1f, 1f));
                        break;
                }
            }
        }

        var heads = new List<BeamHead> { new BeamHead(_board.emitterPos, _board.emitDir, emitterColor) };
        var triggered = new HashSet<GridTile>();
        var visited = new HashSet<(Vector2Int, Vector2Int, Color)>();

        float center = (_board.boardSize - 1) * _board.tileSpacing / 2f;

        int limit = _board.boardSize * _board.boardSize * 4;

        while (heads.Count > 0 && limit-- > 0)
        {
            var nextHeads = new List<BeamHead>();

            foreach (var h in heads)
            {
                var currentCoord = h.pos;
                var nextCoord = currentCoord + h.dir;

                if (!InBounds(nextCoord)) continue;
                if (visited.Contains((nextCoord, h.dir, h.color))) continue;
                visited.Add((nextCoord, h.dir, h.color));

                // Draw beam segment
                Vector2 pStart = new Vector2(currentCoord.x * _board.tileSpacing - center, -(currentCoord.y * _board.tileSpacing) + center);
                Vector2 pEnd = new Vector2(nextCoord.x * _board.tileSpacing - center, -(nextCoord.y * _board.tileSpacing) + center);
                DrawBeamSegment(pStart, pEnd, h.color);

                var tile = _board.cellTiles[nextCoord.y, nextCoord.x];
                if (tile == null) continue;

                switch (tile.type)
                {
                    case MirrorMazeGenerator.MIRROR:
                        tile.SetColor(mirrorActiveColor);
                        nextHeads.Add(new BeamHead(nextCoord, Reflect(h.dir, _board.orient[nextCoord.y, nextCoord.x]), h.color));
                        break;
                    case MirrorMazeGenerator.PRISM:
                        nextHeads.Add(new BeamHead(nextCoord, h.dir, h.color)); // Simplified prism reflection
                        foreach (var d in DIRS)
                            if (d != GetPrismBackDir(_board.orient[nextCoord.y, nextCoord.x] % 4)) nextHeads.Add(new BeamHead(nextCoord, d, h.color));
                        break;
                    case MirrorMazeGenerator.FILTER:
                        nextHeads.Add(new BeamHead(nextCoord, h.dir, receiverColor));
                        break;
                    case MirrorMazeGenerator.ACTIVATOR:
                        triggered.Add(tile);
                        nextHeads.Add(new BeamHead(nextCoord, h.dir, h.color));
                        break;
                    case MirrorMazeGenerator.RECEIVER:
                        if (h.color == receiverColor) triggered.Add(tile);
                        break;
                    case MirrorMazeGenerator.ENERGY_NODE:
                        triggered.Add(tile);
                        break;
                    default:
                        nextHeads.Add(new BeamHead(nextCoord, h.dir, h.color));
                        break;
                }
            }

            heads = nextHeads;
            yield return new WaitForSeconds(beamStepDelay);
        }

        // Check win condition
        bool allTriggered = true;
        foreach (var req in _board.requiredEndpoints)
            if (!triggered.Contains(req)) { allTriggered = false; break; }

        if (allTriggered)
        {
            // Turn beam green
            foreach (Transform child in beamContainer)
            {
                var img = child.GetComponent<Image>();
                if (img != null) img.color = beamSolvedColor;
            }
        }

        onComplete?.Invoke();
        _animateCoroutine = null;
    }

    private void DrawBeamSegment(Vector2 start, Vector2 end, Color color)
    {
        var go = new GameObject("BeamSegment", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(beamContainer, false);
        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();

        Vector2 diff = end - start;
        float dist = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(dist + 2f, beamThickness);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = start;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        img.sprite = beamSprite;
        img.color = color;
        img.raycastTarget = false;
    }

    // --- Helpers ---

    private bool InBounds(Vector2Int pos)
        => pos.x >= 0 && pos.x < _board.boardSize && pos.y >= 0 && pos.y < _board.boardSize;

    private Vector2Int Reflect(Vector2Int d, int o)
        => o == MirrorMazeGenerator.BACKSLASH ? new Vector2Int(d.y, d.x) : new Vector2Int(-d.y, -d.x);

    private Vector2Int GetPrismBackDir(int orient)
    {
        switch (orient % 4)
        {
            case 0: return new Vector2Int(0, 1);
            case 1: return new Vector2Int(-1, 0);
            case 2: return new Vector2Int(0, -1);
            default: return new Vector2Int(1, 0);
        }
    }

    private struct BeamHead
    {
        public Vector2Int pos, dir;
        public Color color;
        public BeamHead(Vector2Int p, Vector2Int d, Color c) { pos = p; dir = d; color = c; }
    }
}