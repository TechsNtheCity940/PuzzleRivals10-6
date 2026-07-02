using UnityEngine;
using UnityEngine.UI;

// Shared sizing helper for puzzle overlays. Puzzle overlay prefabs have wildly
// inconsistent root sizes (e.g. 600x850, 760x1000, 1200x1200), so instead of
// trusting their authored size we uniformly scale each overlay's root so it
// fills a consistent fraction of the canvas. This keeps every puzzle legible
// and consistently sized regardless of how its prefab was built.
public static class OverlayFitter
{
    // Reads the canvas' authored reference resolution (falls back to 1080x1920,
    // the portrait reference both match managers use).
    public static Vector2 GetReferenceResolution(Canvas canvas)
    {
        if (canvas != null)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.referenceResolution.x > 0f && scaler.referenceResolution.y > 0f)
                return scaler.referenceResolution;
        }
        return new Vector2(1080f, 1920f);
    }

    // Uniformly scales the overlay root so it fits within
    // (widthFrac x heightFrac) of the canvas reference resolution.
    // Stretch-anchored roots (size ~0) are left untouched since they already
    // fill their parent. Returns the applied uniform scale.
    public static float Fit(RectTransform rt, Canvas canvas, float widthFrac, float heightFrac)
    {
        if (rt == null) return 1f;

        Vector2 root = rt.sizeDelta;
        if (root.x < 1f || root.y < 1f) return rt.localScale.x; // stretch-anchored; leave as-is

        Vector2 res = GetReferenceResolution(canvas);
        float targetW = res.x * widthFrac;
        float targetH = res.y * heightFrac;
        float scale = Mathf.Min(targetW / root.x, targetH / root.y);

        rt.localScale = new Vector3(scale, scale, 1f);
        return scale;
    }
}
