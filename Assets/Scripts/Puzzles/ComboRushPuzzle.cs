using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Match-clear puzzle overlay. The player taps a tile to clear the
// orthogonally-connected same-colour group (size >= 2); tiles above fall and
// new ones spawn from the top. Chaining clears keeps a combo window alive and
// raises the score multiplier.
public class ComboRushPuzzle : BasePuzzle
{
    [Header("ComboRush Grid")]
    public int columns = 6;
    public int rows = 6;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 105f;

    [Header("Art")]
    public Sprite gemSprite; // neutral gem tinted by the colour palette

    [Header("ComboRush UI")]
    public Slider comboTimerSlider;
    public TMP_Text comboText;
    public TMP_Text multiplierText;
    public TMP_Text scoreText;

    private const float ComboWindow = 3f;

    private static readonly Color[] palette =
    {
        new Color(0.92f, 0.26f, 0.26f), // red
        new Color(0.26f, 0.62f, 0.92f), // blue
        new Color(0.35f, 0.82f, 0.40f), // green
        new Color(0.96f, 0.80f, 0.25f), // yellow
        new Color(0.70f, 0.40f, 0.90f), // purple
    };

    private ComboRushTile[,] tiles;
    private int score;
    private int comboCount;
    private int multiplier = 1;
    private float comboTimeRemaining;
    private bool comboActive;

    private float CenterX => (columns - 1) * tileSpacing * 0.5f;
    private float CenterY => (rows - 1) * tileSpacing * 0.5f;

    private int Target => 800 + difficulty * 400;

    protected override void BuildBoard()
    {
        ClearContainer();

        tiles = new ComboRushTile[columns, rows];
        score = 0;
        comboCount = 0;
        multiplier = 1;
        comboActive = false;
        comboTimeRemaining = 0f;

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                CreateTile(c, r);
            }
        }

        UpdateHud();
        if (comboTimerSlider != null) comboTimerSlider.value = 0f;
    }

    private void ClearContainer()
    {
        if (gridContainer == null) return;
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(gridContainer.GetChild(i).gameObject);
        }
    }

    private ComboRushTile CreateTile(int col, int row, bool animate = false)
    {
        GameObject go = Instantiate(tilePrefab, gridContainer);
        var tile = go.GetComponent<ComboRushTile>();
        int colorID = rng.Next(palette.Length);
        tile.Init(col, row, colorID);
        tile.SetColor(palette[colorID]);
        if (gemSprite != null) tile.SetSprite(gemSprite);
        
        if (animate)
        {
            var rt = (RectTransform)tile.transform;
            rt.anchoredPosition = new Vector2(col * tileSpacing - CenterX, (rows + 1) * tileSpacing + CenterY);
            SetTilePosition(tile, col, row, true);
        }
        else
        {
            SetTilePosition(tile, col, row, false);
        }

        tile.OnClicked += HandleTileClicked;
        tiles[col, row] = tile;
        return tile;
    }

    private void SetTilePosition(ComboRushTile tile, int col, int row, bool animate = false)
    {
        tile.x = col;
        tile.y = row;
        Vector2 target = new Vector2(col * tileSpacing - CenterX, -(row * tileSpacing) + CenterY);
        if (animate)
        {
            tile.MoveTo(target);
        }
        else
        {
            var rt = (RectTransform)tile.transform;
            rt.anchoredPosition = target;
        }
    }

    private void HandleTileClicked(ComboRushTile tile)
    {
        if (!CanAcceptInput() || tile == null) return;
        if (tiles[tile.x, tile.y] != tile) return;

        List<ComboRushTile> group = GetGroup(tile);
        if (group.Count < 2) return;

        ResolveClear(group);
    }

    private void ResolveClear(List<ComboRushTile> group)
    {
        int groupSize = group.Count;
        score += groupSize * multiplier;

        foreach (var t in group)
        {
            tiles[t.x, t.y] = null;
            t.PopAndDestroy(0.15f); // Animated clear
        }

        // Combo window: refill and raise multiplier on each successful clear.
        comboCount++;
        comboActive = true;
        comboTimeRemaining = ComboWindow;
        multiplier++;

        ApplyGravityAndRefill();
        UpdateHud();

        if (score >= Target) Solve();
    }

    private void ApplyGravityAndRefill()
    {
        for (int c = 0; c < columns; c++)
        {
            int write = rows - 1;
            for (int r = rows - 1; r >= 0; r--)
            {
                if (tiles[c, r] != null)
                {
                    if (r != write)
                    {
                        ComboRushTile moved = tiles[c, r];
                        tiles[c, r] = null;
                        tiles[c, write] = moved;
                        SetTilePosition(moved, c, write, true);
                    }
                    write--;
                }
            }

            for (int r = write; r >= 0; r--)
            {
                CreateTile(c, r, true);
            }
        }
    }

    private List<ComboRushTile> GetGroup(ComboRushTile start)
    {
        var result = new List<ComboRushTile>();
        if (start == null) return result;

        int targetColor = start.colorID;
        var visited = new bool[columns, rows];
        var stack = new Stack<ComboRushTile>();
        stack.Push(start);
        visited[start.x, start.y] = true;

        while (stack.Count > 0)
        {
            ComboRushTile t = stack.Pop();
            result.Add(t);

            TryQueue(t.x + 1, t.y, targetColor, visited, stack);
            TryQueue(t.x - 1, t.y, targetColor, visited, stack);
            TryQueue(t.x, t.y + 1, targetColor, visited, stack);
            TryQueue(t.x, t.y - 1, targetColor, visited, stack);
        }

        return result;
    }

    private void TryQueue(int c, int r, int targetColor, bool[,] visited, Stack<ComboRushTile> stack)
    {
        if (c < 0 || c >= columns || r < 0 || r >= rows) return;
        if (visited[c, r]) return;
        ComboRushTile t = tiles[c, r];
        if (t == null || t.colorID != targetColor) return;
        visited[c, r] = true;
        stack.Push(t);
    }

    private void UpdateHud()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
        if (comboText != null) comboText.text = "COMBO: " + comboCount;
        if (multiplierText != null) multiplierText.text = "x" + ((float)multiplier).ToString("0.0");
    }

    protected override void Update()
    {
        base.Update();

        if (isSolved || isTimedOut) return;

        if (comboActive)
        {
            comboTimeRemaining -= Time.deltaTime;
            if (comboTimerSlider != null)
                comboTimerSlider.value = Mathf.Clamp01(comboTimeRemaining / ComboWindow);

            if (comboTimeRemaining <= 0f)
            {
                comboActive = false;
                comboCount = 0;
                multiplier = 1;
                comboTimeRemaining = 0f;
                if (comboTimerSlider != null) comboTimerSlider.value = 0f;
                UpdateHud();
            }
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;

        List<ComboRushTile> group = FindAnyGroup();
        if (group != null)
        {
            ResolveClear(group);
        }
        else
        {
            ReshuffleColors();
        }
    }

    private List<ComboRushTile> FindAnyGroup()
    {
        if (tiles == null) return null;
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                ComboRushTile t = tiles[c, r];
                if (t == null) continue;
                List<ComboRushTile> group = GetGroup(t);
                if (group.Count >= 2) return group;
            }
        }
        return null;
    }

    private void ReshuffleColors()
    {
        if (tiles == null) return;
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                ComboRushTile t = tiles[c, r];
                if (t == null) continue;
                int colorID = rng.Next(palette.Length);
                t.colorID = colorID;
                t.SetColor(palette[colorID]);
            }
        }
    }
}
