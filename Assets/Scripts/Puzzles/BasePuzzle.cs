using System;
using UnityEngine;

// Base class for all UI/Canvas overlay puzzles. The overlay prefabs reference
// concrete subclasses (e.g. ComboRushPuzzle) and serialize the four fields
// below plus their own grid/UI references.
//
// Lifecycle used by the match managers:
//   InitializePuzzle(difficulty, seed)  -> builds the board (BuildBoard)
//   StartPuzzle()                       -> starts the fuse timer & accepts input
//   (player solves)                     -> Solve() raises OnSolved
//   (timer hits zero)                   -> raises OnTimedOut (bomb explodes)
//
// Brain-vs-Brain hooks: ApplyAttack(AttackType) and AutoSolveChunk().
public abstract class BasePuzzle : MonoBehaviour
{
    [Header("Base Puzzle")]
    public string puzzleName;
    public float puzzleTime = 30f;
    public bool isSolved = false;
    public bool isTimedOut = false;

    [HideInInspector] public bool inputEnabled = true;
    [HideInInspector] public int difficulty = 1;

    public event Action<BasePuzzle> OnSolved;
    public event Action<BasePuzzle> OnTimedOut;

    protected System.Random rng = new System.Random();
    protected float timeRemaining;
    protected bool isRunning;

    private float freezeTimer;

    // ---- Read-only state for HUD binding ----
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public float NormalizedTimeRemaining => (puzzleTime > 0f) ? Mathf.Clamp01(timeRemaining / puzzleTime) : 0f;
    public bool IsRunning => isRunning;

    // ---- Lifecycle ----

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
    }

    protected virtual void OnPuzzleSolvedInternal() { }

    protected void TimeOut()
    {
        if (isSolved || isTimedOut) return;
        isTimedOut = true;
        isRunning = false;
        OnTimedOut?.Invoke(this);
    }

    // ---- Brain VS Brain hooks ----

    // Default attack handling. Subclasses may override Scramble for a real
    // board shuffle; Freeze/Blur/InvertControls have generic base behaviour.
    public virtual void ApplyAttack(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Freeze:
                freezeTimer = Mathf.Max(freezeTimer, 2.5f);
                break;
            case AttackType.Scramble:
                if (!isSolved) BuildBoard();
                break;
            case AttackType.Blur:
            case AttackType.InvertControls:
                // Visual/control modifiers are subclass-specific; no-op by default.
                break;
        }
    }

    // Advance the puzzle toward a solution by one step (bots & AutoSolve defense).
    public virtual void AutoSolveChunk() { }

    protected bool CanAcceptInput()
    {
        return isRunning && inputEnabled && !isSolved && !isTimedOut && freezeTimer <= 0f;
    }
}
