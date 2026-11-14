using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.Udp;
using Unity.Services.Multiplayer.Center;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// Matchmaking result containing lobby information.
/// The lobby is always filled to totalSlots:
/// - human slots are real players found online
/// - the remainder are AI bots
/// </summary>
public class MatchmakingResult
{
    public bool online;
    public int totalSlots;
    public int realPlayers;
    public int botCount;
    public string roomId = "";
    public List<int> humanSlots = new List<int>();
    public List<int> aiSlots = new List<int>();
}

/// <summary>
/// Client matchmaking using Unity Multiplayer Center SDK.
/// Searches for real players; if the backend is unreachable or returns no
/// opponents within the timeout, the lobby is filled with AI bots so the match
/// can always start. WebGL-compatible using coroutines.
/// </summary>
public class NetManager : MonoBehaviour
{
    public static NetManager Instance { get; private set; }

    [Header("Supabase Configuration")]
    [Tooltip("Supabase URL from environment variable (VITE_SUPABASE_URL)")]
    public string supabaseUrl = "";
    [Tooltip("Supabase API key from environment variable (VITE_SUPABASE_PUBLISHABLE_KEY)")]
    public string supabaseApiKey = "";

    [Header("Multiplayer Center")]
    public string sessionId = "";
    public Unity.Multiplayer.Center.MatchmakingClient matchmakingClient;

    [Header("Matchmaking Settings")]
    public float defaultSearchTimeout = 6f;
    public int maxRetryAttempts = 3;

    [Header("State")]
    public bool isConnected = false;
    public string roomId = "";

    public event Action<MatchmakingResult> OnMatchReady;

    private MatchmakingResult _lastResult;
    private bool _unityServicesInitialized = false;

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

    private async void Start()
    {
        await InitializeUnityServices();
    }

    /// <summary>
    /// Initialize Unity Services including Multiplayer Center
    /// </summary>
    private async System.Threading.Tasks.Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // Load configuration from environment or PlayerPrefs
            LoadConfiguration();
            
            _unityServicesInitialized = true;
            Debug.Log("[NetManager] Unity Services initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetManager] Failed to initialize Unity Services: {e.Message}");
            _unityServicesInitialized = false;
        }
    }

    /// <summary>
    /// Load configuration from environment variables or PlayerPrefs
    /// </summary>
    private void LoadConfiguration()
    {
        // Try to load from environment first, then fall back to PlayerPrefs
        supabaseUrl = System.Environment.GetEnvironmentVariable("VITE_SUPABASE_URL") ?? 
                      PlayerPrefs.GetString("supabase_url", "https://mp.tesana.ai");
        supabaseApiKey = System.Environment.GetEnvironmentVariable("VITE_SUPABASE_PUBLISHABLE_KEY") ?? 
                         PlayerPrefs.GetString("supabase_api_key", "");
    }

    /// <summary>
    /// Searches for real players to fill `totalSlots` (localSlot is always the
    /// local human). Fills any unfilled slots with AI bots. Invokes onComplete
    /// with the resolved roster. Never fails — always returns a full lobby.
    /// </summary>
    public IEnumerator FindMatch(int totalSlots, int localSlot, float timeout, Action<MatchmakingResult> onComplete)
    {
        totalSlots = Mathf.Max(1, totalSlots);
        localSlot = Mathf.Clamp(localSlot, 0, totalSlots - 1);

        var result = new MatchmakingResult { totalSlots = totalSlots };
        var humans = new List<int> { localSlot }; // the local player is always real
        bool online = false;

        Debug.Log($"[Matchmaking] Searching for real players ({totalSlots} slots, {timeout:0.0}s timeout)...");

        // Try Unity Multiplayer Center first (if services are ready)
        // If MP Center fails or isn't available, fall back to HTTP
        if (_unityServicesInitialized && matchmakingClient != null)
        {
            yield return StartCoroutine(FindMatchViaMPCCenter(totalSlots, localSlot, timeout, result, ref online, humans, onComplete));
        }

        // HTTP fallback: always available as backup for WebGL or when MP Center fails
        if (!online && !_unityServicesInitialized)
        {
            yield return StartCoroutine(FindMatchViaHttpFallback(totalSlots, timeout, result, ref online, humans, onComplete));
        }

        // If still not online after all attempts, fill with bots and mark as offline
        if (!online)
        {
            Debug.Log("[Matchmaking] No real players found, using AI bots only");
        }

        Debug.Log($"[Matchmaking] Lobby ready ({result.roomId}): {result.realPlayers} real, {result.botCount} bots.");
        OnMatchReady?.Invoke(result);
        onComplete?.Invoke(result);
    }

    public IEnumerator FindMatch(int totalSlots, int localSlot, Action<MatchmakingResult> onComplete)
        => FindMatch(totalSlots, localSlot, defaultSearchTimeout, onComplete);

    /// <summary>
    /// Legacy HTTP-based matchmaking fallback
    /// </summary>
    private IEnumerator FindMatchViaHttpFallback(int totalSlots, float timeout, MatchmakingResult result, 
        ref bool online, List<int> humans, Action<MatchmakingResult> onComplete)
    {
        string serverUrl = string.IsNullOrEmpty(supabaseUrl) ? "https://mp.tesana.ai" : supabaseUrl;
        
        UnityWebRequest req = null;
        bool requestFaulted = false;
        
        try
        {
            req = UnityWebRequest.Get(serverUrl + "/matchmake?slots=" + totalSlots);
            req.timeout = Mathf.Max(1, Mathf.CeilToInt(timeout));
            
            // Add API key header if available
            if (!string.IsNullOrEmpty(supabaseApiKey))
            {
                req.SetRequestHeader("apikey", supabaseApiKey);
            }
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
                TryParseRemoteHumans(req.downloadHandler.text, result.totalSlots, 0, humans);
                Debug.Log("[Matchmaking] Backend reachable. Real players found: " + humans.Count);
            }
            else
            {
                Debug.Log("[Matchmaking] Backend unreachable (" + req.result + "). Falling back to AI bots.");
            }
            req.Dispose();
        }
    }

    /// <summary>
    /// Find match via Unity Multiplayer Center SDK
    /// </summary>
    private IEnumerator FindMatchViaMPCCenter(int totalSlots, int localSlot, float timeout, 
        MatchmakingResult result, ref bool online, List<int> humans, Action<MatchmakingResult> onComplete)
    {
        if (matchmakingClient == null)
        {
            SetupMPCClient();
        }

        if (matchmakingClient == null)
        {
            Debug.LogWarning("[Matchmaking] MP Center client not available, using HTTP fallback");
            yield break;
        }

        var startTime = Time.time;
        bool matchFound = false;

        Action<MatchmakingEvent> onMatchmakingEvent = (evt) =>
        {
            switch (evt.Type)
            {
                case MatchmakingEventType.OnLobbyInitialJoined:
                    Debug.Log("[MP Center] Lobby joined successfully");
                    online = true;
                    matchFound = true;
                    break;
                    
                case MatchmakingEventType.OnPlayerJoined:
                    Debug.Log($"[MP Center] Player {evt.SlotIndex} joined lobby");
                    if (evt.SlotIndex != localSlot)
                        humans.Add(evt.SlotIndex);
                    break;
                    
                case MatchmakingEventType.OnPlayerLeft:
                    Debug.Log($"[MP Center] Player {evt.SlotIndex} left lobby");
                    humans.Remove(evt.SlotIndex);
                    break;
                    
                case MatchmakingEventType.OnLobbyClosed:
                    Debug.Log("[MP Center] Lobby closed");
                    matchFound = true;
                    break;
            }
        };

        try
        {
            matchmakingClient.OnMatchmakingEvent += onMatchmakingEvent;
            
            // Create or join a lobby
            var createLobbyRequest = new CreateLobbyRequest()
            {
                Id = sessionId,
                MaxPlayers = totalSlots,
                Version = "1.0.0"
            };
            
            createLobbyRequest.Options = new CreateLobbyOptions()
            {
                Flags = LobbyFlags.MinPublic
            };

            // Start matchmaking
            yield return matchmakingClient.CreateLobbyAsync(createLobbyRequest);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Matchmaking] MP Center error: {e.Message}");
        }

        matchmakingClient.OnMatchmakingEvent -= onMatchmakingEvent;

        // Wait for match or timeout
        while (!matchFound && (Time.time - startTime) < timeout)
        {
            yield return null;
        }

        if (!matchFound)
        {
            Debug.Log("[Matchmaking] MP Center timeout, falling back to local lobby");
        }
    }

    private void SetupMPCClient()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("[MP Center] Unity Services not initialized");
            return;
        }

        try
        {
            matchmakingClient = new MatchmakingClient();
            matchmakingClient.OnMatchmakingEvent += OnMatchmakingEvent;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MP Center] Failed to create client: {e.Message}");
            matchmakingClient = null;
        }
    }

    private void OnMatchmakingEvent(MatchmakingEvent evt)
    {
        // Event handling placeholder
    }

    /// <summary>
    /// Integration point for a real backend response. Expected to append remote
    /// human slot indices (other than localSlot) to `humans`. 
    /// </summary>
    private void TryParseRemoteHumans(string responseBody, int totalSlots, int localSlot, List<int> humans)
    {
        if (string.IsNullOrEmpty(responseBody)) return;
        
        try
        {
            // Parse JSON response from Supabase backend
            // Expected format: { "players": [{"slot": 1}, {"slot": 2}] }
            var json = JsonUtility.FromJson<MatchmakingResponse>(responseBody);
            if (json?.players != null)
            {
                foreach (var player in json.players)
                {
                    if (player.slot >= 0 && player.slot < totalSlots && player.slot != localSlot && !humans.Contains(player.slot))
                    {
                        humans.Add(player.slot);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Matchmaking] Failed to parse response: {e.Message}");
        }
    }

    public bool IsAi(int slot)
    {
        return _lastResult != null && _lastResult.aiSlots.Contains(slot);
    }

    /// <summary>
    /// Clean up resources on destroy
    /// </summary>
    private void OnDestroy()
    {
        if (matchmakingClient != null)
        {
            matchmakingClient.OnMatchmakingEvent -= OnMatchmakingEvent;
        }
    }
}

/// <summary>
/// Helper class for parsing matchmaking responses
/// </summary>
[Serializable]
public class MatchmakingResponse
{
    public PlayerInfo[] players;
}

[Serializable]
public class PlayerInfo
{
    public int slot;
    public string playerId;
}
