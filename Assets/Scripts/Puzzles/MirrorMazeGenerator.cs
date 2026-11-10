using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates Mirror Maze boards with guaranteed solvable paths.
/// Separated from puzzle logic for testability and reuse.
/// </summary>
public static class MirrorMazeGenerator
{
    public const int FLOOR = 0;
    public const int EMITTER = 1;
    public const int RECEIVER = 2;
    public const int MIRROR = 3;
    public const int PRISM = 4;
    public const int ACTIVATOR = 5;
    public const int FILTER = 6;
    public const int ENERGY_NODE = 7;

    public const int SLASH = 0;
    public const int BACKSLASH = 1;

    public struct BoardPlan
    {
        public int boardSize;
        public Vector2Int emitterPos;
        public Vector2Int emitDir;
        public Vector2Int receiverPos;
        public Dictionary<Vector2Int, int> plan; // pos -> type (MIRROR, PRISM, ACTIVATOR, FILTER, ENERGY_NODE)
        public List<List<Vector2Int>> groups; // linked mirror/prism groups
        public int[] scrambledStates; // initial rotation state per group
        public int[] solutionStates; // solved rotation state per group
    }

    // Direction vectors: Up, Right, Down, Left
    private static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0)
    };

    /// <summary>
    /// Generates a complete board plan with guaranteed 1-4 solutions, then scrambles to unsolved state.
    /// </summary>
    public static BoardPlan Generate(int gridSize, int difficulty, System.Random rng)
    {
        int boardSize = Mathf.Max(7, gridSize);

        // Element counts by difficulty
        int numMirrors = 6 + Mathf.Clamp(difficulty, 0, 2);
        int numPrisms = (difficulty >= 2) ? 2 : 1;
        int numActivators = (difficulty >= 1) ? 2 : 1;
        int numLockedPairs = Mathf.Clamp(difficulty, 0, 2);

        var plan = new Dictionary<Vector2Int, int>();
        var groups = new List<List<Vector2Int>>();
        int[] scrambledStates = null;
        int[] solutionStates = null;
        Vector2Int emitterPos = default, receiverPos = default;
        Vector2Int emitDir = Vector2Int.right;

        bool success = false;
        int maxAttempts = 500;

        while (!success && maxAttempts-- > 0)
        {
            plan.Clear();
            var occupied = new HashSet<Vector2Int>();
            int[,] tempTypes = new int[boardSize, boardSize];

            // 1. Place emitter (left edge)
            int ery = rng.Next(2, boardSize - 2);
            emitterPos = new Vector2Int(0, ery);
            emitDir = new Vector2Int(1, 0);
            occupied.Add(emitterPos);
            tempTypes[ery, 0] = EMITTER;

            // 2. Place receiver (right edge)
            int rry = rng.Next(2, boardSize - 2);
            receiverPos = new Vector2Int(boardSize - 1, rry);
            occupied.Add(receiverPos);
            tempTypes[rry, boardSize - 1] = RECEIVER;

            // 3. Place prisms
            int placedPrisms = 0, prismGuard = 0;
            while (placedPrisms < numPrisms && prismGuard++ < 100)
            {
                int px = rng.Next(2, boardSize - 3);
                int py = rng.Next(1, boardSize - 2);
                var pos = new Vector2Int(px, py);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = PRISM;
                    occupied.Add(pos);
                    tempTypes[py, px] = PRISM;
                    placedPrisms++;
                }
            }

            // 4. Place filter (color converter)
            int filterGuard = 0;
            while (filterGuard++ < 100)
            {
                int fx = rng.Next(1, boardSize - 3);
                int fy = rng.Next(1, boardSize - 2);
                var pos = new Vector2Int(fx, fy);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = FILTER;
                    occupied.Add(pos);
                    tempTypes[fy, fx] = FILTER;
                    break;
                }
            }

            // 5. Place activators (endpoints that must be lit)
            int placedActivators = 0, actGuard = 0;
            while (placedActivators < numActivators && actGuard++ < 100)
            {
                int ax = rng.Next(1, boardSize - 2);
                int ay = rng.Next(1, boardSize - 2);
                var pos = new Vector2Int(ax, ay);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = ACTIVATOR;
                    occupied.Add(pos);
                    tempTypes[ay, ax] = ACTIVATOR;
                    placedActivators++;
                }
            }

            // 6. Place energy nodes (bonus time)
            int numNodes = (difficulty > 1) ? 2 : 1;
            int placedNodes = 0, nodeGuard = 0;
            while (placedNodes < numNodes && nodeGuard++ < 100)
            {
                int nx = rng.Next(1, boardSize - 2);
                int ny = rng.Next(1, boardSize - 2);
                var pos = new Vector2Int(nx, ny);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = ENERGY_NODE;
                    occupied.Add(pos);
                    tempTypes[ny, nx] = ENERGY_NODE;
                    placedNodes++;
                }
            }

            // 7. Place mirrors
            int placedMirrors = 0, mirrorGuard = 0;
            while (placedMirrors < numMirrors && mirrorGuard++ < 200)
            {
                int mx = rng.Next(1, boardSize - 2);
                int my = rng.Next(1, boardSize - 2);
                var pos = new Vector2Int(mx, my);
                if (!occupied.Contains(pos))
                {
                    plan[pos] = MIRROR;
                    occupied.Add(pos);
                    tempTypes[my, mx] = MIRROR;
                    placedMirrors++;
                }
            }

            if (placedMirrors < numMirrors || placedPrisms < numPrisms || placedActivators < numActivators)
                continue;

            // 8. Build interactable lists for solver
            var tempInteractables = new List<Vector2Int>();
            var tempEndpoints = new List<Vector2Int> { receiverPos };

            foreach (var kv in plan)
            {
                if (kv.Value == MIRROR || kv.Value == PRISM)
                    tempInteractables.Add(kv.Key);
                else if (kv.Value == ACTIVATOR)
                    tempEndpoints.Add(kv.Key);
            }

            // 9. Shuffle and create linked groups
            Shuffle(tempInteractables, rng);
            var tempGroups = new List<List<Vector2Int>>();
            int pairsCreated = 0;
            while (pairsCreated < numLockedPairs && tempInteractables.Count >= 2)
            {
                var g = new List<Vector2Int> { tempInteractables[^1], tempInteractables[^2] };
                tempInteractables.RemoveRange(tempInteractables.Count - 2, 2);
                tempGroups.Add(g);
                pairsCreated++;
            }
            while (tempInteractables.Count > 0)
            {
                tempGroups.Add(new List<Vector2Int> { tempInteractables[^1] });
                tempInteractables.RemoveAt(tempInteractables.Count - 1);
            }

            // 10. Find all solutions via exhaustive search
            var solutions = new List<int[]>();
            var currentStates = new int[tempGroups.Count];
            int[,] tempOrient = new int[boardSize, boardSize];
            int solutionCount = 0;

            SearchCombinations(0, tempGroups, currentStates, ref solutionCount, solutions, tempOrient, tempEndpoints, receiverPos, tempTypes);

            if (solutionCount > 0 && solutionCount <= 4)
            {
                // Pick a random solution
                solutionStates = solutions[rng.Next(solutions.Count)];

                // Scramble to unsolved state (>=3 clicks different)
                scrambledStates = new int[tempGroups.Count];
                bool isScrambledSolved = true;
                int safety = 0;
                while (isScrambledSolved && safety++ < 100)
                {
                    int diffClicks = 0;
                    for (int i = 0; i < tempGroups.Count; i++)
                    {
                        int maxS = (tempGroups[i][0].y < boardSize && tempGroups[i][0].x < boardSize && tempTypes[tempGroups[i][0].y, tempGroups[i][0].x] == MIRROR) ? 2 : 4;
                        scrambledStates[i] = rng.Next(0, maxS);
                        if (scrambledStates[i] != solutionStates[i]) diffClicks++;
                    }

                    if (diffClicks >= 3)
                    {
                        for (int i = 0; i < tempGroups.Count; i++)
                            foreach (var tile in tempGroups[i])
                                tempOrient[tile.y, tile.x] = scrambledStates[i];

                        if (!TraceBeamStatic(tempOrient, tempEndpoints, receiverPos, tempTypes))
                            isScrambledSolved = false;
                    }
                }

                if (!isScrambledSolved)
                {
                    success = true;
                    groups = tempGroups;
                }
            }
        }

        if (!success)
        {
            // Fallback: minimal valid board
            boardSize = 7;
            emitterPos = new Vector2Int(0, 3);
            emitDir = Vector2Int.right;
            receiverPos = new Vector2Int(6, 3);
            plan[new Vector2Int(3, 3)] = MIRROR;
            groups = new List<List<Vector2Int>> { new List<Vector2Int> { new Vector2Int(3, 3) } };
            scrambledStates = new int[] { 0 };
            solutionStates = new int[] { 1 };
        }

        return new BoardPlan
        {
            boardSize = boardSize,
            emitterPos = emitterPos,
            emitDir = emitDir,
            receiverPos = receiverPos,
            plan = plan,
            groups = groups,
            scrambledStates = scrambledStates,
            solutionStates = solutionStates
        };
    }

    // ----- Internal solver (static, no Unity deps) -----

    private static void SearchCombinations(int groupIdx, List<List<Vector2Int>> groups, int[] currentStates,
        ref int solutionCount, List<int[]> solutions, int[,] tempOrient,
        List<Vector2Int> endpoints, Vector2Int receiverPos, int[,] tempTypes)
    {
        if (groupIdx >= groups.Count)
        {
            if (TraceBeamStatic(tempOrient, endpoints, receiverPos, tempTypes))
            {
                solutionCount++;
                solutions.Add((int[])currentStates.Clone());
            }
            return;
        }

        int maxStates = (tempTypes[groups[groupIdx][0].y, groups[groupIdx][0].x] == MIRROR) ? 2 : 4;
        for (int s = 0; s < maxStates; s++)
        {
            currentStates[groupIdx] = s;
            foreach (var tile in groups[groupIdx])
                tempOrient[tile.y, tile.x] = s;
            SearchCombinations(groupIdx + 1, groups, currentStates, ref solutionCount, solutions, tempOrient, endpoints, receiverPos, tempTypes);
            if (solutionCount > 10) return;
        }
    }

    private static bool TraceBeamStatic(int[,] orient, List<Vector2Int> endpoints, Vector2Int receiverPos, int[,] types)
    {
        var triggered = new HashSet<Vector2Int>();
        var heads = new List<BeamHead> { new BeamHead(new Vector2Int(-1, 0), Vector2Int.right, Color.white) }; // will be replaced with actual emitter
        var visited = new HashSet<(Vector2Int, Vector2Int, Color)>();

        // Fix: emitter position is passed via the types array (EMITTER at edge)
        Vector2Int emitterPos = new Vector2Int(-1, -1);
        Vector2Int emitDir = Vector2Int.right;
        int boardSize = types.GetLength(0);
        for (int y = 0; y < boardSize; y++)
        {
            if (types[y, 0] == EMITTER) { emitterPos = new Vector2Int(0, y); break; }
        }
        heads = new List<BeamHead> { new BeamHead(emitterPos, emitDir, Color.white) };

        int limit = boardSize * boardSize * 4;
        while (heads.Count > 0 && limit-- > 0)
        {
            var nextHeads = new List<BeamHead>();
            foreach (var h in heads)
            {
                var nextPos = h.pos + h.dir;
                if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= boardSize || nextPos.y >= boardSize) continue;
                if (visited.Contains((nextPos, h.dir, h.color))) continue;
                visited.Add((nextPos, h.dir, h.color));

                int type = types[nextPos.y, nextPos.x];
                if (type == MIRROR)
                    nextHeads.Add(new BeamHead(nextPos, Reflect(h.dir, orient[nextPos.y, nextPos.x]), h.color));
                else if (type == PRISM)
                {
                    int prOrient = orient[nextPos.y, nextPos.x] % 4;
                    var backDir = GetPrismBackDir(prOrient);
                    foreach (var d in DIRS)
                        if (d != backDir) nextHeads.Add(new BeamHead(nextPos, d, h.color));
                }
                else if (type == FILTER)
                    nextHeads.Add(new BeamHead(nextPos, h.dir, Color.cyan)); // Receiver color
                else if (type == ACTIVATOR || type == ENERGY_NODE)
                {
                    triggered.Add(nextPos);
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
                }
                else if (type == RECEIVER)
                {
                    if (h.color == Color.cyan) triggered.Add(nextPos);
                }
                else
                    nextHeads.Add(new BeamHead(nextPos, h.dir, h.color));
            }
            heads = nextHeads;
        }

        foreach (var req in endpoints)
            if (!triggered.Contains(req)) return false;
        return true;
    }

    private struct BeamHead
    {
        public Vector2Int pos, dir;
        public Color color;
        public BeamHead(Vector2Int p, Vector2Int d, Color c) { pos = p; dir = d; color = c; }
    }

    private static Vector2Int Reflect(Vector2Int d, int o)
        => o == 1 ? new Vector2Int(d.y, d.x) : new Vector2Int(-d.y, -d.x);

    private static Vector2Int GetPrismBackDir(int orient)
    {
        switch (orient % 4)
        {
            case 0: return new Vector2Int(0, 1);
            case 1: return new Vector2Int(-1, 0);
            case 2: return new Vector2Int(0, -1);
            default: return new Vector2Int(1, 0);
        }
    }

    private static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}