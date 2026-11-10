using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Enhanced base class for all puzzles with common functionality to reduce duplication.
/// Provides standardized audio, visual effects, input handling, and lifecycle management.
/// </summary>
public abstract class BasePuzzle : MonoBehaviour
{
    [Header("Base Puzzle")]
    public string puzzleName;
    public float puzzleTime = 30f;
    public bool isSolved = false;
    public bool isTimedOut = false;

    [HideInInspector] public bool inputEnabled = true;
    [HideInInspector] public bool allowAutomatedInput = false;
    [HideInInspector] public int difficulty = 1;

    public event Action<BasePuzzle> OnSolved;
    public event Action<BasePuzzle> OnTimedOut;

    protected System.Random rng = new System.Random();
    protected float timeRemaining;
    protected bool isRunning;

    protected float freezeTimer;

    // ---- Read-only state for HUD binding ----
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public float NormalizedTimeRemaining => (puzzleTime > 0f) ? Mathf.Clamp01(timeRemaining / puzzleTime) : 0f;
    public bool IsRunning => isRunning;

    // Common audio references (can be overridden by subclasses)
    protected virtual AudioManager AudioManager => AudioManager.Instance;
    protected virtual GameState GameState => GameState.Instance;

    // Common visual effect settings
    [Header("Visual Effects (override in subclass if needed)")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    // ---- Lifecycle ----
    public virtual void Setup(int difficulty)
    {
        Setup(difficulty, Environment.TickCount);
    }

    public virtual void Setup(int difficulty, int seed)
    {
        this.difficulty = Mathf.Max(1, difficulty);
        rng = new System.Random(seed);
        isSolved = false;
        isTimedOut = false;
        isRunning = false;
        freezeTimer = 0f;
        timeRemaining = puzzleTime;
        BuildBoard();
    }

    public virtual void InitializePuzzle()
    {
        InitializePuzzle(1, Environment.TickCount);
    }

    public virtual void InitializePuzzle(int difficulty, int seed)
    {
        this.difficulty = Mathf.Max(1, difficulty);
        rng = new System.Random(seed);
        isSolved = false;
        isTimedOut = false;
        isRunning = false;
        freezeTimer = 0f;
        timeRemaining = puzzleTime;
        BuildBoard();
    }

    public virtual void StartPuzzle()
    {
        timeRemaining = puzzleTime;
        isRunning = true;
        OnTimerTick(1f, timeRemaining);
    }

    public virtual void StopPuzzle()
    {
        isRunning = false;
    }

    // Subclasses build their grid here using their serialized references + rng.
    protected abstract void BuildBoard();

    // ---- Timing ----
    protected virtual void Update()
    {
        if (!isRunning || isSolved || isTimedOut) return;

        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.deltaTime;
            return; // frozen by an attack: pause the fuse
        }

        if (puzzleTime > 0f)
        {
            timeRemaining -= Time.deltaTime;
            float n = Mathf.Clamp01(timeRemaining / puzzleTime);
            OnTimerTick(n, Mathf.Max(0f, timeRemaining));
            if (timeRemaining <= 0f) TimeOut();
        }
    }

    // Override to drive a timer bar / countdown label.
    protected virtual void OnTimerTick(float normalized, float remaining) { }

    // ---- Resolution ----
    protected void Solve()
    {
        if (isSolved || isTimedOut) return;
        isSolved = true;
        isRunning = false;
        OnPuzzleSolvedInternal();
        OnSolved?.Invoke(this);
        PlaySolveEffect();
    }

    protected virtual void OnPuzzleSolvedInternal() { }

    protected void TimeOut()
    {
        if (isSolved || isTimedOut) return;
        isTimedOut = true;
        isRunning = false;
        OnTimedOut?.Invoke(this);
        PlayTimeoutEffect();
    }

    // Common effect methods
    protected void PlaySolveEffect()
    {
        // Play solve sound
        if (AudioManager != null && GameState != null && GameState.Instance != null)
        {
            AudioManager.PlaySfx(GameState.Instance.sfxPuzzleSolve, 0.7f);
        }
        
        // Optional: add visual celebration effect
        // Can be overridden by subclasses for specific celebrations
    }

    protected void PlayTimeoutEffect()
    {
        // Play timeout sound
        if (AudioManager != null && GameState != null && GameState.Instance != null)
        {
            AudioManager.PlaySfx(GameState.Instance.sfxPuzzleError, 0.7f);
        }
        
        // Optional: add visual failure effect
        // Can be overridden by subclasses for specific failure effects
    }

    protected void PlayErrorSound()
    {
        if (AudioManager != null && GameState != null && GameState.Instance != null)
        {
            AudioManager.PlaySfx(GameState.Instance.sfxPuzzleError, 0.5f);
        }
    }

    protected void PlayRotateSound()
    {
        if (AudioManager != null && GameState != null && GameState.Instance != null)
        {
            AudioManager.PlaySfx(GameState.Instance.sfxPuzzleRotate, 0.5f);
        }
    }

    // Standardized input handling
    protected bool CanAcceptInput()
    {
        return isRunning && (inputEnabled || allowAutomatedInput) && !isSolved && !isTimedOut && freezeTimer <= 0f;
    }

    // Standardized visual effects
    protected IEnumerator FlashImage(Image image, Color color, float duration, int flashes = 3)
    {
        Color originalColor = image.color;
        for (int i = 0; i < flashes; i++)
        {
            image.color = color;
            yield return new WaitForSeconds(duration / (flashes * 2));
            image.color = originalColor;
            yield return new WaitForSeconds(duration / (flashes * 2));
        }
    }

    protected IEnumerator ShakeTransform(Transform transform, float duration, float magnitude)
    {
        if (transform == null) yield break;
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            float shake = Mathf.Lerp(magnitude, 0f, percent);
            
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * shake;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * shake;
            
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        
        transform.localPosition = originalPos;
    }

    // Helper for creating UI elements with consistent setup
    protected GameObject CreateUIElement(GameObject prefab, Transform parent, string name = null)
    {
        GameObject go = Instantiate(prefab, parent);
        if (!string.IsNullOrEmpty(name))
            go.name = name;
        return go;
    }

    // Standardized cleanup
    protected virtual void OnDestroy()
    {
        // Clean up any coroutines or references if needed
        // Subclasses can override for specific cleanup
    }

    // ---- Brain VS Brain hooks ----
    public virtual void ApplyAttack(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Freeze:
                freezeTimer = Mathf.Max(freezeTimer, 2.5f);
                break;
            case AttackType.Scramble:
                if (!isSolved) RebuildBoard();
                break;
            case AttackType.Blur:
            case AttackType.InvertControls:
                // Visual/control modifiers are subclass-specific; no-op by default.
                break;
        }
    }

    /// <summary>
    /// Rebuilds the board (used for scramble attacks).
    /// Default implementation calls BuildBoard() after clearing.
    /// Subclasses can override for more efficient partial rebuilds.
    /// </summary>
    protected virtual void RebuildBoard()
    {
        ClearBoard();
        BuildBoard();
    }

    /// <summary>
    /// Clears the board (removes all UI elements).
    /// Subclasses should override to implement their specific clearing logic.
    /// </summary>
    protected virtual void ClearBoard()
    {
        // Default implementation does nothing - subclasses should override
    }

    // Advance the puzzle toward a solution by one step (bots & AutoSolve defense).
    public virtual void AutoSolveChunk() { }

    // Helper method for consistent randomization
    protected T RandomFromArray<T>(T[] array)
    {
        if (array == null || array.Length == 0) return default;
        return array[rng.Next(array.Length)];
    }

    protected int RandomRange(int min, int maxExclusive)
    {
        return rng.Next(min, maxExclusive);
    }

    protected float RandomRange(float min, float max)
    {
        return (float)rng.NextDouble() * (max - min) + min;
    }
}