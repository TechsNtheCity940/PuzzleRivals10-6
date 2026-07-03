using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MirrorMazeVerify : MonoBehaviour
{
    public static void Run()
    {
        GameObject go = new GameObject("MirrorMazeVerify");
        go.AddComponent<MirrorMazeVerify>();
        DontDestroyOnLoad(go);
    }

    private void Start()
    {
        StartCoroutine(TestSequence());
    }

    private IEnumerator TestSequence()
    {
        Debug.Log("--- MIRROR MAZE VERIFY START ---");
        
        if (!Application.isPlaying) yield break;

        var state = GameState.Instance;
        if (state == null) { Debug.LogError("GameState missing"); yield break; }
        
        state.currentMode = GameState.Mode.FFA;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
        yield return new WaitForSeconds(2f);

        var rmm = FindObjectOfType<RankedMatchManager>();
        if (rmm != null)
        {
            GameObject mirrorPrefab = null;
            foreach(var p in rmm.puzzleOverlayPrefabs)
            {
                if (p != null && p.name.Contains("MirrorMaze")) { mirrorPrefab = p; break; }
            }
            
            if (mirrorPrefab != null)
            {
                Debug.Log("Forcing Mirror Maze in RankedMatchManager...");
                rmm.puzzleOverlayPrefabs = new List<GameObject> { mirrorPrefab };
            }
        }

        MirrorMazePuzzle puzzle = null;
        float timeout = 15f;
        while (puzzle == null && timeout > 0)
        {
            puzzle = FindObjectOfType<MirrorMazePuzzle>();
            timeout -= 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        if (puzzle == null) { Debug.LogError("MirrorMazePuzzle instance not found"); yield break; }

        Debug.Log("MirrorMaze found. Solved: " + puzzle.isSolved);
        
        yield return new WaitForSeconds(5f);
        
        Debug.Log("Auto-solving...");
        int steps = 0;
        while (!puzzle.isSolved && steps < 100)
        {
            puzzle.AutoSolveChunk();
            steps++;
            yield return new WaitForSeconds(0.2f);
        }

        if (puzzle.isSolved)
            Debug.Log("SUCCESS: Puzzle solved cleanly in " + steps + " steps.");
        else
            Debug.LogError("FAILURE: Puzzle not solved after 100 steps.");

        yield return new WaitForSeconds(3f);
        
        Debug.Log("--- MIRROR MAZE VERIFY COMPLETE ---");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Destroy(gameObject);
    }
}