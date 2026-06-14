using UnityEngine;
using System;

public class MazeCell : MonoBehaviour
{
    public int gx, gy;
    public SpriteRenderer terrain;
    public SpriteRenderer entity;

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
