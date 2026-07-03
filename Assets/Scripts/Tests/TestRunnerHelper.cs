using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRunnerHelper : MonoBehaviour
{
    private const string StateKey = "PlayModeTest.State";
    private const string ResultKey = "PlayModeTest.Result";

    public void StartTest()
    {
        StartCoroutine(RunTest());
    }

    private IEnumerator RunTest()
    {
        Debug.Log("[Test] Mirror Maze Validation Started");
        
        var gs = FindObjectOfType<GameState>();
        if (gs != null) gs.currentMode = GameState.Mode.FFA;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
        yield return new WaitForSeconds(2f);

        // Force Mirror Maze in MatchBootstrap
        var bootstrap = FindObjectOfType<MatchBootstrap>();
        if (bootstrap != null) {
            var prefab = bootstrap.overlayPrefabs.Find(p => p != null && p.name.Contains("MirrorMaze"));
            if (prefab != null) {
                Debug.Log("[Test] Found MirrorMaze prefab in bootstrap. Forcing it...");
                bootstrap.overlayPrefabs = new List<GameObject> { prefab };
                
                // Reload scene to trigger bootstrap.Start() again
                UnityEngine.SceneManagement.SceneManager.LoadScene("Match");
                yield return new WaitForSeconds(2f);
            }
        }

        var puzzle = FindObjectOfType<MirrorMazePuzzle>();
        if (puzzle != null) {
            Debug.Log("[Test] Mirror Maze instance found. Starting solving...");
            int safety = 0;
            while (!puzzle.isSolved && safety < 100) {
                puzzle.AutoSolveChunk();
                safety++;
                yield return new WaitForSeconds(0.15f);
            }
            UnityEditor.SessionState.SetString(ResultKey, puzzle.isSolved ? "SUCCESS: Solved Mirror Maze" : "FAILURE: Could not solve");
        } else {
            UnityEditor.SessionState.SetString(ResultKey, "FAILURE: Puzzle instance not found");
        }

        UnityEditor.SessionState.SetString(StateKey, "Done");
        UnityEditor.EditorApplication.isPlaying = false;
        Destroy(gameObject);
    }
}