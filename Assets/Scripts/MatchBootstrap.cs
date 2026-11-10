using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lives in the Match scene. Reads GameState.currentMode and boots the right
// match controller:
//   FFA -> RankedMatchManager (random holder, overlay puzzle, bomb passing)
//   BVB -> BrainVSBrainManager (1v1 race; each side gets its own overlay)
// The 8 overlay prefabs are assigned in the inspector (set when the scene is
// built). A Canvas + EventSystem are ensured at runtime.
public class MatchBootstrap : MonoBehaviour
{
    [Header("Puzzle Overlays (one per type)")]
    public List<GameObject> overlayPrefabs = new List<GameObject>();

    [Header("Refs")]
    public Canvas canvas;
    public GameObject bombPrefab;

    [Header("HUD Roots (toggled by mode)")]
    public GameObject ffaHudRoot;
    public GameObject bvbHudRoot;

    [Header("Bot Tuning (BVB opponent)")]
    public float bvbBotInterval = 1.1f;

    [Header("Matchmaking")]
    public float searchTimeout = 6f;

    // Every match first searches for real players via NetManager, then fills any
    // unfilled lobby slots with AI bots before booting the mode.
    private IEnumerator Start()
    {
        EnsureCanvas();
        EnsureNetManager();

        GameState.Mode mode = GameState.Instance != null ? GameState.Instance.currentMode : GameState.Mode.FFA;
        if (mode == GameState.Mode.NONE) mode = GameState.Mode.FFA;
        bool isBvb = mode == GameState.Mode.BVB;

        int totalSlots = isBvb ? 2 : 4;
        int localSlot = GameState.Instance != null ? GameState.Instance.localPlayerIndex : 0;
        localSlot = Mathf.Clamp(localSlot, 0, totalSlots - 1);

        // Hide gameplay HUDs during the search.
        if (ffaHudRoot != null) ffaHudRoot.SetActive(false);
        if (bvbHudRoot != null) bvbHudRoot.SetActive(false);

        TextMeshProUGUI mainText, subText;
        var overlay = BuildSearchOverlay(out mainText, out subText);
        mainText.text = "SEARCHING FOR PLAYERS...";
        subText.text = isBvb ? "1v1 Brain VS Brain" : "4-Player Ranked FFA";

        MatchmakingResult mm = null;
        yield return NetManager.Instance.FindMatch(totalSlots, localSlot, searchTimeout, r => mm = r);
        if (mm == null)
            mm = new MatchmakingResult { totalSlots = totalSlots, realPlayers = 1, botCount = totalSlots - 1, humanSlots = new List<int> { localSlot } };

        mainText.text = mm.online ? "MATCH FOUND" : "NO PLAYERS ONLINE";
        subText.text = mm.realPlayers + " real  -  " + mm.botCount + (mm.botCount == 1 ? " AI bot" : " AI bots");
        yield return new WaitForSeconds(1.2f);
        if (overlay != null) Destroy(overlay);

        // Reveal the correct HUD and boot the mode with the resolved roster.
        if (ffaHudRoot != null) ffaHudRoot.SetActive(!isBvb);
        if (bvbHudRoot != null) bvbHudRoot.SetActive(isBvb);

        int seed = System.Environment.TickCount;
        if (isBvb) BootBrainVsBrain(seed, mm);
        else BootFFA(seed, mm);
    }

    // ---- 4-Player Ranked Free-For-All ----

    private void BootFFA(int seed, MatchmakingResult mm)
    {
        var go = new GameObject("RankedMatchManager");
        var mgr = go.AddComponent<RankedMatchManager>();
        mgr.puzzlePrefabs = new List<GameObject>(overlayPrefabs);
        mgr.matchCanvas = canvas;
        mgr.bombPrefab = bombPrefab;
        mgr.InitializeMatch(CreateTestPlayers(4, mm != null ? mm.humanSlots : null), seed, mm != null ? mm.humanSlots : null);
        Debug.Log("MatchBootstrap: started 4-Player Ranked FFA.");
    }

    // ---- Brain VS Brain (1v1) ----

    private void BootBrainVsBrain(int seed, MatchmakingResult mm)
    {
        var go = new GameObject("BrainVSBrainManager");
        var bvb = go.AddComponent<BrainVSBrainManager>();
        bvb.realPlayers = mm != null ? mm.realPlayers : 1;
        bvb.opponentIsBot = mm == null || mm.realPlayers < 2;
        bvb.canvas = canvas;
        bvb.overlayPrefabs = new List<GameObject>(overlayPrefabs);
        bvb.InitializeMatch(seed);
        Debug.Log("MatchBootstrap: started Brain VS Brain.");
    }
    
    private List<Player> CreateTestPlayers(int count, List<int> humanSlots)
    {
        var players = new List<Player>();
        int localSlot = GameState.Instance != null ? GameState.Instance.localPlayerIndex : 0;
        for (int i = 0; i < count; i++)
        {
            bool isHuman = humanSlots == null || humanSlots.Contains(i);
            var player = new Player(i, i == localSlot ? "YOU" : $"Player{i + 1}", i == localSlot);
            player.isBot = !isHuman;
            players.Add(player);
        }
        return players;
    }

    // ---- NetManager / Search overlay ----

    private void EnsureNetManager()
    {
        if (NetManager.Instance == null)
            new GameObject("NetManager").AddComponent<NetManager>();
    }

    private GameObject BuildSearchOverlay(out TextMeshProUGUI mainText, out TextMeshProUGUI subText)
    {
        var root = new GameObject("MatchmakingOverlay", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.09f, 0.96f);

        mainText = CreateOverlayText(root.transform, "Main", 54, new Vector2(0, 40), new Color(0f, 0.9f, 1f));
        mainText.fontStyle = FontStyles.Bold;
        subText = CreateOverlayText(root.transform, "Sub", 32, new Vector2(0, -40), new Color(0.8f, 0.85f, 0.95f));
        return root;
    }

    private TextMeshProUGUI CreateOverlayText(Transform parent, string name, int size, Vector2 pos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1000, 90);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = ""; t.fontSize = size; t.color = color;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        return t;
    }

    // ---- Canvas / EventSystem ----

    private void EnsureCanvas()
    {
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("MatchCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
        }
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}