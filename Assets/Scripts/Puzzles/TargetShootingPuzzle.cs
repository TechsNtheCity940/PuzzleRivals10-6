using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TargetShootingPuzzle : BasePuzzle
{
    [Header("Target Shooting")]
    public GameObject targetPrefab; // GridTile based
    public RectTransform spawnArea;
    public int minTargets = 3;
    public int maxTargets = 8;
    public float targetScale = 1.0f;

    [Header("Art")]
    public Sprite[] blueShatterFrames;
    public Sprite[] greenShatterFrames;
    public Sprite[] purpleShatterFrames;
    public Sprite[] redShatterFrames;

    private List<GridTile> _activeTargets = new List<GridTile>();
    private int _targetsRemaining;

    protected override void BuildBoard()
    {
        ClearBoard();
        _targetsRemaining = rng.Next(minTargets + difficulty, maxTargets + difficulty);
        
        for (int i = 0; i < _targetsRemaining; i++)
        {
            SpawnTarget();
        }
    }

    private void SpawnTarget()
    {
        if (targetPrefab == null || spawnArea == null) return;

        GameObject go = Instantiate(targetPrefab, spawnArea);
        RectTransform rt = (RectTransform)go.transform;
        
        // Ensure we have a valid rect. If it's zero, use the massive puzzle size as fallback.
        float width = spawnArea.rect.width > 100 ? spawnArea.rect.width : 2200f;
        float height = spawnArea.rect.height > 100 ? spawnArea.rect.height : 1300f;
        
        // Random position within calculated area, avoiding the very edges
        float x = (float)(rng.NextDouble() - 0.5) * (width * 0.9f);
        float y = (float)(rng.NextDouble() - 0.5) * (height * 0.8f);
        rt.anchoredPosition = new Vector2(x, y);
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

            tile.OnClicked += OnTargetClicked;
            _activeTargets.Add(tile);
        }
    }

    private void OnTargetClicked(GridTile tile)
    {
        if (!CanAcceptInput()) return;

        _activeTargets.Remove(tile);
        _targetsRemaining--;

        // Play shatter animation
        UISpriteSheetAnimator anim = tile.GetComponent<UISpriteSheetAnimator>();
        if (anim == null) anim = tile.gameObject.AddComponent<UISpriteSheetAnimator>();
        
        Sprite[] frames = GetFramesForType(tile.type);
        if (frames != null)
        {
            tile.Image.raycastTarget = false; // Prevent multi-clicks
            anim.PlaySet(frames, 20f, false, () => {
                Destroy(tile.gameObject);
            });
        }
        else
        {
            Destroy(tile.gameObject);
        }

        if (_targetsRemaining <= 0)
        {
            Solve();
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

    private void ClearBoard()
    {
        foreach (var t in _activeTargets)
        {
            if (t != null) Destroy(t.gameObject);
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
        if (_activeTargets.Count > 0)
        {
            OnTargetClicked(_activeTargets[0]);
        }
    }
}