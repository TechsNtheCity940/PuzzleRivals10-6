using UnityEngine;

/// <summary>
/// Centralized configuration for all puzzle types. Create via Assets/Create/Puzzle Rivals/Puzzle Config.
/// All magic numbers moved here for designer iteration without code changes.
/// </summary>
[CreateAssetMenu(fileName = "PuzzleConfig", menuName = "Puzzle Rivals/Puzzle Config")]
public class PuzzleConfig : ScriptableObject
{
    [Header("Global")]
    [Tooltip("Base puzzle time in seconds. Individual puzzles can override.")]
    public float basePuzzleTime = 30f;
    [Tooltip("Time bonus for battery/energy node collection.")]
    public float timeBonus = 5f;
    [Tooltip("Freeze attack duration in seconds.")]
    public float freezeDuration = 2.5f;

    [Header("Collapse Grid")]
    public int collapseMinColumns = 6, collapseMaxColumns = 8;
    public int collapseMinRows = 7, collapseMaxRows = 9;
    public int collapseCoreCountBase = 1, collapseCoreCountPerDifficulty = 2;
    public int collapseBraceCountBase = 2, collapseBraceCountPerDifficulty = 2;
    public float collapseTileSpacing = 92f;
    public float collapseRemoveAnimDuration = 0.15f;
    public float collapseGravityDelay = 0.13f;
    public float collapseShakeDuration = 0.8f, collapseShakeMagnitude = 20f;
    public float collapseCollapseDelay = 0.3f;

    [Header("Mirror Maze")]
    public int mirrorMinBoardSize = 7;
    public float mirrorBeamStepDelay = 0.01f;
    public float mirrorBeamThickness = 12f;
    public float mirrorTileSpacing = 60f;
    public int mirrorMinMirrors = 6, mirrorMaxMirrors = 8;
    public int mirrorMinPrisms = 1, mirrorMaxPrisms = 2;
    public int mirrorMinActivators = 1, mirrorMaxActivators = 2;
    public int mirrorMaxLockedPairs = 2;
    public float mirrorRotateAnimDuration = 0.18f;
    public float mirrorPulseDuration = 0.4f;
    public float mirrorShakeDuration = 0.8f, mirrorShakeMagnitude = 20f;

    [Header("Circuit Scramble")]
    public int circuitColumns = 6, circuitRows = 6;
    public float circuitTileSpacing = 110f;
    public float circuitSpeedIncreasePerSolve = 0.05f;
    public float circuitRelayCap = 0.4f;
    public float circuitChargeRate = 0.5f;
    public float circuitDischargeRate = 0.3f;
    public float circuitOverchargeTimer = 3f;

    [Header("Pipe Pulse")]
    public int pipeColumns = 5, pipeRows = 5;
    public float pipeTileSpacing = 110f;
    public float pipeBeamStepDelay = 0.01f;
    public float pipeBeamThickness = 12f;
    public float pipeReservoirBonus = 5f;

    [Header("Combo Rush")]
    public int comboColumns = 6, comboRows = 6;
    public float comboTileSpacing = 105f;
    public float comboWindow = 3f;
    public int comboBaseTarget = 800, comboTargetPerDifficulty = 400;
    public int comboBombThreshold = 5;
    public float comboPopDuration = 0.15f;
    public float comboShakeDuration = 0.2f, comboShakeMagnitude = 8f;

    [Header("Maze Rush")]
    public int mazeMinCells = 3, mazeMaxCells = 5;
    public float mazeTileSpacing = 60f;
    public float mazeMoveDuration = 0.15f;
    public float mazeWarpSlideDuration = 0.08f;
    public float mazeWarpShrinkDuration = 0.08f;

    [Header("Link Lock")]
    public int linkMinGridSize = 5;
    public float linkTileSpacing = 110f;
    public int linkMaxPaletteColors = 8;
    public float linkWireFlowFps = 14f;
    public float linkPulseScale = 1.2f, linkPulseDuration = 0.15f;

    [Header("Rule Breaker")]
    public int ruleColumns = 5, ruleRows = 5;
    public float ruleTileSpacing = 110f;
    public float ruleDuration = 8f;
    public int ruleCorrectPoints = 10, ruleWrongPenalty = 5;
    public int ruleBaseTarget = 60, ruleTargetPerDifficulty = 40;

    [Header("Card Matching")]
    public float cardFlipDuration = 0.3f;
    public int cardBasePairs = 6, cardPairsPerDifficulty = 2;

    [Header("Target Shooting")]
    public float targetSpawnInterval = 1f;
    public int targetBaseCount = 10, targetPerDifficulty = 3;
    public float targetScoreThreshold = 1000f;

    /// <summary>Singleton accessor for runtime convenience.</summary>
    public static PuzzleConfig Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadInstance()
    {
        Instance = Resources.Load<PuzzleConfig>("PuzzleConfig");
        if (Instance == null)
        {
            Debug.LogWarning("[PuzzleConfig] No config found in Resources. Using defaults.");
            Instance = CreateInstance<PuzzleConfig>();
        }
    }
}