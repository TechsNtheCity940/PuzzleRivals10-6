using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Binary burst puzzle: players toggle nodes (powers of two) to match a target sum.
/// Refactored to use BinarySolver for core logic.
/// </summary>
public class BinaryBurstPuzzle : BasePuzzle
{
    [Header("Assets")]
    public Sprite nodeOffSprite;
    public Sprite nodeOnSprite;
    public Sprite[] nodeSolveFrames;
    public GameObject nodePrefab;

    [Header("UI")]
    public TMP_Text targetText;
    public TMP_Text currentText;
    public RectTransform nodeContainer;

    private List<BinaryNode> _nodes = new List<BinaryNode>();
    private BinarySolver _solver;
    private int _nodeCount = 6; // default, will be overridden by difficulty

    protected override void BuildBoard()
    {
        // Determine node count based on difficulty (4-8 nodes)
        _nodeCount = Mathf.Clamp(4 + difficulty / 2, 4, 8);
        int maxValue = (int)Mathf.Pow(2, _nodeCount) - 1;
        int targetValue = Random.Range(1, maxValue + 1);

        // UI
        if (targetText != null) targetText.text = "TARGET: " + targetValue;

        // Clear existing nodes
        ClearContainer();
        _nodes.Clear();

        // Create nodes for each power of two up to _nodeCount
        for (int i = 0; i < _nodeCount; i++)
        {
            int val = (int)Mathf.Pow(2, i);
            GameObject go = Instantiate(nodePrefab, nodeContainer);
            BinaryNode node = go.GetComponent<BinaryNode>();
            node.Init(val, nodeOffSprite, nodeOnSprite);
            node.OnToggled += HandleNodeToggled;
            _nodes.Add(node);
        }

        // Initialize the solver before reading its current sum.
        _solver = new BinarySolver(_nodes, targetValue);
        UpdateCurrentText();
    }

    private void ClearContainer()
    {
        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleNodeToggled(BinaryNode node)
    {
        if (!CanAcceptInput()) return;
        // No need to manually track currentValue; solver knows the state.
        PlayRotateSound();
        UpdateCurrentText();
        CheckWin();
    }

    private void UpdateCurrentText()
    {
        if (currentText != null && _solver != null)
            currentText.text = "SUM: " + _solver.CurrentSum();
    }

    private void CheckWin()
    {
        if (_solver.IsSolved && !isSolved)
        {
            StartCoroutine(WinRoutine());
        }
    }

    private IEnumerator WinRoutine()
    {
        foreach (var node in _nodes)
        {
            if (node.IsOn)
            {
                var anim = node.gameObject.AddComponent<UISpriteSheetAnimator>();
                anim.PlaySet(nodeSolveFrames, 12f, false, () => 
                {
                    // Animation complete - sprite stays on last frame (solve frames)
                });
            }
        }
        yield return new WaitForSeconds(0.5f);
        Solve();
    }

    public override void AutoSolveChunk()
    {
        if (isSolved || isTimedOut) return;

        var nodeToToggle = _solver.GetNextNodeToToggle();
        if (nodeToToggle != null && CanAcceptInput())
        {
            // Fixed: Use Toggle() instead of ToggleManual(bool) to flip the state
            nodeToToggle.Toggle();
            UpdateCurrentText();
            // No need to call UpdatePowerFlow; BinaryBurstPuzzle doesn't have power flow.
            // If there is a generic update needed, you could call base.Update() or similar.
        }
    }
}