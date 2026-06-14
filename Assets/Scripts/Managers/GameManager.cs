using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public enum GameMode { Ranked4PlayerFFA, BrainVSBrain }
    public GameMode currentMode;
    
    public int targetPlayerCount = 4;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // PuzzleAssetLibrary removed during reconciliation — puzzle prefabs are
            // wired directly on ArenaManager / RankedMatchManager instead.
            InitializeMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMusic()
    {
        MusicManager music = FindAnyObjectByType<MusicManager>();
        if (music == null)
        {
            GameObject go = new GameObject("MusicManager");
            music = go.AddComponent<MusicManager>();
        }

        // Load clips from Resources or set them manually if they were serialized.
        // For now, let's assume we can find them in the project.
        // In a real scenario, we'd use a ScriptableObject or Resources.LoadAll.
        music.dashboardMusic.AddRange(Resources.LoadAll<AudioClip>("Audio/dashboard"));
        music.arenaMusic.AddRange(Resources.LoadAll<AudioClip>("Audio/arena"));
        
        music.PlayDashboardMusic();
    }

    public void StartGame(GameMode mode)
    {
        currentMode = mode;
        targetPlayerCount = (mode == GameMode.Ranked4PlayerFFA) ? 4 : 2;
        
        if (MusicManager.Instance != null) MusicManager.Instance.PlayArenaMusic();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void ReturnToMenu()
    {
        if (MusicManager.Instance != null) MusicManager.Instance.PlayDashboardMusic();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
