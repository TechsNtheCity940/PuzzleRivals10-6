using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Score Attack puzzle - destroy targets for points within time limit.
/// Simple click-based puzzle where player must reach a score threshold.
/// </summary>
public class ScoreAttack : BasePuzzle
{
    [Header("Score Attack")]
    public Transform spawnArea;
    public GameObject targetPrefab;
    public int baseTargetScore = 1000;
    public int scorePerHit = 100;
    public float targetSpawnInterval = 1f;
    
    [Header("UI")]
    public TMP_Text scoreText;
    public Slider scoreSlider;
    public Image fillImage;
    
    [Header("Art")]
    public Sprite floorSprite;
    public Sprite scoreTargetSprite;
    public Sprite[] shatterRedFrames;
    public Sprite[] shatterBlueFrames;
    public Sprite[] shatterGreenFrames;
    public Sprite[] shatterPurpleFrames;
    public float shatterFps = 18f;
    
    private int currentScore = 0;
    private int targetScore;
    private List<GameObject> activeTargets = new List<GameObject>();
    private bool spawning = false;
    
    protected override void BuildBoard()
    {
        currentScore = 0;
        targetScore = baseTargetScore + difficulty * 200;
        ClearTargets();
        UpdateUI();
    }
    
    public override void StartPuzzle()
    {
        base.StartPuzzle();
        if (!spawning)
            StartCoroutine(SpawnTargets());
    }
    
    public override void StopPuzzle()
    {
        base.StopPuzzle();
        spawning = false;
    }
    
    private IEnumerator SpawnTargets()
    {
        spawning = true;
        while (isRunning && !isSolved && !isTimedOut)
        {
            SpawnTarget();
            yield return new WaitForSeconds(targetSpawnInterval);
        }
        spawning = false;
    }
    
    private void SpawnTarget()
    {
        if (targetPrefab == null || spawnArea == null) return;
        
        GameObject target = Instantiate(targetPrefab, spawnArea);
        target.name = "Target";
        
        // Position randomly within spawn area
        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(
                Random.Range(-200f, 200f),
                Random.Range(-150f, 150f)
            );
        }
        
        Button btn = target.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnTargetHit(target));
        }
        
        // Set target sprite
        Image img = target.GetComponent<Image>();
        if (img != null && scoreTargetSprite != null)
        {
            img.sprite = scoreTargetSprite;
        }
        
        activeTargets.Add(target);
    }
    
    private void OnTargetHit(GameObject target)
    {
        if (!CanAcceptInput()) return;
        
        currentScore += scorePerHit;
        UpdateUI();
        
        if (currentScore >= targetScore)
        {
            Solve();
        }
        
        // Visual effect
        PlayShatterEffect(target);
        activeTargets.Remove(target);
    }
    
    private void PlayShatterEffect(GameObject target)
    {
        Image img = target.GetComponent<Image>();
        if (img == null) return;

        // Select random shatter effect
        int rand = Random.Range(0, 4);
        Sprite[] frames = null;
        switch (rand)
        {
            case 0: frames = shatterRedFrames; break;
            case 1: frames = shatterBlueFrames; break;
            case 2: frames = shatterGreenFrames; break;
            case 3: frames = shatterPurpleFrames; break;
        }
        if (frames != null && frames.Length > 0)
        {
            var anim = target.GetComponent<UISpriteSheetAnimator>();
            if (anim == null) anim = target.AddComponent<UISpriteSheetAnimator>();
            anim.PlaySet(frames, shatterFps, false, () => {
                Object.Destroy(target);
            });
        }
        else
        {
            Object.Destroy(target, 0.5f);
        }
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{currentScore} / {targetScore}";
        
        if (scoreSlider != null)
        {
            scoreSlider.maxValue = targetScore;
            scoreSlider.value = currentScore;
        }
        
        // Flash green when approaching target
        if (fillImage != null && currentScore > targetScore * 0.75f)
        {
            fillImage.color = Color.Lerp(Color.white, Color.green, Mathf.PingPong(Time.time * 5f, 1f));
        }
    }
    
    public override void AutoSolveChunk()
    {
        if (activeTargets.Count > 0)
        {
            OnTargetHit(activeTargets[0]);
        }
    }
    
    private void ClearTargets()
    {
        foreach (var t in activeTargets)
        {
            if (t != null) Destroy(t);
        }
        activeTargets.Clear();
    }
    
    protected override void ClearBoard()
    {
        spawning = false;
        ClearTargets();
    }
    
    protected override void OnDestroy()
    {
        spawning = false;
        ClearTargets();
        base.OnDestroy();
    }
}