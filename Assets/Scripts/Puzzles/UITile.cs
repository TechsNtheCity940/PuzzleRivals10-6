using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Base class for all UI grid tiles. Handles common functionality:
/// - Grid coordinates (x, y)
/// - Click handling via IPointerClickHandler
/// - Image/color/sprite management
/// - Animation helpers (MoveTo, Pulse, SetRotation)
/// Derived classes add puzzle-specific state (type, colorID, rotation, etc.)
/// </summary>
[RequireComponent(typeof(Image))]
public abstract class UITile : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;

    public event Action<UITile> OnClicked;

    protected void FireOnClicked() => OnClicked?.Invoke(this);

    private Image _image;
    protected Coroutine _moveRoutine;

    public Image Image
    {
        get
        {
            if (_image == null) _image = GetComponent<Image>();
            return _image;
        }
    }

    public virtual void Init(int gx, int gy)
    {
        x = gx;
        y = gy;
    }

    public virtual void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        Image.color = color;
    }

    private Coroutine _rotationRoutine;
    
    public void SetRotation(float zDegrees, float duration = 0.2f)
    {
        if (_rotationRoutine != null) StopCoroutine(_rotationRoutine);
        _rotationRoutine = StartCoroutine(RotationRoutine(zDegrees, duration));
    }
    
    private IEnumerator RotationRoutine(float targetDegrees, float duration)
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, targetDegrees);
        
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            yield return null;
        }
        
        transform.localRotation = endRotation;
        _rotationRoutine = null;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        FireOnClicked();
    }

    public virtual void MoveTo(Vector2 target, float duration = 0.2f)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(target, duration));
    }

    private IEnumerator MoveRoutine(Vector2 target, float duration)
    {
        RectTransform rt = (RectTransform)transform;
        Vector2 start = rt.anchoredPosition;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        rt.anchoredPosition = target;
        _moveRoutine = null;
    }

    public void Pulse(float scale = 1.2f, float duration = 0.15f)
    {
        StartCoroutine(PulseRoutine(scale, duration));
    }

    private IEnumerator PulseRoutine(float scale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 peakScale = startScale * scale;
        float half = duration * 0.5f;

        float elapsed = 0;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, peakScale, elapsed / half);
            yield return null;
        }
        elapsed = 0;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(peakScale, startScale, elapsed / half);
            yield return null;
        }
        transform.localScale = startScale;
    }
}