using UnityEngine;
using System;
using System.Collections.Generic;

// Score Attack — 5x5 grid. Cells randomly light up; click them before they fade.
// Solve when target hits scored. Misses don't reset.
public class ScoreAttack : PuzzleBase
{
    [Header("Assets")]
    public Sprite scoreTargetSprite;
    public Sprite floorSprite;

    [Header("Shatter FX")]
    public Sprite[] shatterRedFrames;
    public Sprite[] shatterBlueFrames;
    public Sprite[] shatterGreenFrames;
    public Sprite[] shatterPurpleFrames;
    public float shatterFps = 18f;

    private const int GRID_SIZE = 5;
    private const float SPACING = 1.0f;
    private const float CELL_SIZE = 0.85f;
    private const float SPAWN_INTERVAL = 0.7f;
    private const float FADE_TIME = 1.4f;

    private int targetScore = 10;
    private int score = 0;
    private float spawnTimer = 0f;
    private ScoreAttackCell[] cells;
    private float[] lit;

    public override void Setup(int d)
    {
        base.Setup(d);
        targetScore = Mathf.Clamp(10 + d * 2, 10, 24);
        BuildBoard();
    }

    private void BuildBoard()
    {
        if (cells != null)
            foreach (var c in cells) if (c != null) Destroy(c.gameObject);

        int total = GRID_SIZE * GRID_SIZE;
        cells = new ScoreAttackCell[total];
        lit = new float[total];
        score = 0;
        spawnTimer = 0f;

        float start = -(GRID_SIZE - 1) * SPACING / 2f;
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                int idx = row * GRID_SIZE + col;
                Vector3 pos = new Vector3(start + col * SPACING, -start - row * SPACING, 0f);
                ScoreAttackCell cell = CreateCell(idx, pos);
                cell.OnClicked += HandleCellClick;
                cells[idx] = cell;
                lit[idx] = 0f;
                cell.Repaint(0f);
            }
        }
    }

    private ScoreAttackCell CreateCell(int idx, Vector3 pos)
    {
        GameObject go = new GameObject("ScoreCell_" + idx);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;

        ScoreAttackCell cell = go.AddComponent<ScoreAttackCell>();
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        // Dim floor base
        GameObject baseGo = new GameObject("Floor");
        baseGo.transform.SetParent(go.transform, false);
        SpriteRenderer baseR = baseGo.AddComponent<SpriteRenderer>();
        baseR.sprite = floorSprite;
        baseR.sortingOrder = 5;
        baseR.color = new Color(0.7f, 0.74f, 0.85f, 0.35f);
        ScaleRenderer(baseGo.transform, baseR, CELL_SIZE);

        // Target overlay (alpha & scale driven by lit value)
        GameObject tgtGo = new GameObject("Target");
        tgtGo.transform.SetParent(go.transform, false);
        SpriteRenderer tgtR = tgtGo.AddComponent<SpriteRenderer>();
        tgtR.sprite = scoreTargetSprite;
        tgtR.sortingOrder = 9;
        tgtR.color = Color.white;
        float targetBaseScale = ComputeScale(tgtR, CELL_SIZE);

        cell.Setup(idx, baseR, tgtR, targetBaseScale);
        return cell;
    }

    private float ComputeScale(SpriteRenderer r, float targetSize)
    {
        if (r.sprite == null) return 1f;
        float native = Mathf.Max(r.sprite.bounds.size.x, r.sprite.bounds.size.y);
        if (native <= 0.0001f) return 1f;
        return targetSize / native;
    }

    private void ScaleRenderer(Transform t, SpriteRenderer r, float targetSize)
    {
        float s = ComputeScale(r, targetSize);
        t.localScale = new Vector3(s, s, 1f);
    }

    private void Update()
    {
        if (!inputEnabled || isSolved) return;
        float dt = Time.deltaTime;

        spawnTimer += dt;
        if (spawnTimer >= SPAWN_INTERVAL)
        {
            spawnTimer -= SPAWN_INTERVAL;
            LightRandomCell();
        }

        float decay = dt / FADE_TIME;
        for (int i = 0; i < lit.Length; i++)
        {
            if (lit[i] > 0f)
            {
                lit[i] = Mathf.Max(0f, lit[i] - decay);
                cells[i].Repaint(lit[i]);
            }
        }
    }

    private void LightRandomCell()
    {
        List<int> unlit = new List<int>();
        for (int i = 0; i < lit.Length; i++) if (lit[i] <= 0f) unlit.Add(i);
        if (unlit.Count == 0) return;
        int pick = unlit[UnityEngine.Random.Range(0, unlit.Count)];
        lit[pick] = 1f;
        cells[pick].Repaint(1f);
    }

    private void HandleCellClick(int idx)
    {
        if (!inputEnabled || isSolved) return;
        if (lit[idx] <= 0f) return;

        lit[idx] = 0f;
        cells[idx].Repaint(0f);
        SpawnShatter(cells[idx].transform.position);
        score++;
        PlayPlace();

        if (score >= targetScore)
        {
            PlaySolve();
            Solve(90 + difficulty * 35);
        }
    }

    // AIBot calls this once via SendMessage. This is a time-based puzzle, so the
    // bot cannot wait — directly award the full target score to complete the round.
    public void BotSolve()
    {
        score = targetScore;
        PlaySolve();
        Solve(90 + difficulty * 35);
    }

    private void SpawnShatter(Vector3 worldPos)
    {
        // Pick a random non-empty shatter colour variant (uses all imported sheets).
        Sprite[][] sets = { shatterRedFrames, shatterBlueFrames, shatterGreenFrames, shatterPurpleFrames };
        List<Sprite[]> available = new List<Sprite[]>();
        foreach (var s in sets) if (s != null && s.Length > 0) available.Add(s);
        if (available.Count == 0) return;
        Sprite[] frames = available[UnityEngine.Random.Range(0, available.Count)];

        GameObject go = new GameObject("ShatterFX");
        go.transform.SetParent(transform, false);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 15;

        SpriteSheetAnimator anim = go.AddComponent<SpriteSheetAnimator>();
        anim.playOnAwake = false;
        anim.destroyOnComplete = true;
        anim.SetFrames(frames, shatterFps, false);
        anim.Play();
    }

    private void PlayPlace()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzlePlace, 0.6f);
    }

    private void PlaySolve()
    {
        if (AudioManager.Instance != null && GameState.Instance != null)
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.8f);
    }
}

public class ScoreAttackCell : MonoBehaviour
{
    public int index;

    private SpriteRenderer floorRenderer;
    private SpriteRenderer targetRenderer;
    private float targetBaseScale = 1f;

    public event Action<int> OnClicked;

    public void Setup(int index, SpriteRenderer floor, SpriteRenderer target, float targetBaseScale)
    {
        this.index = index;
        floorRenderer = floor;
        targetRenderer = target;
        this.targetBaseScale = targetBaseScale;
    }

    public void Repaint(float lit)
    {
        if (targetRenderer == null) return;
        if (lit <= 0f)
        {
            targetRenderer.enabled = false;
        }
        else
        {
            targetRenderer.enabled = true;
            float alpha = 0.35f + 0.65f * lit;
            Color c = targetRenderer.color;
            c.a = alpha;
            targetRenderer.color = c;
            float s = targetBaseScale * (0.82f + 0.18f * lit);
            targetRenderer.transform.localScale = new Vector3(s, s, 1f);
        }
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(index);
    }
}
