using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public interface IGridShape {
    public Vector3Int RootCoord { get; }
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }

    public ShapeData ShapeData { get; }
}

public class Grid : MonoBehaviour {
    [InfoBox("Min LHW defined as -max LHW.\nCenter defined as (0,0,0).")] // not showing in inspector for now
    public int MaxLength => maxLength;
    [SerializeField] int maxLength;
    public int MaxHeight => maxHeight;
    [SerializeField] int maxHeight;
    public int MaxWidth => maxWidth;
    [SerializeField] int maxWidth;

    Dictionary<Vector3Int, IGridShape> cells = new();

    [ReadOnly,SerializeField] List<Vector2Int> validCells = new();

    void Start() { Init(maxLength, maxHeight, maxWidth); }

    void Init(int maxLength, int maxHeight, int maxWidth) {
        this.maxLength = maxLength;
        this.maxHeight = maxHeight;
        this.maxWidth = maxWidth;

        // Set grid bounds
        // actual length/width rounds to odd num due to centering on (0,0,0)
        for (int x = -maxLength/2; x <= maxLength/2; x++) {
            for (int z = -maxWidth/2; z <= maxWidth/2; z++) {
                validCells.Add(new Vector2Int(x, z));
            }
        }
        
        // Add pre-existing scene shapes to grid
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).GetChild(0).TryGetComponent(out IGridShape gridShape)) {
                if (!PlaceShape(gridShape.RootCoord, gridShape)) {
                    Debug.LogError("Unable to place shape. Pre-existing scene shape overlaps with another shape in grid.");
                }
            } else {
                Debug.LogErrorFormat("Only shapes with IGridShape component should be a child of Grid. ({0})", transform.GetChild(i).name);
            }
        }
    }

    #region Manipulation

    public bool PlaceShape(Vector3Int targetCoord, IGridShape gridShape) {
        if (!ValidateShapePlacement(targetCoord, gridShape)) return false;
        PlaceShapeNoValidate(targetCoord, gridShape);

        return true;
    }
    void PlaceShapeNoValidate(Vector3Int targetCoord, IGridShape gridShape) {
        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            cells[targetCoord + offset] = gridShape;
        }

        gridShape.ShapeTransform.SetParent(transform);
        gridShape.ShapeTransform.localPosition = targetCoord;
        gridShape.ShapeTransform.localRotation = Quaternion.identity;
    }

    // Returns false if any placement of shape in gridShapes is invalid.
    // Placement is relative to first shape's root coord placed at targetCoord.
    public bool PlaceShapes(Vector3Int targetCoord, List<IGridShape> gridShapes) {
        if (gridShapes.Count == 0) return false;
        if (!ValidateShapesPlacement(targetCoord, gridShapes)) return false;

        Vector3Int lastShapeRootCoord = gridShapes[0].RootCoord;
        foreach (IGridShape gridShape in gridShapes) {
            targetCoord += gridShape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = gridShape.RootCoord;
            PlaceShapeNoValidate(targetCoord, gridShape);
        }

        return true;
    }

    public bool MoveShapes(Grid targetGrid, Vector3Int targetCoord, List<IGridShape> gridShapes) {
        // Save original gridShape coords in original grid for removal
        List<Vector3Int> origRootCoords = new();
        for (int i = 0; i < gridShapes.Count; i++) {
            origRootCoords.Add(gridShapes[i].RootCoord);
        }

        if (!targetGrid.PlaceShapes(targetCoord, gridShapes)) return false;

        for (int i = 0; i < gridShapes.Count; i++) {
            RemoveShapeCells(origRootCoords[i], gridShapes[i]);
        }

        return true;
    }

    public void DestroyShape(IGridShape gridShape) {
        RemoveShapeCells(gridShape.RootCoord, gridShape);

        // TODO: prob let gridshape handle its destruction, just call that on gridShape
        Destroy(gridShape.ShapeTransform.gameObject);
    }

    // Set exactly one cell
    public bool SetCoord(Vector3Int coord, IGridShape gridShape) {
        if (!IsValidPlacement(coord)) return false;

        cells[coord] = gridShape;
        return true;
    }
    // Remove exactly one cell
    public void RemoveCoord(Vector3Int coord) {
        if (IsOpen(coord) || !IsInBounds(coord)) return;

        cells.Remove(coord);
    }
    void RemoveShapeCells(Vector3Int rootCoord, IGridShape gridShape) {
        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            cells.Remove(rootCoord + offset);
        }
    }

    #endregion

    #region Selection

    // Simple form of SelectStackedShapes. Assumes all shapes are 1x1x1
    // TODO: Modify for multi-space shapes
    public List<IGridShape> SelectStackedShapes(Vector3Int coord) {
        List<IGridShape> stackedShapes = new();
        while (coord.y < maxHeight && !IsOpen(coord)) {
            IGridShape shape = cells[coord];

            if (!stackedShapes.Contains(shape)) {
                stackedShapes.Add(shape);
            }

            coord.y++;
        }

        return stackedShapes;
    }

    // TODO: untested. Possible infinite loop with enqueuing cells that have already been checked?
    // public List<IGridShape> SelectStackedShapes(Vector3Int coord) {
    //     List<IGridShape> stackedShapes = new();
    //     Queue<Vector3Int> cellsToCheck = new Queue<Vector3Int>();
    //
    //     cellsToCheck.Enqueue(coord);
    //     while (cellsToCheck.Count > 0) {
    //         Vector3Int checkCoord = cellsToCheck.Dequeue();
    //         checkCoord += Vector3Int.up; // Search above check cell
    //         if (checkCoord.y < maxHeight !IsOpen(checkCoord)) {
    //             IGridShape shape = cells[checkCoord];
    //             
    //             if (!stackedShapes.Contains(shape)) { // Add shape if new
    //                 stackedShapes.Add(shape);
    //                 
    //                 foreach (Vector3Int offset in shape.ShapeData.Shape) { // Add cells of this shape to the check queue
    //                     cellsToCheck.Enqueue(checkCoord + offset);
    //                 }
    //             }
    //         }
    //     }
    //
    //     return stackedShapes;
    // }

    public IGridShape SelectPosition(Vector3Int coord) {
        if (!IsInBounds(coord)) return null;
        return cells[coord];
    }

    public IGridShape SelectOffset(Vector3Int origin, Vector3Int offset) {
        Vector3Int targetCoord = new Vector3Int(origin.x + offset.x, origin.y + offset.y, origin.z + offset.z);
        if (!IsInBounds(targetCoord)) return null;

        return cells[targetCoord];
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

    // Validates placement of shape when shape's root coord is placed at targetCoord
    public bool ValidateShapePlacement(Vector3Int targetCoord, IGridShape gridShape) {
        foreach (Vector3Int offset in gridShape.ShapeData.Shape) {
            Vector3Int checkPos = new Vector3Int(targetCoord.x + offset.x, targetCoord.y + offset.y, targetCoord.z + offset.z);
            if (!IsValidPlacement(checkPos)) {
                return false;
            }
        }

        return true;
    }

    // Validates placement of shapes in input list using current positioning in their current grid.
    // Placement is relative to first shape's root coord placed at targetCoord;
    public bool ValidateShapesPlacement(Vector3Int targetCoord, List<IGridShape> gridShapes) {
        if (gridShapes.Count == 0) return true;

        Vector3Int lastShapeRootCoord = gridShapes[0].RootCoord;
        foreach (IGridShape gridShape in gridShapes) {
            targetCoord += gridShape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = gridShape.RootCoord;
            if (!ValidateShapePlacement(targetCoord, gridShape)) return false;
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

    public bool IsValidPlacement(Vector3Int coord) { return IsInBounds(coord) && IsOpen(coord); }
    public bool IsOpen(Vector3Int coord) { return !cells.ContainsKey(coord); }
    public bool IsInBounds(Vector3Int coord) { return coord.y < maxHeight && validCells.Contains(new Vector2Int(coord.x, coord.z)); }

    public bool GridIsEmpty() { return cells.Count == 0; }

    #endregion
}