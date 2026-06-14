using UnityEngine;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource musicSource;
    
    [Header("Playlists")]
    public List<AudioClip> dashboardMusic = new List<AudioClip>();
    public List<AudioClip> arenaMusic = new List<AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayDashboardMusic()
    {
        if (dashboardMusic.Count == 0) return;
        PlayClip(dashboardMusic[Random.Range(0, dashboardMusic.Count)]);
    }

    public void PlayArenaMusic()
    {
        if (arenaMusic.Count == 0) return;
        PlayClip(arenaMusic[Random.Range(0, arenaMusic.Count)]);
    }

    private void PlayClip(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
}
