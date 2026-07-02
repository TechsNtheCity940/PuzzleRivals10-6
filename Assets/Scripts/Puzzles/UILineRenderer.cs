using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2[] points;
    public float thickness = 4f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points == null || points.Length < 2) return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            AddLineSegment(vh, points[i], points[i + 1]);
        }
    }

    private void AddLineSegment(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        int vCount = vh.currentVertCount;
        vh.AddVert(start + normal, color, Vector2.zero);
        vh.AddVert(start - normal, color, Vector2.zero);
        vh.AddVert(end - normal, color, Vector2.zero);
        vh.AddVert(end + normal, color, Vector2.zero);

        vh.AddTriangle(vCount, vCount + 1, vCount + 2);
        vh.AddTriangle(vCount, vCount + 2, vCount + 3);
    }
}
