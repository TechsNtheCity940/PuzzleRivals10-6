using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper for BinaryBurstPuzzle: manages the binary node values,
/// computes the current sum, determines the target configuration,
/// and provides the next node to toggle for auto-solving.
/// </summary>
public class BinarySolver
{
    private List<BinaryNode> _nodes;
    private int _targetValue;

    public BinarySolver(List<BinaryNode> nodes, int targetValue)
    {
        _nodes = nodes;
        _targetValue = targetValue;
    }

    /// <summary>
    /// Returns the sum of values of all nodes that are currently ON.
    /// </summary>
    public int CurrentSum()
    {
        int sum = 0;
        foreach (var node in _nodes)
        {
            if (node.IsOn)
                sum += node.Value;
        }
        return sum;
    }

    /// <summary>
    /// Returns true if the current sum matches the target value.
    /// </summary>
    public bool IsSolved => CurrentSum() == _targetValue;

    /// <summary>
    /// Returns the node that should be toggled next to move toward the target,
    /// or null if already solved.
    /// Follows the greedy high‑to‑low bit approach used in AutoSolveChunk.
    /// </summary>
    public BinaryNode GetNextNodeToToggle()
    {
        if (IsSolved)
            return null;

        int current = CurrentSum();

        // Iterate from highest value to lowest.
        for (int i = _nodes.Count - 1; i >= 0; i--)
        {
            var node = _nodes[i];
            int val = node.Value;
            bool bitInTarget = (_targetValue & val) != 0;
            bool nodeIsOn = node.IsOn;

            // If node state differs from what the target requires, toggling it will move us closer.
            if (nodeIsOn != bitInTarget)
                return node;
        }

        // Should not reach here if not solved.
        return null;
    }

    /// <summary>
    /// Forces the solver to recalculate internal state if the node list changed.
    /// </summary>
    public void RefreshNodes(List<BinaryNode> nodes)
    {
        _nodes = nodes;
    }
}