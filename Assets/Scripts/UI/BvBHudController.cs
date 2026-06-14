using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// Drives the Brain VS Brain HUD: two score meters (you vs opponent), a centre
// notification banner for attacks/defenses, and a result banner. Binds to
// BrainVSBrainManager events.
public class BvBHudController : MonoBehaviour
{
    [Header("Score Meters")]
    public Image playerMeterFill;     // Image type = Filled (Vertical)
    public Image opponentMeterFill;
    public TMP_Text playerScoreText;
    public TMP_Text opponentScoreText;
    public TMP_Text playerNameText;
    public TMP_Text opponentNameText;

    [Header("Notifications")]
    public TMP_Text notificationText; // flashes attack/defense messages

    [Header("Result")]
    public GameObject resultBanner; // legacy banner (superseded by leaderboard)
    public TMP_Text resultText;

    [Header("Leaderboard / Restart")]
    public MatchLeaderboard leaderboardPrefab;
    public float restartDelay = 5f;
    public string matchSceneName = "Match";

    private BrainVSBrainManager match;
    private Coroutine notifyRoutine;
    private MatchLeaderboard leaderboard;
    private bool restarting;

    private IEnumerator Start()
    {
        if (resultBanner != null) resultBanner.SetActive(false);
        if (notificationText != null) notificationText.text = "";

        float timeout = 10f;
        while (BrainVSBrainManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        match = BrainVSBrainManager.Instance;
        if (match == null)
        {
            Debug.LogWarning("BvBHudController: no BrainVSBrainManager found.");
            yield break;
        }

        if (playerNameText != null)
            playerNameText.text = ProfileManager.Instance != null
                ? ProfileManager.Instance.currentProfile.playerName + " (YOU)"
                : "YOU";
        if (opponentNameText != null) opponentNameText.text = "OPPONENT";

        match.OnScoreChanged += RefreshMeters;
        match.OnNotification += ShowNotification;
        match.OnMatchEnded += ShowResult;

        RefreshMeters();
    }

    private void OnDestroy()
    {
        if (match != null)
        {
            match.OnScoreChanged -= RefreshMeters;
            match.OnNotification -= ShowNotification;
            match.OnMatchEnded -= ShowResult;
        }
    }

    private void RefreshMeters()
    {
        if (match == null) return;
        float target = Mathf.Max(1, match.targetScore);
        float pNorm = Mathf.Clamp01(match.playerPoints / target);
        float oNorm = Mathf.Clamp01(match.opponentPoints / target);

        if (playerMeterFill != null) playerMeterFill.fillAmount = pNorm;
        if (opponentMeterFill != null) opponentMeterFill.fillAmount = oNorm;
        if (playerScoreText != null) playerScoreText.text = $"{match.playerPoints} / {match.targetScore}";
        if (opponentScoreText != null) opponentScoreText.text = $"{match.opponentPoints} / {match.targetScore}";
    }

    private void ShowNotification(string message, Color color)
    {
        if (notificationText == null) return;
        if (notifyRoutine != null) StopCoroutine(notifyRoutine);
        notifyRoutine = StartCoroutine(FlashNotification(message, color));
    }

    private IEnumerator FlashNotification(string message, Color color)
    {
        notificationText.text = message;
        notificationText.color = color;
        float t = 2f;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            Color c = notificationText.color;
            c.a = Mathf.Clamp01(t / 2f);
            notificationText.color = c;
            yield return null;
        }
        notificationText.text = "";
    }

    private void ShowResult(bool playerWon)
    {
        if (resultBanner != null) resultBanner.SetActive(false);

        ShowLeaderboard(playerWon);

        if (!restarting)
        {
            restarting = true;
            StartCoroutine(RestartAfterDelay());
        }
    }

    private void ShowLeaderboard(bool playerWon)
    {
        if (leaderboardPrefab == null || match == null) return;

        var lib = CosmeticLibrary.Instance;
        string localName = ProfileManager.Instance != null
            ? ProfileManager.Instance.currentProfile.playerName + " (YOU)"
            : "YOU";

        string winnerName = playerWon ? localName : "OPPONENT";
        string loserName = playerWon ? "OPPONENT" : localName;
        int winnerPts = playerWon ? match.playerPoints : match.opponentPoints;
        int loserPts = playerWon ? match.opponentPoints : match.playerPoints;

        var entries = new List<LeaderboardEntry>
        {
            new LeaderboardEntry
            {
                playerName = winnerName,
                scoreLabel = "SCORE: " + winnerPts,
                placeLabel = "1ST",
                emblem = lib != null ? lib.EmblemForRank(2) : null // gold
            },
            new LeaderboardEntry
            {
                playerName = loserName,
                scoreLabel = "SCORE: " + loserPts,
                placeLabel = "2ND",
                emblem = lib != null ? lib.EmblemForRank(1) : null // silver
            }
        };

        if (leaderboard == null)
        {
            Canvas canvas = match.canvas != null ? match.canvas : FindAnyObjectByType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;
            leaderboard = Instantiate(leaderboardPrefab, parent);
            RectTransform rt = leaderboard.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }
        leaderboard.Show("RESULTS", entries);
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(matchSceneName);
    }
}
