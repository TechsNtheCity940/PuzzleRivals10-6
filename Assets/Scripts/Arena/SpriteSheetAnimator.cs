using UnityEngine;

/// <summary>
/// Reusable frame-by-frame sprite sheet animator.
/// Drives a SpriteRenderer through an ordered array of sliced sprites.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 12f;
    public bool loop = true;
    public bool playOnAwake = true;
    public bool destroyOnComplete = false;

    public System.Action OnComplete;

    private SpriteRenderer sr;
    private float timer;
    private int index;
    private bool playing;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (playOnAwake) Play();
    }

    public void SetFrames(Sprite[] f, float framesPerSecond, bool looping)
    {
        frames = f;
        fps = framesPerSecond;
        loop = looping;
    }

    public void Play()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (frames == null || frames.Length == 0) return;
        index = 0;
        timer = 0f;
        playing = true;
        sr.sprite = frames[0];
    }

    public void Stop() => playing = false;

    private void Update()
    {
        if (!playing || sr == null || frames == null || frames.Length == 0 || fps <= 0f) return;

        timer += Time.deltaTime;
        float frameDuration = 1f / fps;
        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            index++;
            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    index = frames.Length - 1;
                    sr.sprite = frames[index];
                    playing = false;
                    OnComplete?.Invoke();
                    if (destroyOnComplete) Destroy(gameObject);
                    return;
                }
            }
            sr.sprite = frames[index];
        }
    }
}
