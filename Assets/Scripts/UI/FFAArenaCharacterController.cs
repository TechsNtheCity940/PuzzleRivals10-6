using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FFAArenaCharacterController : MonoBehaviour
{
    public static FFAArenaCharacterController Instance;

    [Header("Character Images (0=Magenta, 1=Cyan, 2=Lime, 3=Orange)")]
    public Image[] characterImages;

    [Header("Walk Cycle Frames (per character)")]
    public Sprite[] walkMagenta;
    public Sprite[] walkCyan;
    public Sprite[] walkLime;
    public Sprite[] walkOrange;
    public float walkFps = 14f;          // cadence while strolling
    public float panicWalkFps = 22f;     // faster, frantic stride while solving
    public float celebrateWalkFps = 18f; // lively stride while celebrating

    // Built at runtime: walk frame set + body animator per character.
    private Sprite[][] walkSets = new Sprite[4][];
    private UISpriteSheetAnimator[] bodyAnimators = new UISpriteSheetAnimator[4];
    private bool _isMoving; // set by TickWaiting for the character currently updating

    [Header("Hand Held Items")]
    public Image[] handItemImages;
    public Sprite puzzlePieceSprite; // This will be puzzle_icon_neon
    public Sprite bombLitSprite;    // This will be live_bomb_neon
    public Sprite[] bombHeldFrames; // sliced bomb_held_08f sheet; played while panicking
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

    [Header("Idle / Pacing (waiting players)")]
    public float paceRange = 115f;      // max horizontal wander from base (px)
    public float paceSpeed = 78f;       // walking speed (px/sec)
    public float stepHeight = 16f;      // walk-bob height
    public float stepFrequency = 8f;    // step cadence
    public float minPacePause = 0.4f;   // rest between walks
    public float maxPacePause = 2.0f;
    public bool useFacingFlip = true;   // mirror sprite to face walking direction
    public float breathSpeed = 1.6f;
    public float breathAmount = 0.05f;
    public float itemFloatSpeed = 3f;
    public float itemFloatAmount = 10f;

    [Header("Panic (holding the bomb / solving)")]
    public float panicShake = 16f;        // horizontal jitter (px)
    public float panicShakeSpeed = 34f;
    public float panicHopHeight = 28f;    // frantic little jumps
    public float panicHopSpeed = 13f;
    public float panicTremor = 9f;        // trembling rotation (deg)
    public float panicTremorSpeed = 30f;
    public float panicScalePulse = 0.09f;

    [Header("Celebrate (just solved / passed bomb)")]
    public float celebrateDuration = 1.6f;
    public float celebrateHop = 55f;
    public float celebrateHopSpeed = 10f;
    public float celebrateTilt = 15f;

    private enum CharState { Waiting, Panicking, Celebrating }

    private Vector2[] basePositions;
    private Vector3[] baseScale;
    private bool[] isEliminated = new bool[4];
    private float[] individualOffset;
    private int currentBombPlayerId = -1;

    // Per-character behaviour state.
    private CharState[] states = new CharState[4];
    private float[] stateTimer = new float[4];
    private int[] facing = new int[4];
    private float[] paceCurX = new float[4];
    private float[] paceTargetX = new float[4];
    private float[] pacePause = new float[4];
    private float[] stepPhase = new float[4];
    private UISpriteSheetAnimator[] itemAnimators = new UISpriteSheetAnimator[4];

    private void Awake()
    {
        Instance = this;
        individualOffset = new float[4];
        for (int i = 0; i < 4; i++)
        {
            individualOffset[i] = Random.Range(0f, 100f);
            states[i] = CharState.Waiting;
            facing[i] = 1;
            paceCurX[i] = 0f;
            paceTargetX[i] = Random.Range(-paceRange, paceRange);
            pacePause[i] = Random.Range(0f, maxPacePause);
        }
    }

    private void Start()
    {
        // Cache initial positions
        if (characterImages != null && characterImages.Length >= 4)
        {
            basePositions = new Vector2[4];
            baseScale = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                if (characterImages[i] != null)
                {
                    var rt = characterImages[i].rectTransform;
                    basePositions[i] = rt.anchoredPosition;
                    Vector3 s = rt.localScale;
                    // Guard against a zero/negative authored scale.
                    baseScale[i] = (s.sqrMagnitude < 0.0001f) ? Vector3.one
                                   : new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), 1f);
                    characterImages[i].gameObject.SetActive(true);
                }
                else baseScale[i] = Vector3.one;
            }
        }

        // Build per-character walk frame sets and attach a body sprite-sheet
        // animator to each character Image so they play a real walk cycle.
        walkSets[0] = walkMagenta;
        walkSets[1] = walkCyan;
        walkSets[2] = walkLime;
        walkSets[3] = walkOrange;
        for (int i = 0; i < 4; i++)
        {
            if (characterImages == null || i >= characterImages.Length || characterImages[i] == null) continue;
            var anim = characterImages[i].GetComponent<UISpriteSheetAnimator>();
            if (anim == null) anim = characterImages[i].gameObject.AddComponent<UISpriteSheetAnimator>();
            anim.playOnEnable = false;
            anim.Stop();
            bodyAnimators[i] = anim;
            // Seed with the first walk frame so the resting pose matches the cycle.
            if (walkSets[i] != null && walkSets[i].Length > 0 && walkSets[i][0] != null)
                characterImages[i].sprite = walkSets[i][0];
        }

        // Setup hand items
        RefreshHandItems();

        // Initialize FX states
        if (feetBombIcon != null) feetBombIcon.gameObject.SetActive(false);
        if (exitPortalImage != null) exitPortalImage.gameObject.SetActive(false);
        if (explosionImage != null) explosionImage.gameObject.SetActive(false);
    }

    private Color PlayerTint(int i)
    {
        switch (i)
        {
            case 0: return new Color(1f, 0f, 1f, 0.9f);    // Magenta
            case 1: return new Color(0f, 1f, 1f, 0.9f);    // Cyan
            case 2: return new Color(0.5f, 1f, 0f, 0.9f);  // Lime
            default: return new Color(1f, 0.5f, 0f, 0.9f); // Orange
        }
    }

    private void RefreshHandItems()
    {
        if (handItemImages == null) return;

        for (int i = 0; i < handItemImages.Length && i < 4; i++)
        {
            Image item = handItemImages[i];
            if (item == null) continue;

            // Lazily grab/attach a sheet animator for the held bomb animation.
            UISpriteSheetAnimator anim = itemAnimators[i];
            if (anim == null)
            {
                anim = item.GetComponent<UISpriteSheetAnimator>();
                if (anim == null) anim = item.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.playOnEnable = false;
                itemAnimators[i] = anim;
            }

            bool hasBomb = (i == currentBombPlayerId);
            if (hasBomb)
            {
                // Animated, sizzling lit bomb clutched by the panicking solver.
                if (bombHeldFrames != null && bombHeldFrames.Length > 0)
                    anim.PlaySet(bombHeldFrames, 14f, true);
                else
                {
                    anim.Stop();
                    if (bombLitSprite != null) item.sprite = bombLitSprite;
                }
                item.color = Color.white;
                item.rectTransform.sizeDelta = new Vector2(92f, 92f);
            }
            else
            {
                anim.Stop();
                if (puzzlePieceSprite != null) item.sprite = puzzlePieceSprite;
                item.color = PlayerTint(i);
                item.rectTransform.sizeDelta = new Vector2(60f, 60f);
            }

            item.gameObject.SetActive(!isEliminated[i]);
        }
    }

    private void Update()
    {
        if (characterImages == null || basePositions == null || baseScale == null) return;

        float dt = Time.deltaTime;
        for (int i = 0; i < characterImages.Length && i < 4; i++)
        {
            Image img = characterImages[i];
            if (img == null || isEliminated[i]) continue;

            Vector2 offset = Vector2.zero;
            float rotZ = 0f, sx = 1f, sy = 1f;

            switch (states[i])
            {
                case CharState.Panicking:
                    TickPanic(i, dt, ref offset, ref rotZ, ref sx, ref sy);
                    SetBodyWalking(i, true, panicWalkFps); // frantic stride in place
                    break;
                case CharState.Celebrating:
                    TickCelebrate(i, dt, ref offset, ref rotZ, ref sx, ref sy);
                    SetBodyWalking(i, true, celebrateWalkFps);
                    break;
                default:
                    TickWaiting(i, dt, ref offset, ref rotZ, ref sx, ref sy);
                    SetBodyWalking(i, _isMoving, walkFps); // animate only while strolling
                    break;
            }

            RectTransform rt = img.rectTransform;
            rt.anchoredPosition = basePositions[i] + offset;
            rt.localRotation = Quaternion.Euler(0f, 0f, rotZ);
            float fx = (useFacingFlip ? facing[i] : 1) * baseScale[i].x * sx;
            rt.localScale = new Vector3(fx, baseScale[i].y * sy, 1f);

            TickHandItem(i, dt);
        }
    }

    // Plays / pauses a character's walk-cycle frames. When walking, ensures the
    // loop is running at the requested fps (without restarting an already-running
    // loop). When not walking, freezes on the current frame so the resting pose
    // stays consistent with the cycle.
    private void SetBodyWalking(int i, bool walking, float fps)
    {
        var anim = bodyAnimators[i];
        if (anim == null) return;
        Sprite[] set = walkSets[i];
        if (set == null || set.Length == 0) return; // no walk art assigned: leave static

        if (walking)
        {
            if (!anim.IsPlaying || Mathf.Abs(anim.fps - fps) > 0.01f)
                anim.PlaySet(set, fps, true);
        }
        else
        {
            anim.Stop();
        }
    }

    // Waiting: stroll back and forth across the platform with a walk-bob, pausing
    // to breathe at the ends. This replaces the old "float up and down".
    private void TickWaiting(int i, float dt, ref Vector2 offset, ref float rotZ, ref float sx, ref float sy)
    {
        bool moving = false;
        _isMoving = false;

        if (pacePause[i] > 0f)
        {
            pacePause[i] -= dt;
        }
        else
        {
            float diff = paceTargetX[i] - paceCurX[i];
            if (Mathf.Abs(diff) <= 1.5f)
            {
                // Arrived: rest, then choose a fresh target a decent distance away.
                pacePause[i] = Random.Range(minPacePause, maxPacePause);
                float next;
                int guard = 0;
                do { next = Random.Range(-paceRange, paceRange); }
                while (Mathf.Abs(next - paceCurX[i]) < paceRange * 0.4f && guard++ < 8);
                paceTargetX[i] = next;
                facing[i] = next >= paceCurX[i] ? 1 : -1;
            }
            else
            {
                float step = Mathf.Sign(diff) * paceSpeed * dt;
                if (Mathf.Abs(step) > Mathf.Abs(diff)) step = diff;
                paceCurX[i] += step;
                stepPhase[i] += dt * stepFrequency;
                moving = true;
                _isMoving = true;
            }
        }

        if (moving)
        {
            float bob = Mathf.Abs(Mathf.Sin(stepPhase[i]));
            offset = new Vector2(paceCurX[i], bob * stepHeight);
            // Squash on footfall (bob near 0), stretch mid-stride; subtle stride sway.
            float contact = 1f - bob;
            sx = 1f + contact * 0.05f;
            sy = 1f - contact * 0.05f;
            rotZ = -facing[i] * 3f * Mathf.Cos(stepPhase[i]);
        }
        else
        {
            float b = 1f + Mathf.Sin(Time.time * breathSpeed + individualOffset[i]) * breathAmount;
            offset = new Vector2(paceCurX[i], 0f);
            sy = b;
        }
    }

    // Panicking: frantically holding the lit bomb while solving — jittery shaking,
    // little hops and a nervous tremble. Eases back toward platform centre.
    private void TickPanic(int i, float dt, ref Vector2 offset, ref float rotZ, ref float sx, ref float sy)
    {
        float t = Time.time;
        paceCurX[i] = Mathf.MoveTowards(paceCurX[i], 0f, paceSpeed * dt);

        float shake = Mathf.Sin(t * panicShakeSpeed + individualOffset[i]) * panicShake
                    + Mathf.Sin(t * panicShakeSpeed * 1.7f + individualOffset[i]) * (panicShake * 0.4f);
        float hop = Mathf.Abs(Mathf.Sin(t * panicHopSpeed + individualOffset[i])) * panicHopHeight;

        offset = new Vector2(paceCurX[i] + shake, hop);
        rotZ = Mathf.Sin(t * panicTremorSpeed + individualOffset[i]) * panicTremor;

        float pulse = 1f + Mathf.Sin(t * panicHopSpeed * 2f) * panicScalePulse;
        sy = pulse;
        sx = 2f - pulse; // keep apparent volume roughly constant
    }

    // Celebrating: brief joyful hopping after solving and passing the bomb on.
    private void TickCelebrate(int i, float dt, ref Vector2 offset, ref float rotZ, ref float sx, ref float sy)
    {
        stateTimer[i] -= dt;
        float t = Time.time;
        paceCurX[i] = Mathf.MoveTowards(paceCurX[i], 0f, paceSpeed * dt);

        float hop = Mathf.Abs(Mathf.Sin(t * celebrateHopSpeed + individualOffset[i])) * celebrateHop;
        offset = new Vector2(paceCurX[i], hop);
        rotZ = Mathf.Sin(t * celebrateHopSpeed * 1.3f) * celebrateTilt;

        float stretch = 1f + Mathf.Abs(Mathf.Sin(t * celebrateHopSpeed)) * 0.10f;
        sy = stretch;
        sx = 2f - stretch;

        if (stateTimer[i] <= 0f) EnterWaiting(i);
    }

    // Hand item: nervous bomb clutch while panicking, gentle float+spin otherwise.
    private void TickHandItem(int i, float dt)
    {
        if (handItemImages == null || i >= handItemImages.Length) return;
        Image item = handItemImages[i];
        if (item == null) return;

        Vector2 baseOff = (handOffsets != null && i < handOffsets.Length) ? handOffsets[i] : new Vector2(100f, 220f);
        RectTransform irt = item.rectTransform;

        if (i == currentBombPlayerId)
        {
            float t = Time.time;
            float jx = Mathf.Sin(t * panicShakeSpeed * 0.9f + individualOffset[i]) * 6f;
            float jy = Mathf.Abs(Mathf.Sin(t * panicHopSpeed + 1f)) * 6f;
            irt.anchoredPosition = baseOff + new Vector2(jx, jy);
            irt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 8f) * 10f);
        }
        else
        {
            float itemTime = Time.time * itemFloatSpeed + individualOffset[i];
            float itemY = baseOff.y + Mathf.Sin(itemTime) * itemFloatAmount;
            irt.anchoredPosition = new Vector2(baseOff.x, itemY);
            irt.Rotate(0f, 0f, 30f * dt);
        }
    }

    private void EnterWaiting(int i)
    {
        states[i] = CharState.Waiting;
        pacePause[i] = Random.Range(0f, minPacePause);
        paceTargetX[i] = Random.Range(-paceRange, paceRange);
        facing[i] = paceTargetX[i] >= paceCurX[i] ? 1 : -1;
    }

    private void EnterPanic(int i)
    {
        states[i] = CharState.Panicking;
        facing[i] = 1; // face forward while panicking (no flip jitter)
    }

    private void EnterCelebrate(int i)
    {
        states[i] = CharState.Celebrating;
        stateTimer[i] = celebrateDuration;
        facing[i] = 1;
    }

    public void UpdateBombHolder(Player currentHolder)
    {
        int newId = (currentHolder == null || currentHolder.isEliminated || (RankedMatchManager.Instance != null && RankedMatchManager.Instance.isMatchComplete)) 
            ? -1 : currentHolder.playerId;

        if (newId == currentBombPlayerId) return;

        int prevId = currentBombPlayerId;
        currentBombPlayerId = newId;

        // The previous holder just got rid of the bomb (they solved it) -> celebrate.
        if (prevId >= 0 && prevId < 4 && !isEliminated[prevId])
            EnterCelebrate(prevId);

        // The new holder is now frantically solving -> panic with the lit bomb.
        if (newId >= 0 && newId < 4 && !isEliminated[newId])
            EnterPanic(newId);

        RefreshHandItems();
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