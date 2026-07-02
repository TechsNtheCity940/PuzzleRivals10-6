using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Clickable circuit tile used by CircuitScramblePuzzle.
[RequireComponent(typeof(Image))]
public class CircuitScrambleTile : MonoBehaviour, IPointerClickHandler
{
    public int type;
    public int x;
    public int y;
    public int rotation;

    public event Action<CircuitScrambleTile> OnClicked;

    private Image image;
    private Image overlayImage;

    public Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    public Image OverlayImage
    {
        get
        {
            if (overlayImage == null)
            {
                Transform t = transform.Find("FlowOverlay");
                if (t != null) overlayImage = t.GetComponent<Image>();
            }
            return overlayImage;
        }
    }

    public void Init(int t, int gx, int gy, int rot)
    {
        type = t;
        x = gx;
        y = gy;
        rotation = ((rot % 360) + 360) % 360;
        ApplyRotation();
    }

    public void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        Image.color = color;
    }

    public void SetRotation(int deg)
    {
        rotation = ((deg % 360) + 360) % 360;
        ApplyRotation();
    }

    public void Rotate()
    {
        rotation = (rotation + 90) % 360;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
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
