using UnityEngine;
using System;

// Per-cell component for the MirrorBeam puzzle. A cell may be empty, hold a
// rotatable diagonal mirror, the beam emitter, or the receiver. Clicking the
// cell raises OnClicked(gx, gy); the puzzle decides what to do with it.
public class MirrorCell : MonoBehaviour
{
    public int gx, gy;

    [Tooltip("Board background tile renderer.")]
    public SpriteRenderer baseRenderer;
    [Tooltip("Content renderer: mirror / emitter / receiver.")]
    public SpriteRenderer iconRenderer;

    [Tooltip("True when this cell holds a rotatable mirror.")]
    public bool isMirror;
    [Tooltip("Mirror orientation: 0 = '/', 1 = '\\'.")]
    public int orientation;

    public event Action<int, int> OnClicked;

    public void Init(int x, int y)
    {
        gx = x;
        gy = y;
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(gx, gy);
    }
}
