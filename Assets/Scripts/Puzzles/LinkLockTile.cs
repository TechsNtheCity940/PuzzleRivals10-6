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

    public event Action<LinkLockTile> OnClicked;

    [Header("Art (optional, null = flat colour)")]
    public Sprite nodeSprite; // icon drawn on node endpoints, tinted by colour
    public Image wireImage;

    private Image _image;

    public void SetWire(int colorID, Sprite wireSprite, float rotation)
    {
        CacheImage();
        
        Image target = wireImage != null ? wireImage : _image;
        if (target != null)
        {
            target.gameObject.SetActive(true);
            target.sprite = wireSprite;
            target.color = ColorForId(colorID);
            target.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        // If we used a child for wire, hide the root tint. Otherwise root is the wire.
        if (wireImage != null && _image != null)
        {
            _image.color = EmptyColor;
            _image.sprite = null;
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
        if (_image != null)
        {
            Color c = ColorForId(nodeColorID);
            c.a = 1f;
            _image.color = c; // keep colour: it identifies the pair
            _image.sprite = nodeSprite; // tinted node icon when art exists
        }
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
        if (_image != null)
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
