using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ArenaManager : MonoBehaviour
{
public static ArenaManager Instance { get; private set; }

    [Header("Prefabs & References")]
    public GameObject podiumPrefab;
    public GameObject bombPrefab;
    public GameObject pipePulsePrefab;
    public GameObject circuitScramblePrefab;
    public GameObject collapseGridPrefab;
    public GameObject comboRushPrefab;
    public GameObject mazeRushPrefab;
    public GameObject mirrorBeamPrefab;
    public GameObject scoreAttackPrefab;
    public GameObject explosionPrefab;
    public UIDocument uiDocument;
    public Transform puzzleSlot;

    private List<Podium> podiums = new List<Podium>();
    private Bomb bomb;
    private PuzzleBase currentPuzzle;
    private bool[] playersAlive = { true, true, true, true };
    private int currentHolder = -1;
    private int roundNumber = 0;
    private float timerRemaining = 22f;
    private bool timerRunning = false;
    private bool gameOver = false;

    private ProgressBar timerBar;
    private Label hudLabel;
    private Label statusLabel;
    private Label clutchLabel;
    private List<AIBot> bots = new List<AIBot>();

    private const float CLUTCH_THRESHOLD = 3.0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupUI();
        InitializeArena();
        if (GameState.Instance != null)
        {
            if (AudioManager.Instance != null && GameState.Instance.arenaPlaylist.Count > 0)
            {
                AudioManager.Instance.PlayPlaylist(GameState.Instance.arenaPlaylist, true, 0.7f);
            }
            StartNextRound(Random.Range(0, 4));
        }
    }

    private void SetupUI()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        timerBar = root.Q<ProgressBar>("timerBar");
        hudLabel = root.Q<Label>("hudLabel");
        statusLabel = root.Q<Label>("statusLabel");
        clutchLabel = root.Q<Label>("clutchLabel");
        
        if (timerBar != null)
        {
            timerBar.lowValue = 0;
            timerBar.highValue = 22;
        }
    }

    private void InitializeArena()
    {
        Vector3[] offsets = {
            new Vector3(-6, 3, 0),
            new Vector3(6, 3, 0),
            new Vector3(-6, -3, 0),
            new Vector3(6, -3, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject go = Instantiate(podiumPrefab, offsets[i], Quaternion.identity);
            go.name = "Podium_" + i;
            Podium p = go.GetComponent<Podium>();
            if (GameState.Instance != null) {
                p.Setup(i, GameState.Instance.rivalNames[i], GameState.Instance.rivalTitles[i], 
                        GameState.Instance.rivalColors[i], GameState.Instance.rivalSprites[i]);
            }
            podiums.Add(p);
            
            AIBot bot = go.AddComponent<AIBot>();
            bots.Add(bot);
        }

        GameObject bombGo = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity);
        bomb = bombGo.GetComponent<Bomb>();
        bomb.SetLit(false);
    }

    public void StartNextRound(int holder)
    {
        if (gameOver) return;

        roundNumber++;
        currentHolder = holder;
        
        Vector3 targetPos = podiums[holder].transform.position + Vector3.up * 1.5f;
        bomb.MoveTo(targetPos, 0.6f);
        bomb.SetLit(true);

        timerRemaining = Mathf.Max(10f, 22f - (roundNumber - 1) * 1.2f);
        if (timerBar != null) timerBar.highValue = timerRemaining;
        timerRunning = true;

        if (hudLabel != null) hudLabel.text = $"ROUND {roundNumber} - {GameState.Instance.rivalNames[holder]} HAS THE BOMB";
        
        ShowStatus($"ROUND {roundNumber}", GameState.Instance.rivalColors[holder]);
        SpawnPuzzle(1 + roundNumber / 2);
    }

    private void SpawnPuzzle(int difficulty)
    {
        if (currentPuzzle != null) Destroy(currentPuzzle.gameObject);
        
        GameObject[] pool = { pipePulsePrefab, circuitScramblePrefab, collapseGridPrefab, comboRushPrefab, mazeRushPrefab, mirrorBeamPrefab, scoreAttackPrefab };
        List<GameObject> activePool = new List<GameObject>();
        foreach(var p in pool) if(p != null) activePool.Add(p);
        
        if(activePool.Count == 0) return;

        GameObject prefab = activePool[Random.Range(0, activePool.Count)];
        GameObject go = Instantiate(prefab, puzzleSlot);
        currentPuzzle = go.GetComponent<PuzzleBase>();
        currentPuzzle.Setup(difficulty);
        currentPuzzle.OnSolved += (score) => OnPuzzleSolved();
        
        currentPuzzle.inputEnabled = (currentHolder == GameState.Instance.localPlayerIndex);
        
        if (!currentPuzzle.inputEnabled)
        {
            bots[currentHolder].BindPuzzle(currentPuzzle, 0.8f);
        }
    }

    private void OnPuzzleSolved()
    {
        if (gameOver) return;
        timerRunning = false;
        
        foreach(var bot in bots) bot.Stop();

        if (currentHolder == GameState.Instance.localPlayerIndex && timerRemaining <= CLUTCH_THRESHOLD)
        {
            ShowClutch();
        }

        ShowStatus($"{GameState.Instance.rivalNames[currentHolder]} SOLVED IT!", GameState.Instance.rivalColors[currentHolder]);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.7f);

        Invoke(nameof(DelayedNextRound), 1.0f);
    }

    private void ShowClutch()
    {
        if (clutchLabel != null)
        {
            clutchLabel.style.opacity = 1;
            StartCoroutine(FadeOutElement(clutchLabel, 1.5f));
        }
    }

    private void DelayedNextRound()
    {
        StartNextRound(PickRandomAlive(currentHolder));
    }

    private void Update()
    {
        if (timerRunning && !gameOver)
        {
            timerRemaining -= Time.deltaTime;
            if (timerBar != null)
            {
                timerBar.value = timerRemaining;
            }

            if (timerRemaining <= 0)
            {
                timerRunning = false;
                ExplodeHolder();
            }
        }
    }

    private void ExplodeHolder()
    {
        if (gameOver) return;
        playersAlive[currentHolder] = false;
        podiums[currentHolder].SetEliminated(true);
        ShowStatus($"{GameState.Instance.rivalNames[currentHolder]} ELIMINATED!", Color.red);
        
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, podiums[currentHolder].transform.position, Quaternion.identity);
            exp.GetComponent<ExplosionEffect>().Play(0.6f);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(GameState.Instance.sfxBombExplosion, 0.8f);

        ShakeCamera(0.4f, 0.2f);

        if (CountAlive() <= 1)
        {
            DeclareWinner();
        }
        else
        {
            Invoke(nameof(StartRoundAfterExplosion), 2.0f);
        }
    }

    private void StartRoundAfterExplosion()
    {
        StartNextRound(PickRandomAlive(currentHolder));
    }

    private int PickRandomAlive(int exclude)
    {
        List<int> pool = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (playersAlive[i] && i != exclude) pool.Add(i);
        }
        if (pool.Count == 0) return -1;
        return pool[Random.Range(0, pool.Count)];
    }

    private int CountAlive()
    {
        int count = 0;
        foreach (bool a in playersAlive) if (a) count++;
        return count;
    }

    private void DeclareWinner()
    {
        gameOver = true;
        int winner = -1;
        for (int i = 0; i < 4; i++) if (playersAlive[i]) winner = i;
        
        if (winner != -1)
            ShowStatus($"{GameState.Instance.rivalNames[winner]} WINS!", GameState.Instance.rivalColors[winner]);
        else
            ShowStatus("DRAW!", Color.white);
    }

    private void ShowStatus(string text, Color color)
    {
        if (statusLabel != null)
        {
            statusLabel.text = text;
            statusLabel.style.color = color;
            statusLabel.style.opacity = 1;
            StartCoroutine(FadeOutElement(statusLabel, 1.5f));
        }
    }

    private IEnumerator FadeOutElement(Label label, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            label.style.opacity = 1f - (elapsed / 0.5f);
            yield return null;
        }
        label.style.opacity = 0;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }
}
