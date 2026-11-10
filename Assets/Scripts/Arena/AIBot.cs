using UnityEngine;
using System.Collections;

public class AIBot : MonoBehaviour
{
    public float skill = 0.7f;
    private BasePuzzle currentPuzzle;
    private Coroutine solveRoutine;

    public void BindPuzzle(BasePuzzle puzzle, float delayMultiplier = 1.0f)
    {
        Stop();
        currentPuzzle = puzzle;
        if (puzzle != null)
        {
            float baseDelay = 5.0f - (skill * 3.0f);
            float actualDelay = Mathf.Max(1.0f, baseDelay * delayMultiplier);
            solveRoutine = StartCoroutine(SolveAfterDelay(actualDelay));
        }
    }

    public void Stop()
    {
        if (solveRoutine != null)
        {
            StopCoroutine(solveRoutine);
            solveRoutine = null;
        }
        currentPuzzle = null;
    }

    private IEnumerator SolveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (currentPuzzle != null && !currentPuzzle.isSolved && !currentPuzzle.isTimedOut)
        {
            currentPuzzle.AutoSolveChunk();
            yield return new WaitForSeconds(0.35f);
        }
        solveRoutine = null;
    }
}
