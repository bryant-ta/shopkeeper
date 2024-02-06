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

    Dictionary<Vector3Int, Cell> cells = new();

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
            if (transform.GetChild(i).GetChild(0).TryGetComponent(out IGridShape shape)) {
                if (!PlaceShape(shape.RootCoord, shape)) {
                    Debug.LogError("Unable to place shape. Pre-existing scene shape overlaps with another shape in grid.");
                }
            } else {
                Debug.LogErrorFormat("Only shapes with IGridShape component should be a child of Grid. ({0})", transform.GetChild(i).name);
            }
        }
    }

    #region Manipulation

    public bool PlaceShape(Vector3Int targetCoord, IGridShape shape) {
        if (!ValidateShapePlacement(targetCoord, shape)) return false;
        PlaceShapeNoValidate(targetCoord, shape);

        return true;
    }
    void PlaceShapeNoValidate(Vector3Int targetCoord, IGridShape shape) {
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cells[targetCoord + offset] = new Cell(targetCoord + offset, shape);
        }

        shape.ShapeTransform.SetParent(transform);
        shape.ShapeTransform.localPosition = targetCoord;
        shape.ShapeTransform.localRotation = Quaternion.identity;
    }

    // Returns false if any placement of shape in shapes is invalid.
    // Placement is relative to first shape's root coord placed at targetCoord.
    public bool PlaceShapes(Vector3Int targetCoord, List<IGridShape> shapes) {
        if (shapes.Count == 0) return false;
        if (!ValidateShapesPlacement(targetCoord, shapes)) return false;

        Vector3Int lastShapeRootCoord = shapes[0].RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.RootCoord;
            PlaceShapeNoValidate(targetCoord, shape);
        }

        return true;
    }

    public bool MoveShapes(Grid targetGrid, Vector3Int targetCoord, List<IGridShape> shapes) {
        // Save original shape coords in original grid for removal
        List<Vector3Int> origRootCoords = new();
        for (int i = 0; i < shapes.Count; i++) {
            origRootCoords.Add(shapes[i].RootCoord);
        }

        if (!targetGrid.PlaceShapes(targetCoord, shapes)) return false;

        for (int i = 0; i < shapes.Count; i++) {
            RemoveShapeCells(origRootCoords[i], shapes[i]);
        }

        return true;
    }

    public void DestroyShape(IGridShape shape) {
        RemoveShapeCells(shape.RootCoord, shape);

        // TODO: prob let IGridShape handle its destruction, just call that on shape
        Destroy(shape.ShapeTransform.gameObject);
    }

    // Set exactly one cell
    public bool SetCoord(Vector3Int coord, IGridShape shape) {
        if (!IsValidPlacement(coord)) return false;

        cells[coord] = new Cell(coord, shape);
        return true;
    }
    // Remove exactly one cell
    public void RemoveCoord(Vector3Int coord) {
        if (IsOpen(coord) || !IsInBounds(coord)) return;

        cells.Remove(coord);
    }
    void RemoveShapeCells(Vector3Int rootCoord, IGridShape shape) {
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
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
            IGridShape shape = cells[coord].Shape;

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
        return cells[coord].Shape;
    }

    public IGridShape SelectOffset(Vector3Int origin, Vector3Int offset) {
        Vector3Int targetCoord = new Vector3Int(origin.x + offset.x, origin.y + offset.y, origin.z + offset.z);
        if (!IsInBounds(targetCoord)) return null;

        return cells[targetCoord].Shape;
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
    public bool ValidateShapePlacement(Vector3Int targetCoord, IGridShape shape) {
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            Vector3Int checkPos = new Vector3Int(targetCoord.x + offset.x, targetCoord.y + offset.y, targetCoord.z + offset.z);
            if (!IsValidPlacement(checkPos)) {
                return false;
            }
        }

        return true;
    }

    // Validates placement of shapes in input list using current positioning in their current grid.
    // Placement is relative to first shape's root coord placed at targetCoord;
    public bool ValidateShapesPlacement(Vector3Int targetCoord, List<IGridShape> shapes) {
        if (shapes.Count == 0) return true;

        Vector3Int lastShapeRootCoord = shapes[0].RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.RootCoord;
            if (!ValidateShapePlacement(targetCoord, shape)) return false;
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