using UnityEngine;
using UnityEngine.UI;

public class Podium : MonoBehaviour
{
    public int playerIndex;
    public SpriteRenderer avatarSprite;
    public SpriteRenderer glowSprite;
    public Text nameLabel;
    public Text titleLabel;

    public void Setup(int index, string name, string title, Color color, Sprite sprite)
    {
        playerIndex = index;
        if (avatarSprite) avatarSprite.sprite = sprite;
        if (glowSprite) glowSprite.color = color;
        if (nameLabel)
        {
            nameLabel.text = name;
            nameLabel.color = color;
        }
        if (titleLabel) titleLabel.text = title;
    }

    public void SetEliminated(bool eliminated)
    {
        if (eliminated)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr) sr.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            if (avatarSprite) avatarSprite.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }
}
