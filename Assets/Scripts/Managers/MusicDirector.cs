using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Persistent director that keeps the dashboard/menu music rotating on every menu
// screen. It listens for scene loads and triggers AudioManager.PlayMenuMusic() on
// any menu scene. The Match scene is intentionally left alone — the match managers
// start the arena music themselves via AudioManager.PlayArenaMusic().
public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance;

    [Header("Menu Scenes (rotate dashboard music)")]
    public List<string> menuScenes = new List<string> { "Main", "ModeSelect", "Profile", "Store", "SeasonPass" };

    // Spawn the director before the first scene loads so it is present in every
    // play session regardless of which scene starts the game.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("MusicDirector");
            go.AddComponent<MusicDirector>();
        }
    }

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

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        // Handle the scene that is already active at startup. The sceneLoaded event
        // for the very first scene can fire before this object subscribes, so we
        // trigger it explicitly here (PlayMenuMusic is guarded against duplicates).
        HandleScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        HandleScene(scene.name);
    }

    private void HandleScene(string sceneName)
    {
        if (menuScenes.Contains(sceneName))
        {
            EnsureAudioManager();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();
        }
        // Non-menu scenes (e.g. Match) are left alone so the match managers can
        // control the arena music.
    }

    private void EnsureAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
    }
}