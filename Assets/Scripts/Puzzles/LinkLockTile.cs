using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A single cell in the LinkLock (Flow-Free style) puzzle grid.
// Lives on Assets/Prefabs/LinkLockTile.prefab, which has a UnityEngine.UI.Image
// on its root used for the cell background / node / path tint.
public class LinkLockTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Cell State")]
    public int x;
    public int y;
    public int colorID = -1; // -1 = empty
    public bool isNode;
    public bool isPortal;
    public int portalId;
    public bool isBattery;
    public bool batteryCharged;

    public event Action<LinkLockTile> OnClicked;

    [Header("Art (optional, null = flat colour)")]
    public Sprite nodeSprite; // icon drawn on node endpoints, tinted by colour
    public Sprite portalSprite; // neon vortex icon, tinted purple/orange by portalId
    public Sprite batterySprite; // neon battery icon, tinted gold (dim/bright by charge)
    public Image wireImage;

    private Image _image;

    public void SetPortal(int id)
    {
        isPortal = true;
        portalId = id;
        RefreshVisual();
    }

    public void SetBattery()
    {
        isBattery = true;
        batteryCharged = false;
        RefreshVisual();
    }

    public void SetWire(int colorID, Sprite[] flowFrames, Sprite fallback, float fps, float rotation)
    {
        CacheImage();

        Image target = wireImage != null ? wireImage : _image;
        if (target != null)
        {
            target.gameObject.SetActive(true);
            target.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);

            var anim = target.GetComponent<UISpriteSheetAnimator>();
            if (flowFrames != null && flowFrames.Length > 0)
            {
                if (anim == null) anim = target.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.PlaySet(flowFrames, fps, true); // PlaySet forces colour to white...
            }
            else
            {
                if (anim != null) anim.Stop();
                target.sprite = fallback;
            }

            // ...so apply the per-colour tint AFTER PlaySet. Update() only swaps
            // the sprite, never the colour, so this tint persists every frame.
            target.color = ColorForId(colorID);
        }

        // If we used a child for wire, hide the root tint. Otherwise root is the wire.
        if (wireImage != null && _image != null)
        {
            if (isPortal || isBattery)
            {
                // Keep the portal/battery icon + colour visible beneath the wire.
                RefreshVisual();
            }
            else
            {
                _image.color = EmptyColor;
                _image.sprite = null;
            }
        }
    }

    // Small fixed palette mapped by colorID so the tile can self-colour from an
// index alone (the puzzle only passes a colorID into SetNode / SetPath).
    private static readonly Color[] Palette =
    {
        new Color(0.90f, 0.25f, 0.25f), // red
        new Color(0.30f, 0.70f, 0.35f), // green
        new Color(0.30f, 0.55f, 0.95f), // blue
        new Color(0.95f, 0.85f, 0.30f), // yellow
        new Color(0.85f, 0.40f, 0.90f), // magenta
        new Color(0.30f, 0.85f, 0.85f), // cyan
        new Color(0.95f, 0.60f, 0.25f), // orange
        new Color(0.70f, 0.70f, 0.75f), // grey
    };

    private static readonly Color EmptyColor = new Color(1f, 1f, 1f, 0.1f);

    private void Awake()
    {
        CacheImage();
    }

    private void CacheImage()
    {
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
    }

    private static Color ColorForId(int id)
    {
        if (Palette.Length == 0) return Color.white;
        int idx = ((id % Palette.Length) + Palette.Length) % Palette.Length;
        return Palette[idx];
    }

    public void Init(int xCoord, int yCoord)
    {
        CacheImage();
        x = xCoord;
        y = yCoord;
        Clear();
    }

    // Supplies the optional themed node icon (null keeps the flat-colour look).
    public void SetNodeSprite(Sprite sprite)
    {
        nodeSprite = sprite;
    }

    // Draw this cell as a filled node of the given colour.
    public void SetNode(int nodeColorID)
    {
        CacheImage();
        isNode = true;
        colorID = nodeColorID;
        RefreshVisual();
    }

    // Tint this cell as a path segment of the given colour.
    public void SetPath(int pathColorID)
    {
        CacheImage();
        colorID = pathColorID;
        if (_image != null)
        {
            Color c = ColorForId(pathColorID);
            c.a = 0.45f;
            _image.color = c;
            _image.sprite = null; // path segments stay plain tinted cells
        }
    }

    public void Clear()
    {
        CacheImage();
        isNode = false;
        colorID = -1;
        if (wireImage != null)
        {
            wireImage.gameObject.SetActive(false);
        }
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        CacheImage();
        if (_image == null) return;

        if (isNode)
        {
            Color c = ColorForId(colorID);
            c.a = 1f;
            _image.color = c;
            _image.sprite = nodeSprite;
        }
        else if (isPortal)
        {
            // Portal A: Purple, Portal B: Orange. White icon takes the tint.
            _image.color = (portalId == 0) ? new Color(0.7f, 0.1f, 1.0f, 1.0f) : new Color(1.0f, 0.5f, 0.0f, 1.0f);
            _image.sprite = portalSprite;
        }
        else if (isBattery)
        {
            // Uncharged Battery: Dark gold-yellow, Charged: Glowing bright gold-yellow.
            _image.color = batteryCharged ? new Color(1.0f, 0.85f, 0.1f, 1.0f) : new Color(0.45f, 0.4f, 0.15f, 1.0f);
            _image.sprite = batterySprite;
        }
        else
        {
            _image.color = EmptyColor;
            _image.sprite = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
