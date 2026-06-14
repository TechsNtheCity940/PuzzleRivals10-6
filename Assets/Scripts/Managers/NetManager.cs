using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

// Outcome of a matchmaking attempt. The lobby is ALWAYS filled to totalSlots:
// human slots are real players found online; the remainder are AI bots.
public class MatchmakingResult
{
    public bool online;            // did we reach the matchmaking backend?
    public int totalSlots;
    public int realPlayers;        // count of human slots (always includes local)
    public int botCount;           // AI bots filling the rest
    public string roomId = "";
    public List<int> humanSlots = new List<int>(); // slot indices that are real humans
    public List<int> aiSlots = new List<int>();     // slot indices filled by AI bots
}

// Client matchmaking. Every game mode calls FindMatch first to search for real
// players against the backend; if the backend is unreachable or returns no
// opponents within the timeout, the lobby is filled with AI bots so the match
// can always start. WebGL-safe (UnityWebRequest + coroutines, no threads).
public class NetManager : MonoBehaviour
{
    public static NetManager Instance { get; private set; }

    [Header("Backend")]
    public string serverUrl = "https://mp.tesana.ai";
    public float defaultSearchTimeout = 6f;

    [Header("State")]
    public bool isConnected = false;
    public string roomId = "";

    public event Action<MatchmakingResult> OnMatchReady;

    private MatchmakingResult _lastResult;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Searches for real players to fill `totalSlots` (localSlot is always the
    // local human). Fills any unfilled slots with AI bots. Invokes onComplete
    // with the resolved roster. Never fails — always returns a full lobby.
    public IEnumerator FindMatch(int totalSlots, int localSlot, float timeout, Action<MatchmakingResult> onComplete)
    {
        totalSlots = Mathf.Max(1, totalSlots);
        localSlot = Mathf.Clamp(localSlot, 0, totalSlots - 1);

        var result = new MatchmakingResult { totalSlots = totalSlots };
        var humans = new List<int> { localSlot }; // the local player is always real
        bool online = false;

        Debug.Log($"[Matchmaking] Searching for real players ({totalSlots} slots, {timeout:0.0}s timeout)...");

        UnityWebRequest req = null;
        bool requestFaulted = false;
        try
        {
            req = UnityWebRequest.Get(serverUrl + "/matchmake?slots=" + totalSlots);
            req.timeout = Mathf.Max(1, Mathf.CeilToInt(timeout));
        }
        catch (Exception e)
        {
            requestFaulted = true;
            Debug.LogWarning("[Matchmaking] Request setup failed: " + e.Message);
        }

        if (!requestFaulted && req != null)
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                online = true;
                // Hook for a real backend: parse the returned lobby and add any
                // remote human slot indices to `humans`. With no live protocol,
                // no fake humans are fabricated — we honestly fall back to bots.
                TryParseRemoteHumans(req.downloadHandler != null ? req.downloadHandler.text : "", totalSlots, localSlot, humans);
                Debug.Log("[Matchmaking] Backend reachable. Real players found: " + humans.Count);
            }
            else
            {
                Debug.Log("[Matchmaking] Backend unreachable (" + req.result + "). Falling back to AI bots.");
            }
            req.Dispose();
        }

        // Fill remaining slots with AI bots.
        for (int slot = 0; slot < totalSlots; slot++)
            if (!humans.Contains(slot)) result.aiSlots.Add(slot);

        result.online = online;
        result.humanSlots = humans;
        result.realPlayers = humans.Count;
        result.botCount = result.aiSlots.Count;
        result.roomId = (online ? "ROOM_" : "LOCAL_") + UnityEngine.Random.Range(1000, 9999);

        isConnected = online;
        roomId = result.roomId;
        _lastResult = result;

        Debug.Log($"[Matchmaking] Lobby ready ({result.roomId}): {result.realPlayers} real, {result.botCount} bots.");
        OnMatchReady?.Invoke(result);
        onComplete?.Invoke(result);
    }

    public IEnumerator FindMatch(int totalSlots, int localSlot, Action<MatchmakingResult> onComplete)
        => FindMatch(totalSlots, localSlot, defaultSearchTimeout, onComplete);

    // Integration point for a real backend response. Expected to append remote
    // human slot indices (other than localSlot) to `humans`. No-op until a live
    // matchmaking protocol is wired, so the client safely falls back to bots.
    private void TryParseRemoteHumans(string responseBody, int totalSlots, int localSlot, List<int> humans)
    {
        if (string.IsNullOrEmpty(responseBody)) return;
        // TODO: parse responseBody (e.g. JSON of joined player slots) and add
        // each remote human slot index to `humans` (deduped, within range).
    }

    public bool IsAi(int slot)
    {
        return _lastResult != null && _lastResult.aiSlots.Contains(slot);
    }
}
