using UnityEngine;
using System.Collections.Generic;

public class PuzzleSelectionLogger : MonoBehaviour
{
    private void Start()
    {
        if (RankedMatchManager.Instance != null)
        {
            RankedMatchManager.Instance.OnMatchStateChanged += HandleTurnChanged;
        }
    }

    private void HandleTurnChanged()
    {
        var holder = RankedMatchManager.Instance.currentBombHolder;
        if (holder != null)
        {
            Debug.Log($"[LOG] Turn changed for {holder.playerName}");
        }
    }

    private void OnDestroy()
    {
        if (RankedMatchManager.Instance != null)
        {
            RankedMatchManager.Instance.OnMatchStateChanged -= HandleTurnChanged;
        }
    }
}