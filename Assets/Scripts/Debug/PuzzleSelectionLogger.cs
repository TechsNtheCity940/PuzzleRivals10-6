using UnityEngine;
using System.Collections.Generic;

public class PuzzleSelectionLogger : MonoBehaviour
{
    private void Start()
    {
        if (RankedMatchManager.Instance != null)
        {
            RankedMatchManager.Instance.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void HandleTurnChanged(Player holder)
    {
        if (RankedMatchManager.Instance.CurrentPuzzle != null)
        {
            Debug.Log($"[LOG] Selected Puzzle: {RankedMatchManager.Instance.CurrentPuzzle.GetType().Name} for {holder.playerName}");
        }
    }

    private void OnDestroy()
    {
        if (RankedMatchManager.Instance != null)
        {
            RankedMatchManager.Instance.OnTurnChanged -= HandleTurnChanged;
        }
    }
}
