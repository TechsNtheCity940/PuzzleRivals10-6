using UnityEngine;
using System;
using System.Collections.Generic;

// Combo Rush — N numbered cells scattered in a 5x5 grid. Click them in ascending
// order 1..N. A wrong click resets your run back to 1. Claimed cells tint green.
public class ComboRush : PuzzleBase
{
    [Header("Assets")]
    public Sprite comboNodeSprite;

    [Header("Match Burst FX")]
    public Sprite[] matchBurstFrames;
    public float matchBurstFps = 30f;

    private const int GRID_SIZE = 5;
    private const float SPACING = 1.0f;
    private const float CELL_SIZE = 0.85f;

    private int sequenceLength = 5;
    private int currentTarget = 1;
    private readonly List<ComboRushCell> cells = new List<ComboRushCell>();

    public override void Setup(int d)
    {
        base.Setup(d);
        sequenceLength = Mathf.Clamp(5 + d, 5, 10);
        BuildBoard();
    }

    private void BuildBoard()
    {
        foreach (var c in cells) if (c != null) Destroy(c.gameObject);
        cells.Clear();
        currentTarget = 1;

        int totalSlots = GRID_SIZE * GRID_SIZE;
        List<int> slots = new List<int>();
        for (int i = 0; i < totalSlots; i++) slots.Add(i);
        // Fisher-Yates shuffle to pick distinct slots
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int tmp = slots[i]; slots[i] = slots[j]; slots[j] = tmp;
        }

        float start = -(GRID_SIZE - 1) * SPACING / 2f;

        for (int n = 0; n < sequenceLength; n++)
        {
            int slot = slots[n];
            int col = slot % GRID_SIZE;
            int row = slot / GRID_SIZE;
            int number = n + 1;

            Vector3 pos = new Vector3(start + col * SPACING, -start - row * SPACING, 0f);
            ComboRushCell cell = CreateCell(number, pos);
            cell.OnClicked += HandleCellClick;
            cells.Add(cell);
        }
    }

    private ComboRushCell CreateCell(int number, Vector3 pos)
    {
        GameObject go = new GameObject("ComboCell_" + number);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;

        ComboRushCell cell = go.AddComponent<ComboRushCell>();
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        // Background sprite (cyan/lime tint)
        GameObject bgGo = new GameObject("BG");
        bgGo.transform.SetParent(go.transform, false);
        SpriteRenderer bgR = bgGo.AddComponent<SpriteRenderer>();
        bgR.sprite = comboNodeSprite;
        bgR.sortingOrder = 6;
        ScaleRenderer(bgGo.transform, bgR, CELL_SIZE);

        // Node sprite (white/light-green tint)
        GameObject nodeGo = new GameObject("Node");
        nodeGo.transform.SetParent(go.transform, false);
        SpriteRenderer nodeR = nodeGo.AddComponent<SpriteRenderer>();
        nodeR.sprite = comboNodeSprite;
        nodeR.sortingOrder = 9;
        ScaleRenderer(nodeGo.transform, nodeR, CELL_SIZE * 0.78f);

        // Number label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        TextMesh tm = labelGo.AddComponent<TextMesh>();
        tm.text = number.ToString();
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.12f;
        tm.fontSize = 40;
        tm.color = Color.white;
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont != null)
        {
            tm.font = builtinFont;
            MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
            mr.sharedMaterial = builtinFont.material;
            mr.sortingOrder = 20;
        }

        cell.Setup(number, bgR, nodeR);
        cell.SetClaimed(false);
        return cell;
    }

    private void ScaleRenderer(Transform t, SpriteRenderer r, float targetSize)
    {
        if (r.sprite == null) return;
        float native = Mathf.Max(r.sprite.bounds.size.x, r.sprite.bounds.size.y);
        if (native <= 0.0001f) return;
        float s = targetSize / native;
        t.localScale = new Vector3(s, s, 1f);
    }

    private void HandleCellClick(ComboRushCell cell)
    {
        if (!inputEnabled || isSolved) return;
        TryClaim(cell);
    }

    private void TryClaim(ComboRushCell cell)
    {
        if (isSolved || cell == null || cell.claimed) return;

        if (cell.number == currentTarget)
        {
            cell.SetClaimed(true);
            SpawnMatchBurst(cell.transform.position);
            currentTarget++;
            PlayPlace();

            if (currentTarget > sequenceLength)
            {
                PlaySolve();
                Solve(80 + difficulty * 35);
            }
        }
        else
        {
            foreach (var c in cells) c.SetClaimed(false);
            currentTarget = 1;
            PlayRotate();
        }
    }

    // AIBot calls this once via SendMessage. The bot always plays the correct
    // ascending order, so it fully solves the puzzle.
    public void BotSolve()
    {
        int guard = 0;
        while (!isSolved && guard < 200)
        {
            guard++;
            ComboRushCell target = cells.Find(c => c.number == currentTarget);
            if (target == null) break;
            TryClaim(target);
        }
    }

    private static readonly Color[] BurstTints =
    {
        new Color(0f, 0.9f, 1f),    // cyan
        new Color(1f, 0.18f, 0.78f),// magenta
        new Color(1f, 0.78f, 0.1f), // amber
        new Color(0.61f, 1f, 0.18f),// green
        new Color(1f, 0.3f, 0.3f),  // red
    };

    private void SpawnMatchBurst(Vector3 worldPos)
    {
        if (matchBurstFrames == null || matchBurstFrames.Length == 0) return;

        GameObject go = new GameObject("ComboMatchBurst");
        go.transform.position = worldPos + new Vector3(0f, 0f, -0.5f);
        go.transform.localScale = Vector3.one * 1.1f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 30;
        sr.color = BurstTints[UnityEngine.Random.Range(0, BurstTints.Length)];

        SpriteSheetAnimator anim = go.AddComponent<SpriteSheetAnimator>();
        anim.playOnAwake = false;
        anim.destroyOnComplete = true;
        anim.SetFrames(matchBurstFrames, matchBurstFps, false);
        anim.Play();
    }

    private void PlayPlace()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzlePlace, 0.6f);
    }

    private void PlayRotate()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
    }

    private void PlaySolve()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.8f);
    }
}

public class ComboRushCell : MonoBehaviour
{
    public int number;
    public bool claimed;

    private SpriteRenderer bgRenderer;
    private SpriteRenderer nodeRenderer;

    public event Action<ComboRushCell> OnClicked;

    public void Setup(int number, SpriteRenderer bg, SpriteRenderer node)
    {
        this.number = number;
        bgRenderer = bg;
        nodeRenderer = node;
    }

    public void SetClaimed(bool c)
    {
        claimed = c;
        if (bgRenderer != null)
            bgRenderer.color = c ? new Color(0.61f, 1f, 0.18f, 1f) : new Color(0f, 0.9f, 1f, 1f);
        if (nodeRenderer != null)
            nodeRenderer.color = c ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(this);
    }
}
