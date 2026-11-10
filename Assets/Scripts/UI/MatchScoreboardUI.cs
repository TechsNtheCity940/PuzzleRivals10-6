using UnityEngine;
using System.Collections.Generic;

public class MatchScoreboardUI : MonoBehaviour
{
    public static MatchScoreboardUI Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Show(List<MatchResult> results)
    {
        // Show scoreboard - called by RankedMatchManager and BrainVSBrainManager
        var leaderboard = FindAnyObjectByType<MatchLeaderboard>();
        if (leaderboard == null)
        {
            var go = new GameObject("MatchLeaderboard");
            leaderboard = go.AddComponent<MatchLeaderboard>();
        }
        
        var entries = new List<LeaderboardEntry>();
        int rank = 1;
        foreach (var r in results)
        {
            entries.Add(new LeaderboardEntry
            {
                playerName = r.playerName,
                scoreLabel = "SCORE: " + r.score,
                placeLabel = PlaceLabel(rank++),
                emblem = null
            });
        }
        
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            leaderboard.Show("RESULTS", entries);
        }
    }
    
    private string PlaceLabel(int rank)
    {
        switch (rank)
        {
            case 1: return "1ST";
            case 2: return "2ND";
            case 3: return "3RD";
            default: return rank + "TH";
        }
    }
}