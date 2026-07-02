using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CardMatchingPuzzle : BasePuzzle
{
    [Header("Card Matching")]
    public int columns = 4;             // 4 x 3 = 12 cards = 6 pairs
    public int rows = 3;
    public GameObject cardPrefab;
    public RectTransform gridContainer;
    public float tileSpacing = 245f;
    public float cardSize = 220f;       // per-card size; larger & easier to read

    [Header("Start Preview")]
    [Tooltip("Seconds the cards are shown face-up before they flip down and the clock starts.")]
    public float previewDuration = 2.5f;

    [Header("Art")]
    public Sprite cardBack;
    public Sprite[] cardFaces;
    public Sprite[] flipAnimation;

    private GridTile[,] _grid;
    private int[,] _faceIDs;
    private GridTile _firstSelected;
    private GridTile _secondSelected;
    private bool _busy;
    private int _matchesFound;
    private int _totalPairs;

    protected override void BuildBoard()
    {
        _busy = false;
        _firstSelected = null;
        _secondSelected = null;
        _matchesFound = 0;

        int n = columns * rows;
        if (n % 2 != 0) n--; // Ensure even number for pairs
        _totalPairs = n / 2;

        if (gridContainer != null)
        {
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
                Destroy(gridContainer.GetChild(i).gameObject);
        }

        List<int> faces = new List<int>();
        for (int i = 0; i < _totalPairs; i++)
        {
            faces.Add(i % cardFaces.Length);
            faces.Add(i % cardFaces.Length);
        }
        Shuffle(faces);

        _grid = new GridTile[columns, rows];
        _faceIDs = new int[columns, rows];

        float centerW = (columns - 1) * tileSpacing * 0.5f;
        float centerH = (rows - 1) * tileSpacing * 0.5f;

        int idx = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (idx >= faces.Count) break;

                int faceID = faces[idx++];
                _faceIDs[x, y] = faceID;

                GameObject go = Instantiate(cardPrefab, gridContainer);
                RectTransform rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(cardSize, cardSize);
                rt.anchoredPosition = new Vector2(x * tileSpacing - centerW, -(y * tileSpacing) + centerH);

                GridTile gt = go.GetComponent<GridTile>();
                gt.Init(x, y, faceID);
                gt.SetSprite(cardBack);
                gt.OnClicked += OnCardClicked;
                _grid[x, y] = gt;
            }
        }
    }

    // Show every card face-up first so the player can memorise the layout, then
    // flip them all face-down and ONLY THEN start the fuse clock / accept input.
    public override void StartPuzzle()
    {
        StartCoroutine(PreviewThenStart());
    }

    private IEnumerator PreviewThenStart()
    {
        // Block input (CanAcceptInput is false while !isRunning) and bots during preview.
        _busy = true;

        // 1. Reveal all faces.
        foreach (var t in _grid)
        {
            if (t == null) continue;
            if (t.type >= 0 && t.type < cardFaces.Length)
                t.SetSprite(cardFaces[t.type]);
        }

        // 2. Let the player study the board.
        yield return new WaitForSeconds(Mathf.Max(0f, previewDuration));

        // 3. Flip every card face-down (with the flip animation when available).
        if (flipAnimation != null && flipAnimation.Length > 0)
        {
            foreach (var t in _grid)
            {
                if (t == null) continue;
                var anim = t.GetComponent<UISpriteSheetAnimator>();
                if (anim != null) anim.PlaySet(flipAnimation, 30f, false);
            }
            yield return new WaitForSeconds(0.35f);
        }

        foreach (var t in _grid)
        {
            if (t == null) continue;
            var anim = t.GetComponent<UISpriteSheetAnimator>();
            if (anim != null) anim.Stop();
            t.SetSprite(cardBack);
        }

        _busy = false;

        // 4. Now start the fuse timer and accept input.
        base.StartPuzzle();
    }

    private void OnCardClicked(GridTile tile)
    {
        if (!CanAcceptInput() || _busy) return;
        if (tile == _firstSelected) return;

        StartCoroutine(FlipCard(tile));
    }

    private IEnumerator FlipCard(GridTile tile)
    {
        if (_busy) yield break;
        _busy = true;

        // Animation
        UISpriteSheetAnimator anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim != null && flipAnimation != null && flipAnimation.Length > 0)
        {
            bool done = false;
            // Play flip animation faster for production feel
            anim.PlaySet(flipAnimation, 50f, false, () => done = true);
            
            float timeout = 0.5f; // Short timeout
            while (!done && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            anim.Stop(); 
        }

        // Guaranteed reveal of the face after animation attempt
        if (tile.type >= 0 && tile.type < cardFaces.Length)
        {
            tile.SetSprite(cardFaces[tile.type]);
        }

        if (_firstSelected == null)
        {
            _firstSelected = tile;
            _busy = false;
        }
        else
        {
            _secondSelected = tile;
            yield return new WaitForSeconds(0.4f);
            CheckMatch();
            // _busy is set to false in CheckMatch or MismatchAnimation
        }
    }

    private void CheckMatch()
    {
        if (_firstSelected.type == _secondSelected.type)
        {
            _firstSelected.OnClicked -= OnCardClicked;
            _secondSelected.OnClicked -= OnCardClicked;
            _firstSelected.Image.color = new Color(1, 1, 1, 0.7f);
            _secondSelected.Image.color = new Color(1, 1, 1, 0.7f);
            _matchesFound++;
            
            _firstSelected = null;
            _secondSelected = null;
            _busy = false;

            if (_matchesFound >= _totalPairs) Solve();
        }
        else
        {
            StartCoroutine(MismatchAnimation(_firstSelected, _secondSelected));
        }
    }

    private IEnumerator MismatchAnimation(GridTile t1, GridTile t2)
    {
        yield return new WaitForSeconds(0.6f);

        UISpriteSheetAnimator anim1 = t1.GetComponent<UISpriteSheetAnimator>();
        UISpriteSheetAnimator anim2 = t2.GetComponent<UISpriteSheetAnimator>();

        if (anim1 != null && flipAnimation != null && flipAnimation.Length > 0)
        {
            bool d1 = false, d2 = false;
            anim1.PlaySet(flipAnimation, 30f, false, () => d1 = true);
            anim2.PlaySet(flipAnimation, 30f, false, () => d2 = true);
            
            float timeout = 1.0f;
            while ((!d1 || !d2) && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        t1.SetSprite(cardBack);
        t2.SetSprite(cardBack);
        
        _firstSelected = null;
        _secondSelected = null;
        _busy = false;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public override void AutoSolveChunk()
    {
        // Bot just finds a pair
        if (_busy) return;
        
        List<GridTile> remaining = new List<GridTile>();
        foreach (var t in _grid)
        {
            if (t != null && t.Image.sprite == cardBack) remaining.Add(t);
        }

        if (remaining.Count >= 2)
        {
            GridTile t1 = remaining[0];
            GridTile t2 = remaining.Find(x => x != t1 && x.type == t1.type);
            if (t2 != null)
            {
                StartCoroutine(AutoMatch(t1, t2));
            }
        }
    }

    private IEnumerator AutoMatch(GridTile t1, GridTile t2)
    {
        _busy = true;
        t1.SetSprite(cardFaces[t1.type]);
        yield return new WaitForSeconds(0.2f);
        t2.SetSprite(cardFaces[t2.type]);
        yield return new WaitForSeconds(0.2f);
        _firstSelected = t1;
        _secondSelected = t2;
        CheckMatch();
    }
}