using UnityEngine;
using System;
using System.Collections;

public class PipePulseTile : MonoBehaviour
{
    public int row, col;
    public int shape;
    public float currentRotation;
    public float targetRotation = 0;
    
    public SpriteRenderer pipeRenderer;
    public SpriteRenderer bgRenderer;

    [Header("Flow Animation")]
    public float flowFps = 12f;

    private Sprite[] flowFrames;
    private SpriteSheetAnimator flowAnim;
    private bool isFlowing;

    public event Action<int, int> OnClicked;

    public void Setup(int r, int c, int s, float startRot, Sprite pipeSprite)
    {
        Setup(r, c, s, startRot, pipeSprite, null);
    }

    public void Setup(int r, int c, int s, float startRot, Sprite pipeSprite, Sprite[] frames)
    {
        row = r;
        col = c;
        shape = s;
        currentRotation = startRot;
        transform.localRotation = Quaternion.Euler(0, 0, startRot);

        flowFrames = frames;
        isFlowing = false;

        if (pipeRenderer)
        {
            if (flowFrames != null && flowFrames.Length > 0)
            {
                flowAnim = pipeRenderer.GetComponent<SpriteSheetAnimator>();
                if (flowAnim == null) flowAnim = pipeRenderer.gameObject.AddComponent<SpriteSheetAnimator>();
                flowAnim.playOnAwake = false;
                flowAnim.destroyOnComplete = false;
                flowAnim.SetFrames(flowFrames, flowFps, true);
                flowAnim.Stop();
                pipeRenderer.sprite = flowFrames[0]; // frame 0 = empty/unsolved
            }
            else
            {
                pipeRenderer.sprite = pipeSprite;
            }
        }
    }

    public void Rotate()
    {
        currentRotation += 90;
        if (currentRotation >= 360) currentRotation -= 360;
        
        StopAllCoroutines();
        StartCoroutine(AnimateRotation(currentRotation));
    }

    private IEnumerator AnimateRotation(float target)
    {
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, target);
        float elapsed = 0;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            yield return null;
        }
        transform.localRotation = endRot;
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(row, col);
    }

    public void SetSolved(bool solved, Color solvedColor, Color defaultColor)
    {
        if (bgRenderer) bgRenderer.color = solved ? solvedColor : defaultColor;

        if (flowAnim == null || flowFrames == null || flowFrames.Length == 0) return;

        if (solved && !isFlowing)
        {
            isFlowing = true;
            flowAnim.SetFrames(flowFrames, flowFps, true);
            flowAnim.Play(); // loop the water flow
        }
        else if (!solved && isFlowing)
        {
            isFlowing = false;
            flowAnim.Stop();
            if (pipeRenderer) pipeRenderer.sprite = flowFrames[0]; // show empty frame 0
        }
    }
}
