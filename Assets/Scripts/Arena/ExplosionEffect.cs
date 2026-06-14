using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [Tooltip("Optional frame strip. When assigned, the explosion plays these frames instead of the procedural scale/fade.")]
    public Sprite[] frames;
    public float scale = 1f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Play(float duration = 0.5f)
    {
        if (frames != null && frames.Length > 0)
            StartCoroutine(AnimateFrames(duration));
        else
            StartCoroutine(Animate(duration));
    }

    private IEnumerator AnimateFrames(float duration)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * scale;
        float frameTime = duration / frames.Length;
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }
        Destroy(gameObject);
    }

    private IEnumerator Animate(float duration)
    {
        Vector3 startScale = Vector3.one * 0.3f;
        Vector3 endScale = Vector3.one * 1.6f;
        transform.localScale = startScale;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            
            if (elapsed > 0.2f && sr != null)
            {
                float alphaT = (elapsed - 0.2f) / (duration - 0.2f);
                Color c = sr.color;
                c.a = 1f - (alphaT * alphaT);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
