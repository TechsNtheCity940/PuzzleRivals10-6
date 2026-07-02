using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NeonNudgePuzzle : BasePuzzle, IPointerClickHandler
{
    [Header("Assets")]
    public RectTransform orb;
    public RectTransform goal;
    public GameObject obstaclePrefab;
    public RectTransform obstacleContainer;

    [Header("Physics")]
    public float nudgeForce = 500f;
    public float drag = 0.98f;
    public float bounce = 0.5f;

    private Vector2 velocity;
    private Vector2 currentPos;
    private List<RectTransform> obstacles = new List<RectTransform>();

    protected override void BuildBoard()
    {
        currentPos = new Vector2(-400, 0);
        velocity = Vector2.zero;
        if (orb != null) orb.anchoredPosition = currentPos;
        if (goal != null) goal.anchoredPosition = new Vector2(400, 0);

        ClearObstacles();
        int count = 3 + difficulty;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(obstaclePrefab, obstacleContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rng.Next(-300, 300), rng.Next(-400, 400));
            obstacles.Add(rt);
        }
    }

    private void ClearObstacles()
    {
        foreach (Transform t in obstacleContainer) Destroy(t.gameObject);
        obstacles.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanAcceptInput()) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, null, out Vector2 localPos);
        Vector2 dir = (currentPos - localPos).normalized;
        OnNudge(dir);
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || isSolved) return;

        currentPos += velocity * Time.deltaTime;
        velocity *= drag;

        // Simple boundary check
        if (Mathf.Abs(currentPos.x) > 500) { currentPos.x = Mathf.Sign(currentPos.x) * 500; velocity.x *= -bounce; }
        if (Mathf.Abs(currentPos.y) > 900) { currentPos.y = Mathf.Sign(currentPos.y) * 900; velocity.y *= -bounce; }

        if (orb != null) orb.anchoredPosition = currentPos;

        CheckCollisions();
        CheckWin();
    }

    private void CheckCollisions()
    {
        foreach (var obs in obstacles)
        {
            float dist = Vector2.Distance(currentPos, obs.anchoredPosition);
            if (dist < 80f) // Simplified circular collision
            {
                Vector2 dir = (currentPos - obs.anchoredPosition).normalized;
                velocity = dir * nudgeForce * 0.5f;
            }
        }
    }

    private void CheckWin()
    {
        if (goal != null && Vector2.Distance(currentPos, goal.anchoredPosition) < 60f)
        {
            Solve();
        }
    }

    public void OnNudge(Vector2 direction)
    {
        if (!CanAcceptInput()) return;
        velocity += direction * nudgeForce;
    }

    public override void AutoSolveChunk()
    {
        if (goal == null) return;
        Vector2 dir = (goal.anchoredPosition - currentPos).normalized;
        OnNudge(dir);
    }
}
