using UnityEngine;
using UnityEditor;
using System.Collections;

public class MirrorMazeRuntimeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (PlayerPrefs.GetInt("MMTest.Run", 0) != 1) return;
        var go = new GameObject("__MMTest");
        go.AddComponent<MirrorMazeRuntimeTest>();
    }

    private IEnumerator Start()
    {
        // Wait for match scene & MirrorMazePuzzle
        MirrorMazePuzzle puzzle = null;
        float t = 0f;
        while (puzzle == null && t < 10f)
        {
            puzzle = Object.FindAnyObjectByType<MirrorMazePuzzle>(FindObjectsInactive.Include);
            t += Time.deltaTime;
            yield return null;
        }

        if (puzzle == null)
        {
            Finish("ERROR: MirrorMazePuzzle not found in active scene!");
            yield break;
        }

        // Wait for board to be fully generated/scrambled
        yield return new WaitForSeconds(1.5f);

        // Verify elements are populated
        int size = puzzle.gridSize;
        Debug.Log("[MMTest] Found puzzle. Size=" + size + " isSolved=" + puzzle.isSolved);

        // AutoSolve chunk step-by-step
        int steps = 0;
        while (!puzzle.isSolved && steps++ < 20)
        {
            puzzle.AutoSolveChunk();
            yield return new WaitForSeconds(0.4f);
        }

        if (puzzle.isSolved)
        {
            Finish("SUCCESS: Puzzle solved cleanly in " + steps + " steps!");
        }
        else
        {
            Finish("ERROR: Puzzle failed to solve within " + steps + " steps.");
        }
    }

    private void Finish(string msg)
    {
        Debug.Log("[MMTest] RESULT " + msg);
        PlayerPrefs.DeleteKey("MMTest.Run");
        EditorApplication.isPlaying = false;
    }
}
