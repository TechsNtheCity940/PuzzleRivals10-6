using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// A single tile in the RuleBreaker puzzle grid.
// Lives on Assets/Prefabs/RuleBreakerTile.prefab. The prefab wires:
//   backgroundImage -> root UnityEngine.UI.Image
//   shapeImage      -> child "Shape" UnityEngine.UI.Image
//   numberText      -> child "Number" TMP text
// It also has a Button whose OnClick UnityEvent invokes OnClick() on this script,
// while this class also implements IPointerClickHandler; both paths are deduped
// per frame so a single tap raises OnClicked exactly once.
public class RuleBreakerTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile State")]
    public int x;
    public int y;
    public int number;
    public int colorID;
    public int shapeID;
    public bool isSelected;

    [Header("References")]
    public Image backgroundImage;
    public Image shapeImage;
    public TMPro.TMP_Text numberText;

    public event Action<RuleBreakerTile> OnClicked;

    private int _lastClickFrame = -1;

    public void Init(int xCoord, int yCoord, int tileNumber, int tileColorID, int tileShapeID,
        Color[] tileColors, Sprite[] shapeSprites)
    {
        x = xCoord;
        y = yCoord;
        number = tileNumber;
        colorID = tileColorID;
        shapeID = tileShapeID;
        isSelected = false;

        if (backgroundImage != null && tileColors != null && tileColors.Length > 0)
        {
            int ci = Mathf.Clamp(tileColorID, 0, tileColors.Length - 1);
            backgroundImage.color = tileColors[ci];
        }

        if (shapeImage != null)
        {
            if (shapeSprites != null && shapeSprites.Length > 0)
            {
                int si = Mathf.Clamp(tileShapeID, 0, shapeSprites.Length - 1);
                shapeImage.sprite = shapeSprites[si];
                shapeImage.enabled = shapeImage.sprite != null;
            }
            else
            {
                shapeImage.enabled = false;
            }
        }

        if (numberText != null)
        {
            numberText.text = tileNumber.ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        // Dim selected tiles so claimed cells read clearly against the rest.
        if (backgroundImage != null)
        {
            Color c = backgroundImage.color;
            c.a = selected ? 0.35f : 1f;
            backgroundImage.color = c;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RaiseClicked();
    }

    // Hooked up by the prefab's Button OnClick UnityEvent.
    public void OnClick()
    {
        RaiseClicked();
    }

    private void RaiseClicked()
    {
        // Button.onClick and IPointerClickHandler can both fire for one tap; only
        // raise once per frame.
        if (_lastClickFrame == Time.frameCount) return;
        _lastClickFrame = Time.frameCount;
        OnClicked?.Invoke(this);
    }
}
