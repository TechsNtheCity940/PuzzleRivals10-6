using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateRound(int roundIndex, int playerCount)
    {
        // Stub implementation - called by RankedMatchManager
    }

    public void UpdatePlayerCard(PlayerProfileData profile)
    {
        // Stub implementation - called by RankedMatchManager
    }
    
    public void ShowScoreboard(List<MatchResult> results)
    {
        // Show scoreboard via MatchLeaderboard
        var leaderboard = FindFirstObjectByType<MatchLeaderboard>(FindObjectsInactive.Include);
        if (leaderboard == null)
        {
            var go = new GameObject("MatchLeaderboard");
            leaderboard = go.AddComponent<MatchLeaderboard>();
        }
    }
}