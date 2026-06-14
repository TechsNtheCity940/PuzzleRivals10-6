using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Rotate-to-calibrate overlay puzzle. Fills a columns x rows grid of
// CircuitScrambleTile, each spawned at a wrong rotation. Clicking a tile
// rotates it +90 degrees; when every tile matches its target rotation the
// calibration reaches 100% and the puzzle is solved.
public class CircuitScramblePuzzle : BasePuzzle
{
    [Header("Circuit Scramble")]
    public int columns = 4;
    public int rows = 4;
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
    private readonly List<int> targetRotations = new List<int>();

    private static readonly int[] RotationSteps = { 0, 90, 180, 270 };

    protected override void BuildBoard()
    {
        ClearBoard();

        if (calibrationSlider != null)
        {
            calibrationSlider.minValue = 0f;
            calibrationSlider.maxValue = 1f;
        }
        if (requirementText != null) requirementText.text = "Align all circuits";

        if (tilePrefab != null && gridContainer != null)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    GameObject go = Instantiate(tilePrefab, gridContainer);
                    RectTransform rt = (RectTransform)go.transform;
                    rt.anchoredPosition = GridPosition(x, y);

                    CircuitScrambleTile tile = go.GetComponent<CircuitScrambleTile>();
                    if (tile == null) continue;

                    int type = rng.Next(0, 4);
                    int target = RotationSteps[rng.Next(0, RotationSteps.Length)];
                    int current;
                    do { current = RotationSteps[rng.Next(0, RotationSteps.Length)]; }
                    while (current == target);

                    tile.Init(type, x, y, current);
                    if (tileSprite != null)
                    {
                        tile.SetSprite(tileSprite);
                        tile.Image.color = Color.white;
                    }

                    tiles.Add(tile);
                    targetRotations.Add(target);

                    tile.OnClicked += OnTileClicked;
                }
            }
        }

        UpdateCalibration();
    }

    private void OnTileClicked(CircuitScrambleTile tile)
    {
        if (!CanAcceptInput()) return;
        int index = tiles.IndexOf(tile);
        if (index < 0) return;

        tile.Rotate();
        UpdateCalibration();
        CheckSolved();
    }

    public override void AutoSolveChunk()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null && tiles[i].rotation != targetRotations[i])
            {
                tiles[i].Rotate();
                UpdateCalibration();
                CheckSolved();
                return;
            }
        }
    }

    private void CheckSolved()
    {
        if (tiles.Count > 0 && CorrectCount() == tiles.Count) Solve();
    }

    private int CorrectCount()
    {
        int c = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null && tiles[i].rotation == targetRotations[i]) c++;
        }
        return c;
    }

    private void UpdateCalibration()
    {
        int total = tiles.Count;
        float frac = total > 0 ? (float)CorrectCount() / total : 0f;
        if (calibrationSlider != null) calibrationSlider.value = frac;
        if (calibrationText != null)
            calibrationText.text = "CALIBRATION: " + Mathf.RoundToInt(frac * 100f) + "%";
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
        targetRotations.Clear();
    }
}
