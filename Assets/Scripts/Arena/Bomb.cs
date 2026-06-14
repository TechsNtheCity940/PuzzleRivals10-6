using UnityEngine;
using System.Collections;

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

    public SpriteRenderer spriteRenderer;
    public GameObject glowObject;

    private bool isLit;
    private Sprite[] activeFrames;
    private float activeFps;
    private float frameTimer;
    private int frameIndex;

    public void SetLit(bool lit)
    {
        isLit = lit;
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
        if (glowObject) glowObject.SetActive(lit);
    }

    private void Update()
    {
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

    public void MoveTo(Vector3 targetPos, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPos, duration));
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
    }
}
