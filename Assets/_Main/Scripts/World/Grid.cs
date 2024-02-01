using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    Dictionary<Vector3Int, IGridShape> cells = new();

    List<Vector2Int> validCells = new();

    void Start() { Init(); }

    void Init() {
        // Define initial play area
        for (int x = -10; x <= 10; x++) {
            for (int y = -10; y <= 10; y++) {
                validCells.Add(new Vector2Int(x, y));
            }
        }
    }

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

    #endregion

    #region Helper

    public bool IsValidPlacement(int x, int y, int z) { return IsInBounds(x, y) && IsOpen(x, y, z); }
    public bool IsOpen(int x, int y, int z) { return cells.ContainsKey(new Vector3Int(x, y, z)); }
    public bool IsInBounds(int x, int z) { return validCells.Contains(new Vector2Int(x, z)); }

    #endregion
}

public interface IGridShape {
}