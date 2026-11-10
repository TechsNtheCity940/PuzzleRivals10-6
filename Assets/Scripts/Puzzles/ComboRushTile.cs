using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Clickable UI tile used by the ComboRush overlay. Holds a grid coordinate and
// a colour id; raises OnClicked when tapped.
public class ComboRushTile : UITile
{
    public int colorID;
    public bool isSelected;
    public bool isBomb;
    public bool isGeode;

    private Coroutine _pulseRoutine;

    public override void Init(int gx, int gy)
    {
        base.Init(gx, gy);
    }

    public void Init(int gx, int gy, int cid)
    {
        base.Init(gx, gy);
        colorID = cid;
        isBomb = false;
        isGeode = false;
        SetSelected(false);
    }

    public void SetBomb()
    {
        isBomb = true;
        isGeode = false;
        transform.localScale = Vector3.one * 1.15f;
    }

    public void SetGeode()
    {
        isGeode = true;
        isBomb = false;
        colorID = -1;
        Image.color = new Color(0.85f, 0.85f, 0.85f, 1f);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
        Color c = Image.color;
        float boost = selected ? 1.25f : 1f;
        Image.color = new Color(
            Mathf.Clamp01(c.r * boost),
            Mathf.Clamp01(c.g * boost),
            Mathf.Clamp01(c.b * boost),
            c.a);
    }

    public override void MoveTo(Vector2 anchoredPos, float duration = 0.3f)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(anchoredPos, duration));
    }

    private IEnumerator MoveRoutine(Vector2 target, float duration)
    {
        RectTransform rt = (RectTransform)transform;
        Vector2 start = rt.anchoredPosition;
        float elapsed = 0;
        Vector3 startScale = Vector3.one;
        Vector3 peakScale = Vector3.one * 1.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t * (3f - 2f * t);
            rt.anchoredPosition = Vector2.Lerp(start, target, easeT);

            if (t < 0.5f) transform.localScale = Vector3.Lerp(startScale, peakScale, t * 2f);
            else transform.localScale = Vector3.Lerp(peakScale, startScale, (t - 0.5f) * 2f);

            yield return null;
        }
        rt.anchoredPosition = target;
        transform.localScale = Vector3.one;
        _moveRoutine = null;
    }

    public void PopAndDestroy(float duration = 0.2f)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        StartCoroutine(PopAndDestroyRoutine(duration));
    }

    private IEnumerator PopAndDestroyRoutine(float duration)
    {
        float elapsed = 0;
        Vector3 startScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }

    
}