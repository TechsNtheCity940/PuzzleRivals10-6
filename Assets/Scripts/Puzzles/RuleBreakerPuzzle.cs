using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "Follow the rule" puzzle rendered as a Canvas overlay. Each round shows a rule
// (e.g. "Tap all RED tiles"); the player taps matching tiles for points. Solved
// when the score reaches a difficulty-scaled target.
public class RuleBreakerPuzzle : BasePuzzle
{
    [Header("RuleBreaker")]
    public int columns = 5;
    public int rows = 5;
    public GameObject tilePrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 110f;
    public TMPro.TMP_Text ruleDescriptionText;
    public TMPro.TMP_Text ruleTimerText;
    public TMPro.TMP_Text scoreText;
    public Sprite[] shapeSprites;
    public Color[] tileColors;

    private const float RuleDuration = 8f;
    private const int CorrectPoints = 10;
    private const int WrongPenalty = 5;

    private readonly List<RuleBreakerTile> _tiles = new List<RuleBreakerTile>();
    private int _score;
    private int _target;
    private Func<RuleBreakerTile, bool> _ruleMatch;
    private float _ruleTimer;
    private bool _ruleActive;
    private int _matchingRemaining;
    private Coroutine _scoreFlash;

    protected override void BuildBoard()
    {
        _target = 60 + difficulty * 40;
        _score = 0;

        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                GameObject child = gridContainer.GetChild(i).gameObject;
                if (Application.isPlaying) UnityEngine.Object.Destroy(child);
                else UnityEngine.Object.DestroyImmediate(child);
            }
        }
        _tiles.Clear();

        int cols = Mathf.Max(1, columns);
        int rws = Mathf.Max(1, rows);

        // Fit the grid to the container so tiles never overflow or overlap. Each
        // tile is also sized to the spacing (the prefab default was a fixed 100px).
        if (gridContainer != null)
        {
            float availW = gridContainer.rect.width;
            float availH = gridContainer.rect.height;
            if (availW > 1f && availH > 1f)
                tileSpacing = Mathf.Min(availW / cols, availH / rws) * 0.96f;
        }
        float tileSize = tileSpacing * 0.92f;

        int colorCount = (tileColors != null) ? tileColors.Length : 0;
        int shapeCount = (shapeSprites != null) ? shapeSprites.Length : 0;

        float centerX = (cols - 1) * tileSpacing * 0.5f;
        float centerY = (rws - 1) * tileSpacing * 0.5f;

        for (int row = 0; row < rws; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject go = UnityEngine.Object.Instantiate(tilePrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = new Vector2(col * tileSpacing - centerX, -(row * tileSpacing) + centerY);
                rt.sizeDelta = new Vector2(tileSize, tileSize);

                RuleBreakerTile tile = go.GetComponent<RuleBreakerTile>();
                int number = rng.Next(1, 10); // 1..9
                int colorID = colorCount > 0 ? rng.Next(0, colorCount) : 0;
                int shapeID = shapeCount > 0 ? rng.Next(0, shapeCount) : 0;
                tile.Init(col, row, number, colorID, shapeID, tileColors, shapeSprites);
                tile.OnClicked += OnTileClicked;
                _tiles.Add(tile);
            }
        }

        UpdateScore();
        GenerateRule();
    }

    private void GenerateRule()
    {
        // Fresh round: clear previous selections so the whole board is available.
        for (int i = 0; i < _tiles.Count; i++)
        {
            _tiles[i].SetSelected(false);
        }

        int colorCount = (tileColors != null) ? tileColors.Length : 0;
        int shapeCount = (shapeSprites != null) ? shapeSprites.Length : 0;

        // Candidate rule builders. Each returns true and fills out the rule if it
        // currently matches at least one tile.
        List<int> order = new List<int> { 0, 1, 2, 3 };
        Shuffle(order);

        foreach (int type in order)
        {
            Func<RuleBreakerTile, bool> match = null;
            string desc = null;

            switch (type)
            {
                case 0: // colour
                    if (colorCount > 0)
                    {
                        int cid = _tiles[rng.Next(0, _tiles.Count)].colorID;
                        match = t => t.colorID == cid;
                        desc = "Tap all " + ColorName(tileColors[Mathf.Clamp(cid, 0, colorCount - 1)]) + " tiles";
                    }
                    break;
                case 1: // even
                    match = t => t.number % 2 == 0;
                    desc = "Tap EVEN numbers";
                    break;
                case 2: // greater than threshold
                    int thr = 4 + rng.Next(0, 3); // 4..6
                    match = t => t.number > thr;
                    desc = "Tap tiles with number > " + thr;
                    break;
                case 3: // shape
                    if (shapeCount > 0)
                    {
                        int sid = _tiles[rng.Next(0, _tiles.Count)].shapeID;
                        match = t => t.shapeID == sid;
                        desc = "Tap all " + ShapeName(sid) + " tiles";
                    }
                    break;
            }

            if (match != null && CountMatching(match) > 0)
            {
                ApplyRule(match, desc);
                return;
            }
        }

        // Guaranteed fallback: every tile with a number > 0 matches.
        ApplyRule(t => t.number > 0, "Tap ALL tiles");
    }

    private void ApplyRule(Func<RuleBreakerTile, bool> match, string description)
    {
        _ruleMatch = match;
        _matchingRemaining = CountMatching(match);
        _ruleTimer = RuleDuration;
        _ruleActive = true;
        if (ruleDescriptionText != null) ruleDescriptionText.text = description;
        UpdateRuleTimerText();
    }

    private int CountMatching(Func<RuleBreakerTile, bool> match)
    {
        int count = 0;
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (!_tiles[i].isSelected && match(_tiles[i])) count++;
        }
        return count;
    }

    private void OnTileClicked(RuleBreakerTile tile)
    {
        if (!CanAcceptInput()) return;
        if (tile.isSelected) return;
        if (_ruleMatch == null) return;

        if (_ruleMatch(tile))
        {
            ApplyCorrect(tile);
        }
        else
        {
            _score -= WrongPenalty;
            UpdateScore();
            FlashWrong();
        }
    }

    private void ApplyCorrect(RuleBreakerTile tile)
    {
        tile.SetSelected(true);
        tile.GetComponent<GridTile>()?.Pulse(1.3f, 0.2f);
        _score += CorrectPoints;
        _matchingRemaining--;
        UpdateScore();

        if (_score >= _target)
        {
            _ruleActive = false;
            if (ruleDescriptionText != null) ruleDescriptionText.text = "SOLVED!";
            Solve();
            return;
        }

        if (_matchingRemaining <= 0)
        {
            GenerateRule();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!_ruleActive) return;
        if (!CanAcceptInput()) return; // also pauses the rule timer while frozen

        _ruleTimer -= Time.deltaTime;
        if (_ruleTimer < 0f) _ruleTimer = 0f;
        UpdateRuleTimerText();

        if (_ruleTimer <= 0f)
        {
            // Ran out of time on this rule: roll a new one.
            GenerateRule();
        }
    }

    public override void AutoSolveChunk()
    {
        RuleBreakerTile tile = FindMatchingUnselected();
        if (tile == null)
        {
            GenerateRule();
            tile = FindMatchingUnselected();
        }
        if (tile == null) return;
        ApplyCorrect(tile);
    }

    private RuleBreakerTile FindMatchingUnselected()
    {
        if (_ruleMatch == null) return null;
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (!_tiles[i].isSelected && _ruleMatch(_tiles[i])) return _tiles[i];
        }
        return null;
    }

    private void UpdateScore()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + _score;
    }

    private void UpdateRuleTimerText()
    {
        if (ruleTimerText != null) ruleTimerText.text = "TIME " + Mathf.CeilToInt(_ruleTimer);
    }

    private void FlashWrong()
    {
        if (scoreText == null) return;
        if (_scoreFlash != null) StopCoroutine(_scoreFlash);
        _scoreFlash = StartCoroutine(FlashScoreRoutine());
    }

    private IEnumerator FlashScoreRoutine()
    {
        Color original = scoreText.color;
        scoreText.color = new Color(0.95f, 0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.2f);
        scoreText.color = original;
        _scoreFlash = null;
    }

    // Map an arbitrary colour to the nearest common colour name for the rule text.
    private static string ColorName(Color c)
    {
        var named = new (string name, Color col)[]
        {
            ("RED", new Color(0.9f, 0.15f, 0.15f)),
            ("GREEN", new Color(0.2f, 0.7f, 0.25f)),
            ("BLUE", new Color(0.2f, 0.4f, 0.9f)),
            ("YELLOW", new Color(0.95f, 0.9f, 0.2f)),
            ("ORANGE", new Color(0.95f, 0.6f, 0.2f)),
            ("PURPLE", new Color(0.6f, 0.25f, 0.8f)),
            ("CYAN", new Color(0.25f, 0.8f, 0.85f)),
            ("WHITE", new Color(0.95f, 0.95f, 0.95f)),
            ("BLACK", new Color(0.1f, 0.1f, 0.1f)),
        };

        string best = "COLORED";
        float bestDist = float.MaxValue;
        foreach (var entry in named)
        {
            float dr = c.r - entry.col.r;
            float dg = c.g - entry.col.g;
            float db = c.b - entry.col.b;
            float dist = dr * dr + dg * dg + db * db;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = entry.name;
            }
        }
        return best;
    }

    private string ShapeName(int shapeID)
    {
        if (shapeSprites != null && shapeID >= 0 && shapeID < shapeSprites.Length && shapeSprites[shapeID] != null)
        {
            // Sprites are named like "rb_shape_circle" -> show just "CIRCLE".
            string raw = shapeSprites[shapeID].name;
            int us = raw.LastIndexOf('_');
            string clean = (us >= 0 && us < raw.Length - 1) ? raw.Substring(us + 1) : raw;
            return clean.ToUpperInvariant();
        }
        return "SHAPE " + shapeID;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
