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

    // Track which cards are face-down (for AutoSolveChunk)
    private HashSet<GridTile> faceDownCards = new HashSet<GridTile>();

    protected override void BuildBoard()
    {
        _busy = false;
        _firstSelected = null;
        _secondSelected = null;
        _matchesFound = 0;
        faceDownCards.Clear();

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
                faceDownCards.Add(gt);
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
                if (anim != null)
                {
                    anim.PlaySet(flipAnimation, 30f, false, () => {
                        if (t != null) t.SetSprite(cardBack);
                    });
                }
                else
                {
                    t.SetSprite(cardBack);
                }
            }
            yield return new WaitForSeconds(0.35f);
        }
        else
        {
            foreach (var t in _grid)
            {
                if (t == null) continue;
                t.SetSprite(cardBack);
            }
        }

        // Rebuild faceDownCards after animation completes
        faceDownCards.Clear();
        foreach (var t in _grid)
        {
            if (t != null) faceDownCards.Add(t);
        }

        _busy = false;

        // 4. Now start the fuse timer and accept input.
        base.StartPuzzle();
    }

    private void OnCardClicked(UITile tile)
    {
        if (!(tile is GridTile gt)) return;
        if (!CanAcceptInput() || _busy) return;
        if (gt == _firstSelected) return;

        StartCoroutine(FlipCard(gt));
    }

    private IEnumerator FlipCard(GridTile tile)
    {
        if (_busy) yield break;
        _busy = true;

        // Remove from face-down set
        faceDownCards.Remove(tile);

        // Animation - play the flip animation once
        if (flipAnimation != null && flipAnimation.Length > 0)
        {
            UISpriteSheetAnimator anim = tile.GetComponent<UISpriteSheetAnimator>();
            if (anim != null)
            {
                // Play flip animation, when complete set to cardBack (face down)
                anim.PlaySet(flipAnimation, 50f, false, () => {
                    if (flipAnimation.Length > 0) tile.Image.sprite = flipAnimation[0];
                });
            }
            else
            {
                tile.Image.sprite = flipAnimation[0];
            }
            
            // Wait for animation to complete (flipAnimation at 50 FPS with 2 frames = 0.04s)
            yield return new WaitForSeconds(0.05f);
        }

        // Reveal the face after animation complete/reset
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

        // Play flip animation on both cards, then reset to cardBack
        UISpriteSheetAnimator anim1 = t1.GetComponent<UISpriteSheetAnimator>();
        if (anim1 != null && flipAnimation != null && flipAnimation.Length > 0)
        {
            anim1.PlaySet(flipAnimation, 30f, false, () => {
                t1.SetSprite(cardBack);
            });
        }
        else
        {
            t1.SetSprite(cardBack);
        }
        
        UISpriteSheetAnimator anim2 = t2.GetComponent<UISpriteSheetAnimator>();
        if (anim2 != null && flipAnimation != null && flipAnimation.Length > 0)
        {
            anim2.PlaySet(flipAnimation, 30f, false, () => {
                t2.SetSprite(cardBack);
            });
        }
        else
        {
            t2.SetSprite(cardBack);
        }
        
        // Wait for animation to complete (30 FPS with 2 frames = ~0.067s)
        yield return new WaitForSeconds(0.3f);

        // Re-add to face-down set
        if (t1 != null) faceDownCards.Add(t1);
        if (t2 != null) faceDownCards.Add(t2);

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
        
        // Find a face-down card that hasn't been matched
        GridTile t1 = null;
        foreach (var t in faceDownCards)
        {
            if (t != null && t.Image.color.a > 0) // Not yet dimmed (matched)
            {
                t1 = t;
                break;
            }
        }
        
        if (t1 == null) return;

        GridTile t2 = null;
        int targetType = t1.type;
        foreach (var t in faceDownCards)
        {
            if (t != null && t != t1 && t.type == targetType && t.Image.color.a > 0)
            {
                t2 = t;
                break;
            }
        }
        
        if (t2 != null)
        {
            StartCoroutine(AutoMatch(t1, t2));
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