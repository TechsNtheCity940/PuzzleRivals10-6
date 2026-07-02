using System;
using UnityEngine;
using UnityEngine.UI;

// Cycles an array of sprite frames on a UI Image. Used by the CollapseGrid
// bricks to play the cyan-shimmer / pink-pulse / crack / brace animations, and
// to play one-shot effects (dust removal, collapse burst). Pure runtime, no
// editor dependency. Robust to null/empty frame arrays.
[RequireComponent(typeof(Image))]
public class UISpriteSheetAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 10f;
    public bool loop = true;
    public bool playOnEnable = true;

    private Image image;
    private float timer;
    private int index;
    private bool playing;
    private Action onComplete;

    private Image TargetImage
    {
        get { if (image == null) image = GetComponent<Image>(); return image; }
    }

    private void OnEnable()
    {
        if (playOnEnable && frames != null && frames.Length > 0) Play();
    }

    // Begin (or restart) looping/one-shot playback of the current frames.
    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        index = 0;
        timer = 0f;
        playing = true;
        TargetImage.sprite = frames[0];
        TargetImage.color = Color.white;
    }

    // Configure and immediately play a frame set. onDone fires when a non-looping
    // animation finishes (ignored when loop = true).
    public void PlaySet(Sprite[] newFrames, float newFps, bool newLoop, Action onDone = null)
    {
        onComplete = onDone;
        if (newFrames == null || newFrames.Length == 0)
        {
            onComplete?.Invoke();
            onComplete = null;
            return;
        }

        frames = newFrames;
        fps = newFps;
        loop = newLoop;
        Play();
    }

    public void Stop() => playing = false;

    // True while a frame set is actively cycling. Lets callers gate animations
    // (start only when needed, avoid restarting an already-running loop).
    public bool IsPlaying => playing;

    private void Update()
    {
        if (!playing || frames == null || frames.Length == 0 || fps <= 0f) return;

        timer += Time.deltaTime;
        float frameDur = 1f / fps;
        while (timer >= frameDur && playing)
        {
            timer -= frameDur;
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
                    playing = false;
                    TargetImage.sprite = frames[index];
                    
                    var callback = onComplete;
                    onComplete = null;
                    callback?.Invoke();
                    return;
                }
            }
            TargetImage.sprite = frames[index];
        }
    }
}
