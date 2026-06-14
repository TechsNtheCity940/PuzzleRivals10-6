using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Clickable circuit tile used by CircuitScramblePuzzle. Lives on
// CircuitScrambleTile.prefab whose root carries an Image (raycast target).
// Holds a grid coordinate, a generic "type" (visual variety) and the current
// rotation in degrees. Raises OnClicked when tapped.
[RequireComponent(typeof(Image))]
public class CircuitScrambleTile : MonoBehaviour, IPointerClickHandler
{
    public int type;
    public int x;
    public int y;
    public int rotation;

    public event Action<CircuitScrambleTile> OnClicked;

    private Image image;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
    }

    public Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    public void Init(int t, int gx, int gy, int rot)
    {
        type = t;
        x = gx;
        y = gy;
        rotation = ((rot % 360) + 360) % 360;
        SetType(type);
        ApplyRotation();
    }

    public void SetType(int t)
    {
        type = t;
        Image.color = TypeColor(t);
    }

    public void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
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

    private Color TypeColor(int t)
    {
        switch (((t % 4) + 4) % 4)
        {
            case 0: return new Color(0.30f, 0.90f, 1.00f);
            case 1: return new Color(1.00f, 0.60f, 0.40f);
            case 2: return new Color(0.70f, 1.00f, 0.50f);
            default: return new Color(0.90f, 0.60f, 1.00f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
