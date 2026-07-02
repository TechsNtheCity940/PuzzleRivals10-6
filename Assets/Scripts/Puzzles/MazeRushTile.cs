using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// UI grid tile for the MazeRush overlay (referenced by MazeRushTile.prefab,
// which has an Image on its root). Holds a grid coordinate and terrain flags.
// The owning MazeRushPuzzle wires OnClicked to handle player movement.
[RequireComponent(typeof(Image))]
public class MazeRushTile : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;
    public bool isWall;
    public bool isCheckpoint;
    public bool isExit;
    public bool isStart;
    public bool isGate;
    public bool gateLocked;
    public bool isPortal;
    public int portalId;

    public event Action<MazeRushTile> OnClicked;

    [Header("Art (optional, null = flat colour)")]
    public Sprite[] wallFrames;
    public Sprite[] floorFrames;
    public Sprite[] startFrames;
    public Sprite[] exitFrames;
    public Sprite[] checkpointFrames;
    public Sprite[] playerFrames;

    private Image image;
    private UISpriteSheetAnimator animator;
    private bool isPlayer;

    private static readonly Color WallColor = new Color(0.12f, 0.13f, 0.18f, 1f);
    private static readonly Color FloorColor = new Color(0.82f, 0.86f, 0.96f, 1f);
    private static readonly Color StartColor = new Color(0f, 0.90f, 1f, 1f);
    private static readonly Color ExitColor = new Color(0.61f, 1f, 0.18f, 1f);
    private static readonly Color CheckpointColor = new Color(1f, 0.75f, 0.20f, 1f);
    private static readonly Color PlayerColor = new Color(0.30f, 0.95f, 1f, 1f);
    private static readonly Color GateLockedColor = new Color(0.95f, 0.25f, 0.30f, 1f);
    private static readonly Color GateOpenColor = new Color(0.30f, 0.85f, 0.45f, 1f);

    public void SetGate(bool gate, bool locked)
    {
        isGate = gate;
        gateLocked = locked;
        Refresh();
    }

    public Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    public UISpriteSheetAnimator Animator
    {
        get
        {
            if (animator == null)
            {
                animator = GetComponent<UISpriteSheetAnimator>();
                if (animator == null) animator = gameObject.AddComponent<UISpriteSheetAnimator>();
            }
            return animator;
        }
    }

    // Supplies the themed art (any may be null to keep the flat-colour fallback).
    public void SetFrames(Sprite[] wall, Sprite[] floor, Sprite[] start, Sprite[] exit,
        Sprite[] checkpoint, Sprite[] player)
    {
        wallFrames = wall;
        floorFrames = floor;
        startFrames = start;
        exitFrames = exit;
        checkpointFrames = checkpoint;
        playerFrames = player;
        Refresh();
    }

    public void Init(int gx, int gy)
    {
        x = gx;
        y = gy;
        Refresh();
    }

    public void SetWall(bool value)
    {
        isWall = value;
        Refresh();
    }

    public void SetCheckpoint(bool value)
    {
        isCheckpoint = value;
        Refresh();
    }

    public void SetExit(bool value)
    {
        isExit = value;
        Refresh();
    }

    public void SetStart(bool value)
    {
        isStart = value;
        Refresh();
    }

    public void SetPortal(bool portal, int id)
    {
        isPortal = portal;
        portalId = id;
        Refresh();
    }

    // Player highlight overlay (not a serialized terrain flag).
    public void SetPlayer(bool value)
    {
        isPlayer = value;
        Refresh();
    }

    private void Refresh()
    {
        // Walls and floors now use dedicated, distinct sprite art so the grid
        // actually reads as a maze (textured wall blocks vs. dark floor panels).
        // Structural tiles are STATIC (single frame) to keep the maze readable;
        // only the unique start/exit/checkpoint markers animate. The player is a
        // separate animated drone icon (CreatePlayerIcon), so isPlayer no longer
        // tints the tile underneath — the floor shows through beneath the drone.
        Sprite[] animFrames = null;   // animated marker frames
        Sprite staticSprite = null;   // single static sprite (wall / floor)
        Color col;

        if (isGate) { col = gateLocked ? GateLockedColor : GateOpenColor; }
        else if (isPortal) { col = (portalId == 0) ? new Color(0.7f, 0.1f, 1f, 1f) : new Color(1f, 0.5f, 0f, 1f); }
        else if (isWall) { staticSprite = FirstFrame(wallFrames); col = WallColor; }
        else if (isExit) { animFrames = exitFrames; col = ExitColor; }
        else if (isStart) { animFrames = startFrames; col = StartColor; }
        else if (isCheckpoint) { animFrames = checkpointFrames; col = CheckpointColor; }
        else { staticSprite = FirstFrame(floorFrames); col = FloorColor; }

        if (animFrames != null && animFrames.Length > 0)
        {
            Animator.PlaySet(animFrames, 10f, true);
            Image.color = Color.white;
        }
        else if (staticSprite != null)
        {
            Animator.Stop();
            Image.sprite = staticSprite;
            Image.color = Color.white;
        }
        else
        {
            Animator.Stop();
            Image.sprite = null;
            Image.color = col;
        }
    }

    private static Sprite FirstFrame(Sprite[] arr)
    {
        return (arr != null && arr.Length > 0) ? arr[0] : null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
