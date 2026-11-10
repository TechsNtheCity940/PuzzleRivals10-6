using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A single cell in the LinkLock (Flow-Free style) puzzle grid.
// Lives on Assets/Prefabs/LinkLockTile.prefab, which has a UnityEngine.UI.Image
// on its root used for the cell background / node / path tint.
public class LinkLockTile : UITile
{
    [Header("Cell State")]
    public int colorID = -1; // -1 = empty
    public bool isNode;
    public bool isPortal;
    public int portalId;
    public bool isBattery;
    public bool batteryCharged;

    [Header("Art (optional, null = flat colour)")]
    public Sprite nodeSprite;
    public Sprite portalSprite;
    public Sprite batterySprite;
    public Image wireImage;

    private static readonly Color[] Palette =
    {
        new Color(0.90f, 0.25f, 0.25f),
        new Color(0.30f, 0.70f, 0.35f),
        new Color(0.30f, 0.55f, 0.95f),
        new Color(0.95f, 0.85f, 0.30f),
        new Color(0.85f, 0.40f, 0.90f),
        new Color(0.30f, 0.85f, 0.85f),
        new Color(0.95f, 0.60f, 0.25f),
        new Color(0.70f, 0.70f, 0.75f),
    };

    private static readonly Color EmptyColor = new Color(1f, 1f, 1f, 0.1f);

    private static Color ColorForId(int id)
    {
        if (Palette.Length == 0) return Color.white;
        int idx = ((id % Palette.Length) + Palette.Length) % Palette.Length;
        return Palette[idx];
    }

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
        Image target = wireImage != null ? wireImage : Image;
        if (target != null)
        {
            target.gameObject.SetActive(true);
            target.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);

            var anim = target.GetComponent<UISpriteSheetAnimator>();
            if (flowFrames != null && flowFrames.Length > 0)
            {
                if (anim == null) anim = target.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.PlaySet(flowFrames, fps, true);
            }
            else
            {
                if (anim != null) anim.Stop();
                target.sprite = fallback;
            }

            target.color = ColorForId(colorID);
        }

        if (wireImage != null && Image != null)
        {
            if (isPortal || isBattery)
                RefreshVisual();
            else
            {
                Image.color = EmptyColor;
                Image.sprite = null;
            }
        }
    }

    public void SetNodeSprite(Sprite sprite)
    {
        nodeSprite = sprite;
        isNode = true;
        RefreshVisual();
    }

    public void SetNode(int nodeColorID)
    {
        isNode = true;
        colorID = nodeColorID;
        RefreshVisual();
    }

    public void Clear()
    {
        isNode = false;
        colorID = -1;
        if (wireImage != null)
            wireImage.gameObject.SetActive(false);
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (Image == null) return;

        if (isNode)
        {
            Color c = ColorForId(colorID);
            c.a = 1f;
            Image.color = c;
            Image.sprite = nodeSprite;
        }
        else if (isPortal)
        {
            Image.color = (portalId == 0) ? new Color(0.7f, 0.1f, 1.0f, 1.0f) : new Color(1.0f, 0.5f, 0.0f, 1.0f);
            Image.sprite = portalSprite;
        }
        else if (isBattery)
        {
            Image.color = batteryCharged ? new Color(1.0f, 0.85f, 0.1f, 1.0f) : new Color(0.45f, 0.4f, 0.15f, 1.0f);
            Image.sprite = batterySprite;
        }
        else
        {
            Image.color = EmptyColor;
            Image.sprite = null;
        }
    }

    public override void Init(int gx, int gy)
    {
        base.Init(gx, gy);
        Clear();
    }
}
