using UnityEngine;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public enum Mode { NONE, FFA, BVB }

    [Header("Audio Clips")]
    public AudioClip musicMenu;
    public AudioClip musicGameplay;
    public AudioClip jingleWin;
    public AudioClip jingleLose;
    public AudioClip sfxBombTick;
    public AudioClip sfxBombPass;
    public AudioClip sfxBombExplosion;
    public AudioClip sfxPuzzleSolve;
    public AudioClip sfxPuzzlePlace;
    public AudioClip sfxPuzzleRotate;
    public AudioClip sfxPuzzleError;
    public AudioClip sfxUiClick;
    public AudioClip sfxUiHover;
    public AudioClip sfxUiConfirm;

    [Header("Audio Playlists")]
    public List<AudioClip> arenaPlaylist = new List<AudioClip>();
    public List<AudioClip> dashboardPlaylist = new List<AudioClip>();

    [Header("Rival Data")]
    public Color[] rivalColors = {
        new Color(1.0f, 0.18f, 0.78f),   // magenta
        new Color(0.0f, 0.90f, 1.0f),    // cyan
        new Color(0.61f, 1.0f, 0.18f),   // lime
        new Color(1.0f, 0.54f, 0.12f),   // orange
    };

    public string[] rivalNames = { "GLITCH", "CIPHER", "VERTEX", "AXIOM" };

    public string[] rivalTitles = {
        "The Glitchsmith",
        "The Code Breaker",
        "The Geometer",
        "The Strategist",
    };

    public Sprite[] rivalSprites = new Sprite[4];

    [Header("Current Session")]
    public Mode currentMode = Mode.NONE;
    public int localPlayerIndex = 0;

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

    public void ResetState()
    {
        currentMode = Mode.NONE;
        localPlayerIndex = 0;
    }
}
