using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages mirror/prism groups and their states for Mirror Maze puzzle.
/// Handles group creation, state tracking, and rotation logic.
/// </summary>
public class MirrorGroupManager
{
    private MirrorMazeGenerator.BoardPlan _plan;
    private MirrorMazePuzzle.BoardData _boardData;
    private System.Random _rng;
    
    // Group state tracking
    private readonly Dictionary<GridTile, int> _tileGroupIndex = 
        new Dictionary<GridTile, int>();
    private readonly Dictionary<int, List<GridTile>> _groupTiles = 
        new Dictionary<int, List<GridTile>>();
    
    public MirrorGroupManager(MirrorMazeGenerator.BoardPlan plan, 
                              MirrorMazePuzzle.BoardData boardData, 
                              System.Random rng)
    {
        _plan = plan;
        _boardData = boardData;
        _rng = rng;
        
        InitializeGroups();
    }
    
    private void InitializeGroups()
    {
        // Clear existing mappings
        _tileGroupIndex.Clear();
        _groupTiles.Clear();
        
        // Build tile-to-group mapping
        for (int groupIndex = 0; groupIndex < _plan.groups.Count; groupIndex++)
        {
            var group = _plan.groups[groupIndex];
            var tileList = new List<GridTile>();
            
            foreach (var pos in group)
            {
                var tile = _boardData.cellTiles[pos.y, pos.x];
                if (tile != null)
                {
                    _tileGroupIndex[tile] = groupIndex;
                    tileList.Add(tile);
                }
            }
            
            if (tileList.Count > 0)
            {
                _groupTiles[groupIndex] = tileList;
            }
        }
    }
    
    /// <summary>
    /// Gets the group index for a tile, or -1 if not in a group.
    /// </summary>
    public int GetGroupIndex(GridTile tile)
    {
        return _tileGroupIndex.TryGetValue(tile, out var index) ? index : -1;
    }
    
    /// <summary>
    /// Gets all tiles in a specific group.
    /// </summary>
    public List<GridTile> GetGroupTiles(int groupIndex)
    {
        return _groupTiles.TryGetValue(groupIndex, out var tiles) ? tiles : new List<GridTile>();
    }
    
    /// <summary>
    /// Gets the number of groups.
    /// </summary>
    public int GroupCount => _plan.groups.Count;
    
    /// <summary>
    /// Gets the maximum states for a group at the given index.
    /// </summary>
    public int GetMaxStates(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= _plan.groups.Count) return 1;
        
        var firstTile = _plan.groups[groupIndex][0];
        var tile = _boardData.cellTiles[firstTile.y, firstTile.x];
        if (tile == null) return 1;
        
        // Mirrors have 2 states (0°, 90°), others have 4 states (0°, 90°, 180°, 270°)
        return tile.type == MirrorMazeGenerator.MIRROR ? 2 : 4;
    }
    
    /// <summary>
    /// Gets the current state of a group.
    /// </summary>
    public int GetGroupState(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= _plan.groups.Count) return 0;
        
        var firstTile = _plan.groups[groupIndex][0];
        return _boardData.orient[firstTile.y, firstTile.x];
    }
    
    /// <summary>
    /// Sets the state of a group and updates all tiles in it.
    /// </summary>
    public void SetGroupState(int groupIndex, int state, bool updateVisuals = true)
    {
        if (groupIndex < 0 || groupIndex >= _plan.groups.Count) return;
        
        // Validate state
        int maxStates = GetMaxStates(groupIndex);
        state = Mathf.Clamp(state, 0, maxStates - 1);
        
        // Update board data
        var group = _plan.groups[groupIndex];
        foreach (var pos in group)
        {
            _boardData.orient[pos.y, pos.x] = state;
        }
        
        // Update visuals if requested
        if (updateVisuals)
        {
            UpdateGroupVisuals(groupIndex, state);
        }
    }
    
    /// <summary>
    /// Advances a group by one step toward its target state.
    /// Returns true if the group was advanced, false if already at target.
    /// </summary>
    public bool AdvanceGroupTowardTarget(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= _plan.groups.Count) return false;
        
        int currentState = GetGroupState(groupIndex);
        int targetState = _plan.solutionStates[groupIndex];
        int maxStates = GetMaxStates(groupIndex);
        
        if (currentState == targetState) return false;
        
        // Calculate next state (shortest path)
        int forwardSteps = (targetState - currentState + maxStates) % maxStates;
        int backwardSteps = (currentState - targetState + maxStates) % maxStates;
        
        int nextState;
        if (forwardSteps <= backwardSteps)
        {
            nextState = (currentState + 1) % maxStates;
        }
        else
        {
            nextState = (currentState - 1 + maxStates) % maxStates;
        }
        
        SetGroupState(groupIndex, nextState);
        return true;
    }
    
    /// <summary>
    /// Randomizes all group states to a scrambled configuration.
    /// Ensures the puzzle is not already solved.
    /// </summary>
    public void ScrambleAllGroups()
    {
        bool isSolved = true;
        int safety = 0;
        
        // Keep scrambling until we get an unsolved state
        while (isSolved && safety++ < 100)
        {
            isSolved = false;
            
            for (int i = 0; i < _plan.groups.Count; i++)
            {
                int maxStates = GetMaxStates(i);
                int randomState = Random.Range(0, maxStates);
                SetGroupState(i, randomState, false);
                
                if (randomState != _plan.solutionStates[i])
                {
                    isSolved = false;
                }
            }
        }
        
        // Update visuals after scrambling
        for (int i = 0; i < _plan.groups.Count; i++)
        {
            UpdateGroupVisuals(i, GetGroupState(i));
        }
    }
    
    private void UpdateGroupVisuals(int groupIndex, int state)
    {
        var group = _plan.groups[groupIndex];
        foreach (var pos in group)
        {
            var tile = _boardData.cellTiles[pos.y, pos.x];
            if (tile == null) continue;
            
            // Calculate rotation angle based on tile type and state
            float angle = 0f;
            if (tile.type == MirrorMazeGenerator.MIRROR)
            {
                angle = (state == MirrorMazeGenerator.BACKSLASH) ? 90f : 0f;
            }
            else if (tile.type == MirrorMazeGenerator.PRISM || 
                     tile.type == MirrorMazeGenerator.FILTER ||
                     tile.type == MirrorMazeGenerator.ACTIVATOR ||
                     tile.type == MirrorMazeGenerator.ENERGY_NODE)
            {
                angle = state * 90f;
            }
            
            // Apply rotation with tweening would be handled by the puzzle
            // For now, we just set it directly - the puzzle handles tweening
            tile.SetRotation(angle);
        }
    }
}