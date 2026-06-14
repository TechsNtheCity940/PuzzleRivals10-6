using System;
using System.Collections.Generic;
using UnityEngine;

// The 8 puzzle categories used by the ranked arena. These map 1:1 to the
// overlay prefabs authored in the project.
public enum RankedArenaPuzzleType
{
    GridLogic,        // CollapseGridOverlay
    PatternRecall,    // MirrorMazeOverlay
    PipePulse,        // PipePulseOverlay
    ComboRush,        // ComboRushOverlay
    RuleBreaker,      // RuleBreakerOverlay
    LinkLock,         // LinkLockOverlay
    MazeRush,         // MazeRushOverlay
    CircuitScramble,  // CircuitScrambleOverlay
    ButtonMash,       // ButtonMashOverlay
    TargetShoot,      // TargetShootOverlay
    GridShift,        // GridShiftOverlay
    CardMatch         // CardMatchOverlay
}

// Static catalogue describing the available puzzle types and how each maps to
// its overlay prefab (by name fragment, matching the manager's lookup).
public static class RankedArena
{
    public static readonly List<RankedArenaPuzzleType> PuzzleTypes = new List<RankedArenaPuzzleType>
    {
        RankedArenaPuzzleType.GridLogic,
        RankedArenaPuzzleType.PatternRecall,
        RankedArenaPuzzleType.PipePulse,
        RankedArenaPuzzleType.ComboRush,
        RankedArenaPuzzleType.RuleBreaker,
        RankedArenaPuzzleType.LinkLock,
        RankedArenaPuzzleType.MazeRush,
        RankedArenaPuzzleType.CircuitScramble,
        RankedArenaPuzzleType.ButtonMash,
        RankedArenaPuzzleType.TargetShoot,
        RankedArenaPuzzleType.GridShift,
        RankedArenaPuzzleType.CardMatch
    };

    // Fragment found in the overlay prefab name for each type.
    public static string PrefabNameFragment(RankedArenaPuzzleType type)
    {
        switch (type)
        {
            case RankedArenaPuzzleType.GridLogic: return "Collapse";
            case RankedArenaPuzzleType.PatternRecall: return "Mirror";
            case RankedArenaPuzzleType.PipePulse: return "Pipe";
            case RankedArenaPuzzleType.ComboRush: return "Combo";
            case RankedArenaPuzzleType.RuleBreaker: return "Rule";
            case RankedArenaPuzzleType.LinkLock: return "Link";
            case RankedArenaPuzzleType.MazeRush: return "Maze";
            case RankedArenaPuzzleType.CircuitScramble: return "Circuit";
            case RankedArenaPuzzleType.ButtonMash: return "Button";
            case RankedArenaPuzzleType.TargetShoot: return "Target";
            case RankedArenaPuzzleType.GridShift: return "GridShift";
            case RankedArenaPuzzleType.CardMatch: return "Card";
            default: return "";
        }
    }
}

// Lightweight competitor model. In the overlay-based design players are
// represented by data + a player card (no world-space avatar), so Player is a
// plain serializable class rather than a MonoBehaviour.
[Serializable]
public class Player
{
    public int playerId;
    public string playerName;
    public bool isLocal;
    public bool isBot;          // true = AI bot filling a lobby slot; false = real human
    public bool isEliminated;
    public bool hasBomb;
    public int rankPoints;
    public int puzzlesSolved;
    public Color color = Color.white;

    public Player() { }

    public Player(int id, string name, bool local)
    {
        playerId = id;
        playerName = name;
        isLocal = local;
    }
}
