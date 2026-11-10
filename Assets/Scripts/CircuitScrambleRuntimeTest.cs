#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CircuitScrambleRuntimeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (PlayerPrefs.GetInt("CSTest.Run", 0) != 1) return;
        var go = new GameObject("__CSTest");
        go.AddComponent<CircuitScrambleRuntimeTest>();
    }

    private IEnumerator Start()
    {
        // Wait for match scene & CircuitScramblePuzzle
        CircuitScramblePuzzle puzzle = null;
        float t = 0f;
        while (puzzle == null && t < 10f)
        {
            puzzle = Object.FindAnyObjectByType<CircuitScramblePuzzle>(FindObjectsInactive.Include);
            t += Time.deltaTime;
            yield return null;
        }

        if (puzzle == null)
        {
            Finish("ERROR: CircuitScramblePuzzle not found in active scene!");
            yield break;
        }

        // Wait for board to be fully generated/scrambled
        yield return new WaitForSeconds(1.5f);

        // Verify elements are populated
        int columns = puzzle.columns;
        int rows = puzzle.rows;
        Debug.Log("[CSTest] Found puzzle. Size=" + columns + "x" + rows + " isSolved=" + puzzle.isSolved);

        // AutoSolve chunk step-by-step
        int steps = 0;
        while (!puzzle.isSolved && steps++ < 50)
        {
            puzzle.AutoSolveChunk();
            yield return new WaitForSeconds(0.15f);
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
        string path = Path.Combine(Application.dataPath, "../cstest_result.txt");
        File.WriteAllText(path, msg);
        Debug.Log("[CSTest] RESULT " + msg);
        PlayerPrefs.DeleteKey("CSTest.Run");
        EditorApplication.isPlaying = false;
    }
}
#endif // UNITY_EDITOR
