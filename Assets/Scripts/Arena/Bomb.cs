using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite litSprite;

    [Header("Animated Frames (optional)")]
    public Sprite[] idleFrames;
    public Sprite[] litFrames;
    public Sprite[] passFrames;
    public float idleFps = 10f;
    public float litFps = 14f;

    private bool passing;
    
    // Fuse properties
    private float maxFuseTime = 15f;
    private float fuseTimeRemaining = 15f;
    private bool isExploded = false;

    public SpriteRenderer spriteRenderer;
    public GameObject glowObject;

    private bool isLit;
    private Sprite[] activeFrames;
    private float activeFps;
    private float frameTimer;
    private int frameIndex;

    // Events
    public event System.Action<Player> OnBombExploded;
    public event System.Action OnBombPassed;
    
    // Current bomb holder reference
    public Player currentHolder;

    public void SetLit(bool lit)
    {
        isLit = lit;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (glowObject != null) glowObject.SetActive(lit);
        activeFrames = lit ? litFrames : idleFrames;
        activeFps = lit ? litFps : idleFps;
        frameIndex = 0;
        frameTimer = 0f;

        if (spriteRenderer)
        {
            if (activeFrames != null && activeFrames.Length > 0)
                spriteRenderer.sprite = activeFrames[0];
            else
                spriteRenderer.sprite = lit ? litSprite : idleSprite;
        }
    }

    private void Update()
    {
        // Keep the fuse running during the pass animation as well.
        if (isLit && currentHolder != null && !isExploded)
        {
            fuseTimeRemaining -= Time.deltaTime;
            if (fuseTimeRemaining <= 0f)
            {
                fuseTimeRemaining = 0f;
                Explode();
                return;
            }
        }

        if (passing) return;
        if (spriteRenderer == null || activeFrames == null || activeFrames.Length == 0 || activeFps <= 0f) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / activeFps;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % activeFrames.Length;
            spriteRenderer.sprite = activeFrames[frameIndex];
        }
    }

    public void ResetTimer(float speedMultiplier = 1.0f)
    {
        fuseTimeRemaining = maxFuseTime / Mathf.Max(0.01f, speedMultiplier);
        isExploded = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    public void AttachToPlayer(Player player)
    {
        if (player == null) return;
        currentHolder = player;
        // Bomb doesn't need to physically attach - it stays in the overlay
    }

    public void Explode()
    {
        if (isExploded) return;
        isExploded = true;
        isLit = false;
        activeFrames = null;
        if (glowObject != null) glowObject.SetActive(false);
        
        // Visual effect
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        OnBombExploded?.Invoke(currentHolder);
        
        // Find RankedMatchManager and notify
        var mgr = FindAnyObjectByType<RankedMatchManager>();
        if (mgr != null && currentHolder != null)
        {
            mgr.OnBombExploded(currentHolder);
        }
        
        // Keep this object alive so the next round can reuse the same bomb.
        // Destroying it here left later rounds without a bomb instance.
    }

    public void MoveTo(Vector3 targetPos, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPos, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        bool hasPass = passFrames != null && passFrames.Length > 0 && spriteRenderer != null;
        passing = hasPass;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Arc movement
            float arcHeight = Mathf.Sin(t * Mathf.PI) * 2f;
            transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * arcHeight;

            if (hasPass)
            {
                int fi = Mathf.Clamp(Mathf.FloorToInt(t * passFrames.Length), 0, passFrames.Length - 1);
                spriteRenderer.sprite = passFrames[fi];
            }
            yield return null;
        }
        transform.position = targetPos;
        passing = false;
        // Resume the current loop (lit/idle) cleanly.
        frameIndex = 0;
        frameTimer = 0f;
        if (spriteRenderer != null && activeFrames != null && activeFrames.Length > 0)
            spriteRenderer.sprite = activeFrames[0];
        
        OnBombPassed?.Invoke();
    }
    
    // Fuse properties for HUD
    public float CurrentFuseNormalized => (maxFuseTime > 0) ? Mathf.Clamp01(fuseTimeRemaining / maxFuseTime) : 0f;
    public float CurrentFuseRemaining => Mathf.Max(0f, fuseTimeRemaining);
}