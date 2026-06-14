using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A single selectable cosmetic in a grid (used by the Profile equip grids and
// the Store). Shows the cosmetic preview sprite, a label, and a state badge
// (price / OWNED / EQUIPPED / locked). Raises OnClicked with its data.
public class CosmeticTile : MonoBehaviour
{
    [Header("Refs")]
    public Button button;
    public Image preview;        // the cosmetic sprite
    public Image selectedOutline;// shown when equipped/selected
    public TMP_Text label;       // name/rarity
    public Image badgeBg;        // background of the state badge
    public TMP_Text badgeText;   // price or OWNED/EQUIPPED

    public CosmeticCategory Category { get; private set; }
    public CosmeticItem Item { get; private set; }

    public event Action<CosmeticTile> OnClicked;

    private void Awake()
    {
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Setup(CosmeticCategory category, CosmeticItem item)
    {
        Category = category;
        Item = item;
        if (preview != null && item != null)
        {
            preview.sprite = item.sprite;
            preview.color = Color.white;
            preview.preserveAspect = true;
        }
        if (label != null) label.text = item != null ? PrettyName(item) : "";
    }

    public void SetState(string text, Color badgeColor, bool selected)
    {
        if (badgeText != null) badgeText.text = text;
        if (badgeBg != null) badgeBg.color = badgeColor;
        if (selectedOutline != null) selectedOutline.gameObject.SetActive(selected);
    }

    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
    }

    private static string PrettyName(CosmeticItem item)
    {
        // "epic_02" -> "Epic 02"; emblem keys -> trimmed
        string k = item.key.Replace("EmblemS1", "").Replace("_", " ");
        if (k.Length > 0) k = char.ToUpper(k[0]) + k.Substring(1);
        return k.Trim();
    }
}
