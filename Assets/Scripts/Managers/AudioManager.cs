using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private Coroutine playlistRoutine;

    // True while the rotating dashboard/menu playlist is the active music. Used so
    // moving between menu scenes does not restart the music.
    private bool menuMusicActive;

    private Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
            LoadClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Guarantees usable AudioSources even when this manager is spawned at runtime
    // (e.g. by MusicDirector) rather than wired up in a scene.
    private void EnsureSources()
    {
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void LoadClips()
    {
        // Load all audio from Resources/Audio
        AudioClip[] dashboard = Resources.LoadAll<AudioClip>("Audio/dashboard");
        foreach (var clip in dashboard) musicClips["menu_" + clip.name] = clip;

        AudioClip[] arena = Resources.LoadAll<AudioClip>("Audio/arena");
        foreach (var clip in arena) musicClips["arena_" + clip.name] = clip;
    }

    public void PlayMenuMusic()
    {
        // Already rotating the menu playlist — don't restart between menu scenes.
        if (menuMusicActive && playlistRoutine != null) return;

        List<AudioClip> menuTracks = new List<AudioClip>();
        foreach (var pair in musicClips)
            if (pair.Key.StartsWith("menu_")) menuTracks.Add(pair.Value);

        if (menuTracks.Count == 0) return;

        PlayPlaylist(menuTracks, true, 0.8f);
        menuMusicActive = true;
    }

    public void PlayArenaMusic()
    {
        // Randomly pick an arena track
        List<string> arenaTracks = new List<string>();
        foreach (var key in musicClips.Keys) if (key.StartsWith("arena_")) arenaTracks.Add(key);
        
        if (arenaTracks.Count > 0)
        {
            string track = arenaTracks[Random.Range(0, arenaTracks.Count)];
            PlayMusic(track);
        }
    }

    private void PlayMusic(string key)
    {
        if (musicClips.ContainsKey(key))
        {
            // A single keyed track is not the menu playlist.
            StopPlaylist();
            menuMusicActive = false;

            if (musicSource.clip == musicClips[key] && musicSource.isPlaying) return;
            
            musicSource.clip = musicClips[key];
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ---- Restored API used across the arena, puzzles and menus ----

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (clip == null || musicSource == null) return;
        StopPlaylist();
        menuMusicActive = false;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlayPlaylist(List<AudioClip> clips, bool loop, float volume)
    {
        if (clips == null || clips.Count == 0 || musicSource == null) return;
        StopPlaylist();
        menuMusicActive = false;
        musicSource.volume = volume;
        playlistRoutine = StartCoroutine(PlaylistRoutine(clips, loop));
    }

    private IEnumerator PlaylistRoutine(List<AudioClip> clips, bool loop)
    {
        do
        {
            for (int i = 0; i < clips.Count; i++)
            {
                AudioClip clip = clips[i];
                if (clip == null) continue;
                musicSource.clip = clip;
                musicSource.loop = false;
                musicSource.Play();
                while (musicSource.isPlaying) yield return null;
            }
        }
        while (loop);
        playlistRoutine = null;
    }

    private void StopPlaylist()
    {
        if (playlistRoutine != null)
        {
            StopCoroutine(playlistRoutine);
            playlistRoutine = null;
        }
    }

    public void StopMusic()
    {
        StopPlaylist();
        menuMusicActive = false;
        musicSource.Stop();
    }
}
