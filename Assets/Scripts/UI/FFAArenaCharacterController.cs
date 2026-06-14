using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FFAArenaCharacterController : MonoBehaviour
{
    public static FFAArenaCharacterController Instance;

    [Header("Character Images (0=Magenta, 1=Cyan, 2=Lime, 3=Orange)")]
    public Image[] characterImages;

    [Header("Hand Held Items")]
    public Image[] handItemImages;
    public Sprite puzzlePieceSprite; // This will be puzzle_icon_neon
    public Sprite bombLitSprite;    // This will be live_bomb_neon
    public Vector2[] handOffsets = new Vector2[] {
        new Vector2(100f, 240f),  // Magenta (Right)
        new Vector2(-100f, 240f), // Cyan (Left)
        new Vector2(100f, 220f),  // Lime (Right)
        new Vector2(-100f, 220f)  // Orange (Left)
    };

    [Header("FX Images")]
    public Image feetBombIcon; // Keep for compatibility but we will hide it
    public Image exitPortalImage;
    public Image explosionImage;

    [Header("Settings")]
    public float bobSpeed = 2f;
    public float bobAmount = 15f;
    public float scaleBreathSpeed = 1.5f;
    public float scaleBreathAmount = 0.05f;
    public float itemFloatSpeed = 3f;
    public float itemFloatAmount = 10f;

    private Vector2[] basePositions;
    private bool[] isEliminated = new bool[4];
    private float[] individualOffset;
    private int currentBombPlayerId = -1;

    private void Awake()
    {
        Instance = this;
        individualOffset = new float[4];
        for (int i = 0; i < 4; i++)
        {
            individualOffset[i] = Random.Range(0f, 100f);
        }
    }

    private void Start()
    {
        // Cache initial positions
        if (characterImages != null && characterImages.Length >= 4)
        {
            basePositions = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                if (characterImages[i] != null)
                {
                    basePositions[i] = characterImages[i].rectTransform.anchoredPosition;
                    characterImages[i].gameObject.SetActive(true);
                }
            }
        }

        // Setup hand items
        RefreshHandItems();

        // Initialize FX states
        if (feetBombIcon != null) feetBombIcon.gameObject.SetActive(false);
        if (exitPortalImage != null) exitPortalImage.gameObject.SetActive(false);
        if (explosionImage != null) explosionImage.gameObject.SetActive(false);
    }

    private void RefreshHandItems()
    {
        if (handItemImages == null || handItemImages.Length < 4) return;

        for (int i = 0; i < 4; i++)
        {
            if (handItemImages[i] == null) continue;
            
            bool hasBomb = (i == currentBombPlayerId);
            handItemImages[i].sprite = hasBomb ? bombLitSprite : puzzlePieceSprite;
            
            // Set color based on platform character color if puzzle piece
            if (!hasBomb)
            {
                if (i == 0) handItemImages[i].color = new Color(1f, 0f, 1f, 0.9f); // Magenta
                else if (i == 1) handItemImages[i].color = new Color(0f, 1f, 1f, 0.9f); // Cyan
                else if (i == 2) handItemImages[i].color = new Color(0.5f, 1f, 0f, 0.9f); // Lime
                else if (i == 3) handItemImages[i].color = new Color(1f, 0.5f, 0f, 0.9f); // Orange
            }
            else
            {
                handItemImages[i].color = Color.white;
            }

            handItemImages[i].gameObject.SetActive(!isEliminated[i]);
            
            // Adjust size for bomb vs puzzle piece
            handItemImages[i].rectTransform.sizeDelta = hasBomb ? new Vector2(80f, 80f) : new Vector2(60f, 60f);
        }
    }

    private void Update()
    {
        if (characterImages == null) return;

        // Idle floating/breathing animation for alive characters
        for (int i = 0; i < characterImages.Length; i++)
        {
            if (characterImages[i] == null || isEliminated[i]) continue;

            float time = Time.time * bobSpeed + individualOffset[i];
            float yOffset = Mathf.Sin(time) * bobAmount;
            characterImages[i].rectTransform.anchoredPosition = basePositions[i] + new Vector2(0f, yOffset);

            float scaleTime = Time.time * scaleBreathSpeed + individualOffset[i];
            float scaleOffset = 1f + Mathf.Sin(scaleTime) * scaleBreathAmount;
            characterImages[i].rectTransform.localScale = new Vector3(scaleOffset, scaleOffset, 1f);

            // Hand item animation
            if (handItemImages != null && i < handItemImages.Length && handItemImages[i] != null)
            {
                float itemTime = Time.time * itemFloatSpeed + individualOffset[i];
                float itemY = handOffsets[i].y + Mathf.Sin(itemTime) * itemFloatAmount;
                handItemImages[i].rectTransform.anchoredPosition = new Vector2(handOffsets[i].x, itemY);
                
                // Rotation
                handItemImages[i].rectTransform.Rotate(0f, 0f, 30f * Time.deltaTime);
            }
        }
    }

    public void UpdateBombHolder(Player currentHolder)
    {
        int newId = (currentHolder == null || currentHolder.isEliminated || (RankedMatchManager.Instance != null && RankedMatchManager.Instance.isMatchComplete)) 
            ? -1 : currentHolder.playerId;
        
        if (newId != currentBombPlayerId)
        {
            currentBombPlayerId = newId;
            RefreshHandItems();
        }
    }

    public void PlayEliminationSequence(int playerIndex, System.Action onComplete)
    {
        if (playerIndex < 0 || playerIndex >= 4)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(EliminationRoutine(playerIndex, onComplete));
    }

    private IEnumerator EliminationRoutine(int id, System.Action onComplete)
    {
        isEliminated[id] = true;
        if (handItemImages != null && id < handItemImages.Length && handItemImages[id] != null)
            handItemImages[id].gameObject.SetActive(false);

        Vector2 pos = basePositions[id];

        // --- Step 1: Play Explosion on the Platform ---
        if (explosionImage != null)
        {
            explosionImage.gameObject.SetActive(true);
            explosionImage.rectTransform.anchoredPosition = pos + new Vector2(0f, 100f); // center of character
            explosionImage.rectTransform.localScale = Vector3.one * 0.1f;
            explosionImage.color = Color.white;
            
            // Fast scale up and fade out
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                explosionImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * 3f, t);
                explosionImage.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }
            explosionImage.gameObject.SetActive(false);
        }

        // --- Step 2: Open Green Exit Portal ---
        if (exitPortalImage != null && characterImages[id] != null)
        {
            exitPortalImage.gameObject.SetActive(true);
            exitPortalImage.rectTransform.anchoredPosition = pos + new Vector2(0f, -50f);
            exitPortalImage.rectTransform.localScale = Vector3.zero;

            // Spin and scale up portal
            float elapsed = 0f;
            float portalInDuration = 0.4f;
            while (elapsed < portalInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / portalInDuration;
                exitPortalImage.rectTransform.localScale = Vector3.one * t;
                exitPortalImage.rectTransform.Rotate(0f, 0f, 360f * Time.deltaTime);
                yield return null;
            }

            // Shrink character into the portal
            elapsed = 0f;
            float charShrinkDuration = 0.5f;
            Vector3 startScale = characterImages[id].rectTransform.localScale;
            while (elapsed < charShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / charShrinkDuration;
                characterImages[id].rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                characterImages[id].rectTransform.Rotate(0f, 0f, 180f * Time.deltaTime);
                exitPortalImage.rectTransform.Rotate(0f, 0f, 360f * Time.deltaTime);
                yield return null;
            }
            characterImages[id].gameObject.SetActive(false);

            // Close portal
            elapsed = 0f;
            float portalOutDuration = 0.3f;
            while (elapsed < portalOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / portalOutDuration;
                exitPortalImage.rectTransform.localScale = Vector3.one * (1f - t);
                exitPortalImage.rectTransform.Rotate(0f, 0f, 360f * Time.deltaTime);
                yield return null;
            }
            exitPortalImage.gameObject.SetActive(false);
        }
        else
        {
            // Fallback
            if (characterImages[id] != null)
            {
                characterImages[id].gameObject.SetActive(false);
            }
        }

        onComplete?.Invoke();
    }
}