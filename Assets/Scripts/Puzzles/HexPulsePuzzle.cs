using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rotate six-sided pipe tiles to connect a perimeter source to the central battery.
/// Uses axial hex coordinates and matching six-direction ports, rather than the old
/// placeholder rule that simply required every tile to point at zero degrees.
/// </summary>
public class HexPulsePuzzle : BasePuzzle
{
    [Header("Assets")]
    public GameObject hexPrefab;
    public Sprite straightSprite;
    public Sprite curveSprite;
    public Sprite batterySprite;
    public Sprite[] batteryAnimFrames;

    [Header("Grid")]
    [Min(3)] public int width = 5;
    [Min(3)] public int height = 5;
    public RectTransform container;
    public float hexSize = 100f;

    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    private HexTile[,] grid;
    private int[,] solutionRotations;
    private bool[,] solutionPath;
    private bool[,] solutionStraight;
    private Vector2Int batteryPos;
    private Vector2Int sourcePos;
    private List<Vector2Int> solutionRoute = new List<Vector2Int>();

    protected override void BuildBoard()
    {
        if (container == null || hexPrefab == null)
        {
            Debug.LogError("HexPulsePuzzle requires both a grid Container and Hex Prefab.");
            return;
        }

        width = Mathf.Max(3, width);
        height = Mathf.Max(3, height);
        batteryPos = new Vector2Int(width / 2, height / 2);
        grid = new HexTile[width, height];
        solutionRotations = new int[width, height];
        solutionPath = new bool[width, height];
        solutionStraight = new bool[width, height];
        solutionRoute.Clear();
        ClearContainer();

        List<Vector2Int> border = new List<Vector2Int>();
        for (int q = 0; q < width; q++)
        for (int r = 0; r < height; r++)
            if (q == 0 || r == 0 || q == width - 1 || r == height - 1)
                border.Add(new Vector2Int(q, r));

        sourcePos = border[RandomRange(0, border.Count)];
        solutionRoute = FindRoute(sourcePos, batteryPos);
        if (solutionRoute == null || solutionRoute.Count < 2)
        {
            Debug.LogError("HexPulsePuzzle could not create a source-to-battery route.");
            return;
        }

        for (int i = 0; i < solutionRoute.Count; i++)
        {
            Vector2Int cell = solutionRoute[i];
            solutionPath[cell.x, cell.y] = true;

            if (i == 0)
            {
                solutionRotations[cell.x, cell.y] = DirectionIndex(cell, solutionRoute[i + 1]) * 60;
            }
            else if (i == solutionRoute.Count - 1)
            {
                solutionRotations[cell.x, cell.y] = DirectionIndex(cell, solutionRoute[i - 1]) * 60;
            }
            else
            {
                int toPrevious = DirectionIndex(cell, solutionRoute[i - 1]);
                int toNext = DirectionIndex(cell, solutionRoute[i + 1]);
                bool straight = (toPrevious + 3) % 6 == toNext;
                int rotation = FindShapeRotation(straight, toPrevious, toNext);
                solutionStraight[cell.x, cell.y] = straight;
                solutionRotations[cell.x, cell.y] = rotation * 60;
            }
        }

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                GameObject go = Instantiate(hexPrefab, container);
                RectTransform rect = go.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = GetHexPosition(q, r);
                    rect.sizeDelta = new Vector2(hexSize * 2f, hexSize * 2f);
                }

                HexTile tile = go.GetComponent<HexTile>();
                if (tile == null)
                {
                    Debug.LogError("HexPulsePuzzle hex prefab is missing HexTile.");
                    Destroy(go);
                    continue;
                }

                tile.q = q;
                tile.r = r;
                if (q == batteryPos.x && r == batteryPos.y)
                {
                    tile.Init(HexType.Battery, batterySprite, batteryAnimFrames);
                    tile.SetRotation(solutionRotations[q, r]);
                }
                else if (q == sourcePos.x && r == sourcePos.y)
                {
                    tile.Init(HexType.Source, straightSprite);
                    tile.SetRotation(solutionRotations[q, r]);
                }
                else if (solutionPath[q, r])
                {
                    bool isStraight = solutionStraight[q, r];
                    tile.Init(isStraight ? HexType.Straight : HexType.Curve,
                        isStraight ? straightSprite : curveSprite);
                    tile.SetRotation(solutionRotations[q, r]);
                }
                else
                {
                    HexType type = rng.Next(0, 2) == 0 ? HexType.Straight : HexType.Curve;
                    tile.Init(type, type == HexType.Straight ? straightSprite : curveSprite);
                    tile.SetRotation(rng.Next(0, 6) * 60);
                }

                grid[q, r] = tile;
                tile.OnClicked += HandleTileClicked;
            }
        }

        UpdateFlow();
    }

    private void ClearContainer()
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    private Vector2 GetHexPosition(int q, int r)
    {
        float x = Mathf.Sqrt(3f) * hexSize * (q + r * 0.5f);
        float y = 1.5f * hexSize * r;
        float originX = Mathf.Sqrt(3f) * hexSize * (batteryPos.x + batteryPos.y * 0.5f);
        float originY = 1.5f * hexSize * batteryPos.y;
        return new Vector2(x - originX, y - originY);
    }

    private List<Vector2Int> FindRoute(Vector2Int start, Vector2Int goal)
    {
        // Build a monotonic axial-coordinate route from at most two adjacent
        // directions. This guarantees a simple path: no cell can be revisited,
        // and every bend is exactly 60 degrees, matching the curve tile ports.
        int dq = goal.x - start.x;
        int dr = goal.y - start.y;
        List<int> steps = new List<int>();

        if (dq >= 0 && dr >= 0)
        {
            AddSteps(steps, 0, dq);
            AddSteps(steps, 1, dr);
        }
        else if (dq <= 0 && dr <= 0)
        {
            AddSteps(steps, 3, -dq);
            AddSteps(steps, 4, -dr);
        }
        else if (dq >= 0)
        {
            if (dq + dr >= 0)
            {
                AddSteps(steps, 0, dq + dr);
                AddSteps(steps, 5, -dr);
            }
            else
            {
                AddSteps(steps, 5, dq);
                AddSteps(steps, 4, -dr - dq);
            }
        }
        else
        {
            if (dq + dr >= 0)
            {
                AddSteps(steps, 2, -dq);
                AddSteps(steps, 1, dq + dr);
            }
            else
            {
                AddSteps(steps, 2, dr);
                AddSteps(steps, 3, -dq - dr);
            }
        }

        // Shuffle only the order of the two adjacent step directions. All moves
        // remain monotonic toward the goal, so the route cannot cross itself.
        for (int i = steps.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            int temp = steps[i];
            steps[i] = steps[j];
            steps[j] = temp;
        }

        List<Vector2Int> route = new List<Vector2Int> { start };
        Vector2Int current = start;
        foreach (int direction in steps)
        {
            current += Directions[direction];
            if (!InsideGrid(current))
            {
                Debug.LogError("HexPulsePuzzle generated a route outside its grid.");
                return null;
            }
            route.Add(current);
        }

        return current == goal ? route : null;
    }

    private static void AddSteps(List<int> steps, int direction, int count)
    {
        for (int i = 0; i < count; i++)
            steps.Add(direction);
    }

    private bool InsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private int DirectionIndex(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        for (int i = 0; i < Directions.Length; i++)
            if (Directions[i] == delta) return i;
        return -1;
    }

    private int FindShapeRotation(bool straight, int directionA, int directionB)
    {
        for (int rotation = 0; rotation < 6; rotation++)
        {
            int first = rotation;
            int second = straight ? (rotation + 3) % 6 : (rotation + 2) % 6;
            if ((first == directionA && second == directionB) ||
                (first == directionB && second == directionA))
                return rotation;
        }
        return 0;
    }

    private bool HasPort(HexTile tile, int direction)
    {
        if (tile == null) return false;
        int rotation = ((tile.Rotation / 60) % 6 + 6) % 6;
        switch (tile.Type)
        {
            case HexType.Source:
            case HexType.Battery:
                return direction == rotation;
            case HexType.Straight:
                return direction == rotation || direction == (rotation + 3) % 6;
            case HexType.Curve:
                return direction == rotation || direction == (rotation + 2) % 6;
            default:
                return false;
        }
    }

    private void HandleTileClicked(HexTile tile)
    {
        if (tile == null || !CanAcceptInput() || tile.Type == HexType.Battery || tile.Type == HexType.Source)
            return;
        tile.Rotate();
        UpdateFlow();
    }

    private void UpdateFlow()
    {
        if (grid == null) return;
        bool[,] powered = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(sourcePos);
        powered[sourcePos.x, sourcePos.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            HexTile currentTile = grid[current.x, current.y];
            for (int direction = 0; direction < Directions.Length; direction++)
            {
                if (!HasPort(currentTile, direction)) continue;
                Vector2Int next = current + Directions[direction];
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                    continue;
                if (powered[next.x, next.y]) continue;

                HexTile nextTile = grid[next.x, next.y];
                int opposite = (direction + 3) % 6;
                if (!HasPort(nextTile, opposite)) continue;

                powered[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        foreach (var tile in grid)
            if (tile != null) tile.SetPowered(powered[tile.q, tile.r]);

        if (powered[batteryPos.x, batteryPos.y])
            Solve();
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || grid == null) return;
        foreach (Vector2Int cell in solutionRoute)
        {
            HexTile tile = grid[cell.x, cell.y];
            if (tile != null && tile.Rotation != solutionRotations[cell.x, cell.y])
            {
                tile.SetRotation(solutionRotations[cell.x, cell.y]);
                UpdateFlow();
                return;
            }
        }
        UpdateFlow();
    }
}

public enum HexType { Straight, Curve, Source, Battery }

public class HexTile : MonoBehaviour
{
    public HexType Type;
    public int q, r;
    public int Rotation;
    public event System.Action<HexTile> OnClicked;

    private Image image;
    private Sprite[] batteryFrames;
    private Sprite batterySpriteFallback;
    private UISpriteSheetAnimator batteryAnimator;

    private void Awake()
    {
        image = GetComponent<Image>();
        Button button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OnClick);
    }

    public void Init(HexType type, Sprite sprite, Sprite[] animationFrames = null)
    {
        Type = type;
        if (image == null) image = GetComponent<Image>();
        if (image != null) image.sprite = sprite;
        batteryFrames = animationFrames;
        batterySpriteFallback = sprite;
        SetPowered(false);
    }

    public void Rotate()
    {
        SetRotation(Rotation + 60);
    }

    public void SetRotation(int degrees)
    {
        Rotation = degrees % 360;
        if (Rotation < 0) Rotation += 360;
        Rotation = (Mathf.RoundToInt(Rotation / 60f) % 6) * 60;
        transform.localRotation = Quaternion.Euler(0f, 0f, Rotation);
    }

    public void SetPowered(bool powered)
    {
        if (image == null) image = GetComponent<Image>();
        if (image != null)
            image.color = powered ? (Type == HexType.Battery ? new Color(0.45f, 1f, 0.65f) : Color.cyan) : Color.white;

        if (Type == HexType.Battery && image != null)
        {
            if (batteryAnimator == null) batteryAnimator = image.GetComponent<UISpriteSheetAnimator>();
            if (powered && batteryFrames != null && batteryFrames.Length > 0)
            {
                if (batteryAnimator.frames != batteryFrames) batteryAnimator.PlaySet(batteryFrames, 12f, false, () => {
                    if (image != null && batterySpriteFallback != null) image.sprite = batterySpriteFallback;
                });
            }
            else if (batteryAnimator != null)
            {
                batteryAnimator.Stop();
                if (batterySpriteFallback != null) image.sprite = batterySpriteFallback;
            }
        }
    }

    public void OnClick()
    {
        OnClicked?.Invoke(this);
    }
}
