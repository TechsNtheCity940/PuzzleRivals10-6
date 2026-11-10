using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles gravity mechanics for grid-based puzzles.
/// Can be reused across different grid puzzle types.
/// </summary>
public class GridGravitySystem
{
    private int columns;
    private int rows;
    private float tileSpacing;
    private System.Random rng;
    
    // Grid data
    private Transform[,] grid;
    private int[,] roles; // -1 = empty, otherwise contains tile type/data
    
    // Callbacks
    private System.Action<bool> onGravityComplete; // true if any movement occurred
    
    // Constructor without Random parameter
    public GridGravitySystem(int columns, int rows, float tileSpacing)
    {
        this.columns = columns;
        this.rows = rows;
        this.tileSpacing = tileSpacing;
        this.rng = new System.Random();
        
        grid = new Transform[columns, rows];
        roles = new int[columns, rows];
        
        // Initialize as empty
        ClearGrid();
    }
    
    // Constructor with Random parameter
    public GridGravitySystem(int columns, int rows, float tileSpacing, System.Random rng)
    {
        this.columns = columns;
        this.rows = rows;
        this.tileSpacing = tileSpacing;
        this.rng = rng;
        
        grid = new Transform[columns, rows];
        roles = new int[columns, rows];
        
        // Initialize as empty
        ClearGrid();
    }
    
    public void ClearGrid()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                grid[c, r] = null;
                roles[c, r] = -1;
            }
        }
    }
    
    public void SetCell(int column, int row, Transform transform, int role)
    {
        if (IsInBounds(column, row))
        {
            grid[column, row] = transform;
            roles[column, row] = role;
        }
    }
    
    public void ClearCell(int column, int row)
    {
        if (IsInBounds(column, row))
        {
            grid[column, row] = null;
            roles[column, row] = -1;
        }
    }
    
    public Transform GetCell(int column, int row)
    {
        if (IsInBounds(column, row))
            return grid[column, row];
        return null;
    }
    
    public int GetCellRole(int column, int row)
    {
        if (IsInBounds(column, row))
            return roles[column, row];
        return -1;
    }
    
    public bool IsInBounds(int column, int row)
    {
        return column >= 0 && column < columns && row >= 0 && row < rows;
    }
    
    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < columns && pos.y >= 0 && pos.y < rows;
    }
    
    /// <summary>
    /// Applies gravity to the grid. Objects fall down to fill empty spaces.
    /// </summary>
    /// <param name="excludeRoles">Roles that should not be affected by gravity (e.g., fixed braces)</param>
    /// <param name="fallDelay">Delay between each fall step in seconds</param>
    /// <param name="onComplete">Callback when gravity finishes</param>
    public void ApplyGravity(int[] excludeRoles, float fallDelay, System.Action<bool> onComplete = null)
    {
        this.onGravityComplete = onComplete;
        
        // Start the coroutine via MonoBehaviour - we'll need a reference
        // For now, we'll return whether movement occurred immediately
        bool moved = ApplyGravityInstant(excludeRoles);
        
        if (onComplete != null)
            onComplete.Invoke(moved);
    }
    
    /// <summary>
    /// Applies gravity instantly without animation.
    /// </summary>
    /// <returns>True if any movement occurred</returns>
    public bool ApplyGravityInstant(int[] excludeRoles)
    {
        bool anyMoved = false;
        
        // Process each column from bottom up
        for (int c = 0; c < columns; c++)
        {
            int writePos = rows - 1;
            
            // Scan from bottom to top
            for (int r = rows - 1; r >= 0; r--)
            {
                if (IsInBounds(c, r) && grid[c, r] != null)
                {
                    int role = roles[c, r];
                    bool isExcluded = false;
                    
                    // Check if this role should be excluded from gravity
                    if (excludeRoles != null)
                    {
                        foreach (int excludedRole in excludeRoles)
                        {
                            if (role == excludedRole)
                            {
                                isExcluded = true;
                                break;
                            }
                        }
                    }
                    
                    if (!isExcluded)
                    {
                        // Move this item down to writePos if needed
                        if (r != writePos)
                        {
                            // Swap positions
                            grid[c, writePos] = grid[c, r];
                            roles[c, writePos] = roles[c, r];
                            
                            // Clear original position
                            grid[c, r] = null;
                            roles[c, r] = -1;
                            
                            anyMoved = true;
                        }
                        
                        writePos--;
                    }
                    else
                    {
                        // This item blocks items above it from falling past it
                        writePos = r - 1;
                    }
                }
            }
        }
        
        return anyMoved;
    }
    
    /// <summary>
    /// Gets the world position for a grid cell.
    /// </summary>
    public Vector2 GetWorldPosition(int column, int row, float centerX, float centerY)
    {
        return new Vector2(
            column * tileSpacing - centerX,
            -(row * tileSpacing) + centerY
        );
    }
    
    /// <summary>
    /// Gets the grid coordinates from a world position.
    /// </summary>
    public bool TryGetGridPosition(Vector2 worldPos, float centerX, float centerY, out int column, out int row)
    {
        column = Mathf.RoundToInt((worldPos.x + centerX) / tileSpacing);
        row = Mathf.RoundToInt((-worldPos.y + centerY) / tileSpacing);
        
        return IsInBounds(column, row);
    }
}