using System.Collections.Generic;
using UnityEngine;

public class Grid {
    // Min LHW defined as -max LHW
    // Center defined as (0,0,0)
    public int MaxLength => maxLength;
    int maxLength;
    public int MaxHeight => maxHeight;
    int maxHeight;
    public int MaxWidth => maxWidth;
    int maxWidth;
    
    Dictionary<Vector3Int, IGridShape> cells = new();

    List<Vector2Int> validCells = new();

    public Grid(int maxLength, int maxHeight, int maxWidth) {
        this.maxLength = maxLength;
        this.maxHeight = maxHeight;
        this.maxWidth = maxWidth;
        
        // TEMP: Define initial play area
        for (int x = -10; x <= 10; x++) {
            for (int y = -10; y <= 10; y++) {
                validCells.Add(new Vector2Int(x, y));
            }
        }
    }

    #region Validation

    public bool ValidateShapePlacement() {
        // foreach (Block block in piece.Blocks) {
        //     Vector2Int boardPos = new Vector2Int(x + block.Position.x, y + block.Position.y);
        //     if (!checkType(boardPos.x, boardPos.y)) {
        //         return false;
        //     }
        // }

        return true;
    }

    #endregion

    #region Manipulation

    public bool Place(IGridShape gridShape, int x, int y, int z) {
        if (!IsValidPlacement(x, y, z)) return false;

        cells[new Vector3Int(x, y, z)] = gridShape;
        return true;
    }
    
    public void Remove(int x, int y, int z) {
        if (IsOpen(x, y, z) || !IsInBounds(x, z)) return;

        cells.Remove(new Vector3Int(x, y, z));
    }

    #endregion

    #region Selection

    public IGridShape SelectPosition(int x, int y, int z) {
        if (!IsInBounds(x, y)) return null;
        return cells[new Vector3Int(x, y, z)];
    }

    public IGridShape SelectOffset(Vector3Int origin, Vector3Int offset) {
        int targetX = origin.x + offset.x;
        int targetY = origin.y + offset.y;
        int targetZ = origin.z + offset.z;

        if (!IsInBounds(targetX, targetY)) return null;

        return cells[new Vector3Int(targetX, targetY, targetZ)];
    }

    /// <summary>
    /// Returns true if valid highest open cell exists in column at (x, z).
    /// </summary>
    public bool SelectLowestOpen(int x, int z, out int lowestOpenY) {
        for (int y = 0; y < maxHeight; y++) {
            Vector3Int coord = new Vector3Int(x, y, z);
            if (!cells.ContainsKey(coord)) {
                lowestOpenY = coord.y;
                return true;
            }
        }

        lowestOpenY = -1;
        return false;
    }

    #endregion

    #region ValidCells

    /// <summary>
    /// Adds a range of valid cells to grid (inclusive)
    /// </summary>
    public void AddRange(int startX, int startY, int endX, int endY) {
        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= startY; y++) {
                validCells.Add(new Vector2Int(x, y));
            }
        }
    }

    #endregion

    #region Helper

    public bool IsValidPlacement(int x, int y, int z) { return IsInBounds(x, y) && IsOpen(x, y, z); }
    public bool IsOpen(int x, int y, int z) { return cells.ContainsKey(new Vector3Int(x, y, z)); }
    public bool IsInBounds(int x, int z) { return validCells.Contains(new Vector2Int(x, z)); }

    #endregion
}

public interface IGridShape {
}