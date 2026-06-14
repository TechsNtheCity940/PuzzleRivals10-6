using UnityEngine;
using System;

public class PuzzleBase : MonoBehaviour
{
    public event Action<int> OnSolved; // score

    public bool inputEnabled = false;
    public bool isSolved = false;
    protected int difficulty = 1;

    public virtual void Setup(int difficulty)
    {
        this.difficulty = difficulty;
    }

    protected void Solve(int score = 0)
    {
        if (isSolved) return;
        isSolved = true;
        OnSolved?.Invoke(score);
    }
}
