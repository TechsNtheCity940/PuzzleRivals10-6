using UnityEngine;
using System.Collections;

public class AIBot : MonoBehaviour
{
    public float skill = 0.7f;
    private PuzzleBase currentPuzzle;
    private Coroutine solveRoutine;

    public void BindPuzzle(PuzzleBase puzzle, float delayMultiplier = 1.0f)
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
    }

    private IEnumerator SolveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentPuzzle != null)
        {
            currentPuzzle.SendMessage("BotSolve", SendMessageOptions.DontRequireReceiver);
        }
    }
}
