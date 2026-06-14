using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Shared UI grid tile used by the PipePulse, CollapseGrid and MirrorMaze
// overlays (all reference GridTile.prefab). Holds a grid coordinate and a
// generic integer "type" the owning puzzle interprets (pipe shape, brick
// state, mirror orientation, etc.). Raises OnClicked when tapped.
[RequireComponent(typeof(Image))]
public class GridTile : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;
    public int type;

    public event Action<GridTile> OnClicked;

    private UnityEngine.UI.Image _image;
    private UnityEngine.UI.Image _overlayImage;
    private Coroutine _moveRoutine;

    public UnityEngine.UI.Image Image
    {
        get
        {
            if (_image == null) _image = GetComponent<UnityEngine.UI.Image>();
            return _image;
        }
    }

    public UnityEngine.UI.Image OverlayImage
    {
        get
        {
            if (_overlayImage == null)
            {
                Transform t = transform.Find("PipeOverlay");
                if (t == null) t = transform.Find("Overlay");
                if (t != null) _overlayImage = t.GetComponent<UnityEngine.UI.Image>();
            }
            return _overlayImage;
        }
    }

    public void Init(int gx, int gy, int t)
    {
        x = gx;
        y = gy;
        type = t;
    }

    public void SetSprite(Sprite sprite)
    {
        if (OverlayImage != null) OverlayImage.sprite = sprite;
        else Image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        if (OverlayImage != null) OverlayImage.color = color;
        else Image.color = color;
    }

    public void SetRotation(float zDegrees)
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, zDegrees);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }

    public void MoveTo(Vector2 target, float duration = 0.2f)
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
