using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RuneTracerPuzzle : BasePuzzle, IPointerDownHandler, IDragHandler
{
    [Header("Assets")]
    public Image runeImage;
    public Sprite runePathSprite;
    public Sprite[] traceAnimFrames;

    [Header("Logic")]
    public RectTransform container;
    public float tolerance = 50f;

    private List<Vector2> pathPoints = new List<Vector2>();
    private int nextPointIndex = 0;
    private bool isTracing = false;

    protected override void BuildBoard()
    {
        nextPointIndex = 0;
        isTracing = false;
        if (runeImage != null) runeImage.sprite = runePathSprite;

        // Generate a simple circular or zig-zag path
        pathPoints.Clear();
        int count = 5 + difficulty;
        for (int i = 0; i < count; i++)
        {
            float angle = (float)i / count * Mathf.PI * 2;
            pathPoints.Add(new Vector2(Mathf.Cos(angle) * 300, Mathf.Sin(angle) * 300));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanAcceptInput()) return;
        isTracing = true;
        CheckPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isTracing || !CanAcceptInput()) return;
        CheckPoint(eventData.position);
    }

    private void CheckPoint(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPos, null, out Vector2 localPos);
        
        float dist = Vector2.Distance(localPos, pathPoints[nextPointIndex]);
        if (dist < tolerance)
        {
            nextPointIndex++;
            if (nextPointIndex >= pathPoints.Count)
            {
                Solve();
            }
        }
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;
        nextPointIndex = Mathf.Min(nextPointIndex + 1, pathPoints.Count - 1);
        if (nextPointIndex >= pathPoints.Count - 1) Solve();
    }
}
