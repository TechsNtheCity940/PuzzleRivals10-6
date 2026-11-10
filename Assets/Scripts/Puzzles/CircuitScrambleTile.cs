using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Clickable circuit tile used by CircuitScramblePuzzle.
public class CircuitScrambleTile : UITile
{
    public int type;
    public int rotation;

    private Image _overlayImage;

    public Image OverlayImage
    {
        get
        {
            if (_overlayImage == null)
            {
                Transform t = transform.Find("FlowOverlay");
                if (t != null) _overlayImage = t.GetComponent<Image>();
            }
            return _overlayImage;
        }
    }

    public override void Init(int gx, int gy)
    {
        base.Init(gx, gy);
    }

    public void Init(int t, int gx, int gy, int rot)
    {
        base.Init(gx, gy);
        type = t;
        rotation = ((rot % 360) + 360) % 360;
        ApplyRotation();
    }

    public override void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
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

    public new void Pulse(float scale = 1.2f, float duration = 0.15f)
    {
        base.Pulse(scale, duration);
    }
}