using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// A single live player card shown in the FFA match HUD. Displays the player's
// equipped cosmetics (full-color background card, frame, nameplate, rank emblem)
// plus live status (bomb holder highlight, puzzles solved, elimination). Pure
// view: the FFAHudController feeds it a Player + a resolved CosmeticLoadout.
public class PlayerCard : MonoBehaviour
{
    [Header("Cosmetic Layers (back -> front)")]
    public Image backgroundCard;   // full-card background art
    public Image avatar;           // player avatar portrait
    public Image frame;            // ornate border art (transparent center)
    public Image emblem;           // rank emblem badge
    public Image nameplate;        // ornate bar behind the name

    [Header("Text / State")]
    public TMP_Text nameText;      // player name
    public TMP_Text solvedText;    // "SOLVED: n"
    public Image bombIcon;         // shown while this player holds the bomb
    public GameObject holderHighlight; // glow when active holder
    public GameObject eliminatedOverlay; // dim + "OUT" when eliminated

    [Header("Explosion FX")]
    public Image cardExplosionImage; // overlay on player card

    private Player boundPlayer;
    private Coroutine bombPulseRoutine;

    public Player BoundPlayer => boundPlayer;

    private void Start()
    {
        // Dynamically configure explosion image on card if missing
        if (cardExplosionImage == null)
        {
            GameObject expGo = new GameObject("CardExplosion", typeof(RectTransform), typeof(Image));
            expGo.transform.SetParent(transform, false);
            RectTransform rt = expGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            cardExplosionImage = expGo.GetComponent<Image>();
            cardExplosionImage.preserveAspect = true;
            cardExplosionImage.gameObject.SetActive(false);
        }
    }

    public void Bind(Player player, CosmeticLoadout loadout)
    {
        boundPlayer = player;
        ApplyLoadout(loadout);

        if (player != null && nameText != null)
        {
            nameText.text = player.isLocal ? player.playerName + " (YOU)" : player.playerName;
            nameText.color = Color.white;
        }
        Refresh();
    }

    // Assigns full-color cosmetic sprites. Null sprites leave the prefab default
    // so the card never blanks out.
    public void ApplyLoadout(CosmeticLoadout lo)
    {
        SetSprite(backgroundCard, lo.background);
        SetSprite(avatar, lo.avatar);
        SetSprite(frame, lo.frame);
        SetSprite(nameplate, lo.nameplate);
        SetSprite(emblem, lo.emblem);
    }

    private void SetSprite(Image img, Sprite sprite)
    {
        if (img == null || sprite == null) return;
        img.sprite = sprite;
        img.color = Color.white; // full-color art: no tint
        img.enabled = true;
    }

    public void Refresh()
    {
        if (boundPlayer == null) return;

        if (solvedText != null) solvedText.text = "SOLVED: " + boundPlayer.puzzlesSolved;
        // bombIcon visibility is driven by SetBombActive (animated overlay).
        if (holderHighlight != null) holderHighlight.SetActive(boundPlayer.hasBomb && !boundPlayer.isEliminated);
        if (eliminatedOverlay != null) eliminatedOverlay.SetActive(boundPlayer.isEliminated);
    }

    // Shows/hides an animated bomb sprite-sheet or lit bomb image over this card while its player
    // is the active bomb holder.
    public void SetBombActive(bool active, Sprite bombLitSprite)
    {
        if (bombIcon == null) return;

        bool show = active && boundPlayer != null && !boundPlayer.isEliminated;
        if (show)
        {
            RectTransform rt = bombIcon.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 28f); // hover just above the card top
            rt.sizeDelta = new Vector2(95f, 90f);
            bombIcon.preserveAspect = true;
            bombIcon.color = Color.white;

            if (bombLitSprite != null)
            {
                bombIcon.sprite = bombLitSprite;
            }

            // Hide the static "B" label child if present.
            foreach (Transform c in bombIcon.transform) c.gameObject.SetActive(false);

            bombIcon.gameObject.SetActive(true);

            // Lit bomb procedural pulse
            if (bombPulseRoutine != null) StopCoroutine(bombPulseRoutine);
            bombPulseRoutine = StartCoroutine(BombLitPulseRoutine());
        }
        else
        {
            if (bombPulseRoutine != null)
            {
                StopCoroutine(bombPulseRoutine);
                bombPulseRoutine = null;
            }
            bombIcon.gameObject.SetActive(false);
        }
    }

    private IEnumerator BombLitPulseRoutine()
    {
        Vector3 baseScale = new Vector3(1.1f, 1.1f, 1f);
        while (true)
        {
            // Hot core pulse and jitter animation to look highly unstable & animated
            float speed = 12f;
            float scaleOffset = 0.12f * Mathf.Sin(Time.time * speed);
            bombIcon.rectTransform.localScale = baseScale + new Vector3(scaleOffset, scaleOffset, 0f);
            
            // Random tiny vibration/jitter on X & Y
            float jitterX = Random.Range(-1.5f, 1.5f);
            float jitterY = Random.Range(-1.5f, 1.5f);
            bombIcon.rectTransform.anchoredPosition = new Vector2(jitterX, 28f + jitterY);
            
            yield return null;
        }
    }

    public void ExplodeCard(Sprite explosionSprite)
    {
        if (cardExplosionImage == null) return;
        StartCoroutine(ExplodeCardRoutine(explosionSprite));
    }

    private IEnumerator ExplodeCardRoutine(Sprite explosionSprite)
    {
        cardExplosionImage.gameObject.SetActive(true);
        if (explosionSprite != null)
        {
            cardExplosionImage.sprite = explosionSprite;
        }
        cardExplosionImage.color = Color.white;
        cardExplosionImage.rectTransform.localScale = Vector3.one * 0.1f;

        float elapsed = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cardExplosionImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * 2.2f, t);
            cardExplosionImage.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }

        cardExplosionImage.gameObject.SetActive(false);
    }
}
