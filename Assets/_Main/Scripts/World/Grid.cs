using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridShape {
    public ShapeData ShapeData { get; }

    public Transform GetTransform();
}

public class Grid : MonoBehaviour {
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

    public void Init(int maxLength, int maxHeight, int maxWidth) {
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

    #region Manipulation

    public bool PlaceShape(Vector3Int rootCoord, IGridShape gridShape) {
        if (!ValidateShapePlacement(rootCoord, gridShape)) return false;

        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            cells[rootCoord + offset] = gridShape;
        }

        Transform fullGridShapeTrans = gridShape.GetTransform().parent;
        fullGridShapeTrans.SetParent(transform);
        fullGridShapeTrans.localPosition = rootCoord;

        return true;
    }
    
    public void RemoveShape(Vector3Int rootCoord, IGridShape gridShape) {
        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            cells.Remove(rootCoord + offset);
        }
        
        gridShape.GetTransform().parent.SetParent(null);
    }

    // Modify exactly one cell
    public bool Set(Vector3Int coord, IGridShape gridShape) {
        if (!IsValidPlacement(coord)) return false;

        cells[coord] = gridShape;
        return true;
    }
    
    // Modify exactly one cell
    public void Remove(Vector3Int coord) {
        if (IsOpen(coord) || !IsInBounds(coord.x, coord.z)) return;

        cells.Remove(coord);
    }

    #endregion

    #region Selection

    public IGridShape SelectPosition(Vector3Int coord) {
        if (!IsInBounds(coord.x, coord.z)) return null;
        return cells[coord];
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

    #region Validation

    public bool ValidateShapePlacement(Vector3Int rootCoord, IGridShape gridShape) {
        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            Vector3Int checkPos = new Vector3Int(rootCoord.x + offset.x, rootCoord.y + offset.y, rootCoord.z + offset.z);
            if (!IsValidPlacement(checkPos)) {
                return false;
            }
        }

        return true;
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

    public bool IsValidPlacement(Vector3Int coord) { return IsInBounds(coord.x, coord.z) && IsOpen(coord); }
    public bool IsOpen(Vector3Int coord) { return !cells.ContainsKey(coord); }
    public bool IsInBounds(int x, int z) { return validCells.Contains(new Vector2Int(x, z)); }

    #endregion
}