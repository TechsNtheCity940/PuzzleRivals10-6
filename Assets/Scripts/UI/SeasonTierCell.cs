using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One tier column in the Season Pass track: level label, free + premium reward
// preview images, and claim/lock states.
public class SeasonTierCell : MonoBehaviour
{
    public TMP_Text levelText;
    public Image freeReward;
    public Image premiumReward;
    public GameObject freeClaimedBadge;     // shown when this tier's level is reached
    public GameObject premiumLockedOverlay; // shown when premium pass not owned
    public GameObject currentMarker;        // highlights the player's current level

    public void Setup(int level, Sprite freeSprite, Sprite premiumSprite,
        bool reached, bool premiumOwned, bool isCurrent)
    {
        if (levelText != null) levelText.text = "LV " + level;
        if (freeReward != null)
        {
            freeReward.sprite = freeSprite;
            freeReward.color = freeSprite != null ? Color.white : new Color(1, 1, 1, 0.15f);
            freeReward.preserveAspect = true;
        }
        if (premiumReward != null)
        {
            premiumReward.sprite = premiumSprite;
            premiumReward.color = premiumSprite != null ? Color.white : new Color(1, 1, 1, 0.15f);
            premiumReward.preserveAspect = true;
        }
        if (freeClaimedBadge != null) freeClaimedBadge.SetActive(reached);
        if (premiumLockedOverlay != null) premiumLockedOverlay.SetActive(!premiumOwned);
        if (currentMarker != null) currentMarker.SetActive(isCurrent);
    }
}
