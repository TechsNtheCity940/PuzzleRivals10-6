using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class TargetShootingPuzzle : BasePuzzle
{
    [Header("Target Shooting")]
    public GameObject targetPrefab; // GridTile based
    public RectTransform spawnArea;
    public int minTargets = 3;
    public int maxTargets = 8;
    public float targetScale = 1.0f;
    public float targetDuration = 3.5f; // Target stays for 3.5 seconds

    [Header("Art")]
    public Sprite[] blueShatterFrames;
    public Sprite[] greenShatterFrames;
    public Sprite[] purpleShatterFrames;
    public Sprite[] redShatterFrames;

    private class ActiveTarget
    {
        public GridTile tile;
        public float spawnTime;
        public float duration;
        public float scale;
        public int colorType;
        public bool isHit;
    }

    private List<ActiveTarget> _activeTargets = new List<ActiveTarget>();
    private int _totalRequiredHits;
    private int _successfulHits;

    protected override void BuildBoard()
    {
        ClearBoard();
        
        _successfulHits = 0;
        _totalRequiredHits = rng.Next(minTargets + difficulty, maxTargets + difficulty);
        if (_totalRequiredHits < 3) _totalRequiredHits = 3; // Ensure at least 3

        // Spawn initial targets (up to 3)
        int initialCount = Mathf.Min(3, _totalRequiredHits);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnTarget();
        }
    }

    private void SpawnTarget()
    {
        if (targetPrefab == null || spawnArea == null) return;

        GameObject go = Instantiate(targetPrefab, spawnArea);
        RectTransform rt = (RectTransform)go.transform;
        
        float width = spawnArea.rect.width > 100 ? spawnArea.rect.width : 2200f;
        float height = spawnArea.rect.height > 100 ? spawnArea.rect.height : 1300f;
        
        // Random position within calculated area, ensuring no overlapping
        float minDistance = 260f; // Minimum distance in pixels to prevent overlaps
        Vector2 pos = Vector2.zero;
        bool posValid = false;
        int retries = 0;

        while (!posValid && retries < 60)
        {
            float rx = (float)(rng.NextDouble() - 0.5f) * (width * 0.85f);
            float ry = (float)(rng.NextDouble() - 0.5f) * (height * 0.75f);
            pos = new Vector2(rx, ry);
            
            posValid = true;
            foreach (var active in _activeTargets)
            {
                if (active != null && active.tile != null)
                {
                    float dist = Vector2.Distance(((RectTransform)active.tile.transform).anchoredPosition, pos);
                    if (dist < minDistance)
                    {
                        posValid = false;
                        break;
                    }
                }
            }
            retries++;
        }

        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one * targetScale;

        GridTile tile = go.GetComponent<GridTile>();
        if (tile != null)
        {
            int type = rng.Next(0, 4); // Color type
            tile.Init(0, 0, type);
            
            Sprite[] frames = GetFramesForType(type);
            if (frames != null && frames.Length > 0)
            {
                tile.SetSprite(frames[0]);
            }

            tile.OnClicked += (UITile t) => OnTargetClicked((GridTile)t);

                    ActiveTarget active = new ActiveTarget
            {
                tile = tile,
                spawnTime = Time.time,
                duration = targetDuration - (difficulty * 0.2f), // scales down with higher difficulty
                scale = targetScale,
                colorType = type,
                isHit = false
            };

            // Set minimum duration cap
            if (active.duration < 1.8f) active.duration = 1.8f;

            _activeTargets.Add(active);
        }
    }

    private void OnTargetClicked(GridTile tile)
    {
        if (!CanAcceptInput()) return;

        // Find the active target info
        ActiveTarget target = _activeTargets.Find(t => t.tile == tile);
        if (target == null || target.isHit) return;

        target.isHit = true;
        _activeTargets.Remove(target); // Remove from active list immediately to make space for replenishment
        _successfulHits++;

        // Play shoot audio
        if (AudioManager.Instance != null && GameState.Instance != null)
        {
            AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.6f);
        }

        // Play shatter animation
        UISpriteSheetAnimator anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
        
        Sprite[] frames = GetFramesForType(target.colorType);
        if (frames != null && frames.Length > 0)
        {
            tile.Image.raycastTarget = false; // Prevent multi-clicks
            tile.transform.localScale = Vector3.one * targetScale; // Reset to full scale for slam shatter
            tile.Image.color = Color.white; // Reset color to full white
            
            anim.PlaySet(frames, 24f, false, () => {
                Destroy(tile.gameObject);
            });
        }
        else
        {
            Destroy(tile.gameObject);
        }

        if (_successfulHits >= _totalRequiredHits)
        {
            Solve();
        }
        else
        {
            // Replenish target immediately
            ReplenishTargets();
        }
    }

    private void ReplenishTargets()
    {
        // Maintain up to 3 active, non-hit targets on the board
        int targetActiveCount = Mathf.Min(3, _totalRequiredHits - _successfulHits);
        while (_activeTargets.Count < targetActiveCount)
        {
            SpawnTarget();
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || isSolved || isTimedOut) return;

        // Tick down timers for active targets
        for (int i = _activeTargets.Count - 1; i >= 0; i--)
        {
            ActiveTarget target = _activeTargets[i];
            if (target == null || target.tile == null || target.isHit) continue;

            float age = Time.time - target.spawnTime;
            float ratio = 1f - (age / target.duration);

            if (ratio <= 0f)
            {
                // Target expired! Remove and despawn gracefully
                _activeTargets.RemoveAt(i);
                StartCoroutine(DespawnTargetRoutine(target.tile));
                
                // Replenish target
                ReplenishTargets();
            }
            else
            {
                // Visual scale shrink feedback
                target.tile.transform.localScale = Vector3.one * (target.scale * Mathf.Lerp(0.3f, 1f, ratio));
                // Color flashing feedback (more red/warning as it decays)
                target.tile.Image.color = Color.Lerp(new Color(1f, 0.4f, 0.4f, 0.8f), Color.white, ratio);
            }
        }
    }

    private IEnumerator DespawnTargetRoutine(GridTile tile)
    {
        if (tile == null) yield break;

        tile.Image.raycastTarget = false;
        
        // Play a rapid shrink and fade out
        float t = 0f;
        float duration = 0.25f;
        Vector3 startScale = tile.transform.localScale;
        Color startColor = tile.Image.color;

        while (t < duration && tile != null)
        {
            t += Time.deltaTime;
            float ratio = t / duration;
            tile.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ratio);
            tile.Image.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), ratio);
            yield return null;
        }

        if (tile != null)
        {
            Destroy(tile.gameObject);
        }
    }

    private Sprite[] GetFramesForType(int type)
    {
        switch (type)
        {
            case 0: return blueShatterFrames;
            case 1: return greenShatterFrames;
            case 2: return purpleShatterFrames;
            case 3: return redShatterFrames;
            default: return null;
        }
    }

    protected override void ClearBoard()
    {
        StopAllCoroutines();

        foreach (var t in _activeTargets)
        {
            if (t != null && t.tile != null) Destroy(t.tile.gameObject);
        }
        _activeTargets.Clear();
        
        if (spawnArea != null)
        {
            for (int i = spawnArea.childCount - 1; i >= 0; i--)
            {
                Destroy(spawnArea.GetChild(i).gameObject);
            }
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut || _activeTargets.Count == 0) return;

        // AutoSolve click one of the currently active non-hit targets
        for (int i = 0; i < _activeTargets.Count; i++)
        {
            var target = _activeTargets[i];
            if (target != null && target.tile != null && !target.isHit)
            {
                OnTargetClicked(target.tile);
                return;
            }
        }
    }
}