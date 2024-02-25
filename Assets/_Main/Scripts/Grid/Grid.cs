using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TriInspector;
using UnityEngine;

public class Grid : MonoBehaviour {
    [InfoBox("Min LHW defined as -max LHW.\nCenter defined as (0,0,0).")]
    [SerializeField] int maxLength;
    public int MaxLength => maxLength;
    [SerializeField] int maxHeight;
    public int MaxHeight => maxHeight;
    [SerializeField] int maxWidth;
    public int MaxWidth => maxWidth;

    [SerializeField] bool smoothPlaceMovement = true;

    public Dictionary<Vector3Int, Cell> Cells => cells;
    [SerializeField] Dictionary<Vector3Int, Cell> cells = new();

    List<Zone> zones = new();
    HashSet<Vector2Int> validCells = new();

    // Requires Init at Start since requires IGridShape setup which occurs in Awake. This also means everything relying on Grid can
    // only occur in Start. Thus, Grid Start is executed before most other gameObjects.
    void Start() { Init(maxLength, maxHeight, maxWidth); }

    void Init(int maxLength, int maxHeight, int maxWidth) {
        this.maxLength = maxLength;
        this.maxHeight = maxHeight;
        this.maxWidth = maxWidth;

        // Set grid bounds
        // actual length/width rounds to odd num due to centering on (0,0,0)
        for (int x = -maxLength / 2; x <= maxLength / 2; x++) {
            for (int z = -maxWidth / 2; z <= maxWidth / 2; z++) {
                validCells.Add(new Vector2Int(x, z));
            }
        }

        // Add pre-existing scene shapes to grid
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).childCount == 0) continue;
            if (transform.GetChild(i).GetChild(0).TryGetComponent(out IGridShape shape)) {
                if (!PlaceShape(shape.RootCoord, shape, true)) {
                    Debug.LogError("Unable to place shape. Pre-existing scene shape overlaps with another shape in grid.");
                }
            }
        }
    }

    #region Manipulation

    public bool PlaceShape(Vector3Int targetCoord, IGridShape shape, bool ignoreZone = false) {
        if (!ValidateShapePlacement(targetCoord, shape, ignoreZone)) return false;
        PlaceShapeNoValidate(targetCoord, shape);

        return true;
    }
    void PlaceShapeNoValidate(Vector3Int targetCoord, IGridShape shape) {
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cells[targetCoord + offset] = new Cell(targetCoord + offset, shape);
        }
        
        DOTween.Kill(shape.ShapeTransform.GetInstanceID() + TweenManager.PlaceShapeID);
        
        shape.ShapeTransform.SetParent(transform, true);
        shape.RootCoord = targetCoord;

        if (smoothPlaceMovement) {
            shape.Collider.enabled = false;
        
            Sequence seq = DOTween.Sequence().SetId(shape.ShapeTransform.GetInstanceID() + TweenManager.PlaceShapeID);
            seq.Append(shape.ShapeTransform.DOLocalMove(targetCoord, TweenManager.PlaceShapeDur));
            seq.Join(shape.ShapeTransform.DOLocalRotateQuaternion(Quaternion.identity, TweenManager.PlaceShapeDur));
            seq.Play().OnComplete(() => {
                    shape.Collider.enabled = true;
                }
            );
        } else {
            shape.ShapeTransform.localPosition = targetCoord;
            shape.ShapeTransform.localRotation = Quaternion.identity;
        }
    }

    // Returns false if any placement of shape in shapes is invalid.
    // Placement is relative to first shape's root coord placed at targetCoord.
    public bool PlaceShapes(Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        if (shapes.Count == 0) return false;
        if (!ValidateShapesPlacement(targetCoord, shapes, ignoreZone)) return false;

        Vector3Int lastShapeRootCoord = shapes[0].RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.RootCoord;
            PlaceShapeNoValidate(targetCoord, shape);
        }

        return true;
    }

    public bool MoveShapes(Grid targetGrid, Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        if (!ignoreZone) {
            for (int i = 0; i < shapes.Count; i++) {
                if (!CheckZones(shapes[i].RootCoord, prop => prop.CanTake)) {
                    return false;
                }
            }
        }

        // Save original shape coords in original grid for removal
        List<Vector3Int> origRootCoords = new();
        for (int i = 0; i < shapes.Count; i++) {
            origRootCoords.Add(shapes[i].RootCoord);
        }

        if (!targetGrid.PlaceShapes(targetCoord, shapes, ignoreZone)) return false;

        for (int i = 0; i < shapes.Count; i++) {
            RemoveShapeCells(origRootCoords[i], shapes[i], false);
        }

        return true;
    }

    public void DestroyShape(IGridShape shape) {
        RemoveShapeCells(shape.RootCoord, shape, true);

        // TODO: prob call IGridShape cleanup tasks on its destruction
        shape.DestroyShape();
    }

    // Set exactly one cell
    public bool SetCoord(Vector3Int coord, IGridShape shape) {
        if (!IsValidPlacement(coord, true)) return false;

        cells[coord] = new Cell(coord, shape);
        return true;
    }
    // Remove exactly one cell
    public void RemoveCoord(Vector3Int coord) {
        if (IsOpen(coord) || !IsInBounds(coord)) return;

        cells.Remove(coord);
    }

    /// <summary>
    /// Removes shapes from grid.
    /// </summary>
    /// <param name="coord">Original coord of shape to remove. Can be different from that shape's current RootCoord,
    /// such as when the shape was just placed in the new grid, but still needs to be removed from the old.</param>
    /// <param name="shape">Shape of cells to remove</param>
    /// <param name="triggerAllFall">If false, shapes directly above coord will ignore falling. Set false to correctly move
    /// a stack of shapes.</param>
    void RemoveShapeCells(Vector3Int coord, IGridShape shape, bool triggerAllFall) {
        Queue<Vector3Int> gapCoords = new();
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cells.Remove(coord + offset);
            gapCoords.Enqueue(coord + offset);
        }

        // Trigger falling for any shapes above removed shape cells
        while (gapCoords.Count > 0) {
            Vector3Int aboveCoord = gapCoords.Dequeue() + Vector3Int.up;
            if (IsInBounds(aboveCoord) && !IsOpen(aboveCoord)) {
                // Check every cell beneath the above shape is open
                IGridShape aboveShape = cells[aboveCoord].Shape;
                bool canFall = false;
                foreach (var offset in aboveShape.ShapeData.ShapeOffsets) {
                    if (!triggerAllFall && offset == Vector3Int.zero) continue; // no fall for cells directly above input coord
                    
                    if (IsOpen(aboveCoord + offset + Vector3Int.down)) {
                        canFall = true;
                    }
                    else {
                        canFall = false;
                        break;
                    }
                }

                if (canFall) {
                    // Will recursively cause all shapes above to fall as well
                    MoveShapes(this, aboveShape.RootCoord + Vector3Int.down, new List<IGridShape> {aboveShape}, true);
                    gapCoords.Enqueue(aboveCoord);
                }
            }
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
    public bool ValidateShapePlacement(Vector3Int targetCoord, IGridShape shape, bool ignoreZone = false) {
        if (shape == null) {
            Debug.LogError("Cannot validate shape placement: shape is null");
            return false;
        }

        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            Vector3Int checkPos = new Vector3Int(targetCoord.x + offset.x, targetCoord.y + offset.y, targetCoord.z + offset.z);
            if (!IsValidPlacement(checkPos, ignoreZone)) {
                return false;
            }
        }

        return true;
    }

    // Validates placement of shapes in input list using current positioning in their current grid.
    // Placement is relative to first shape's root coord placed at targetCoord;
    public bool ValidateShapesPlacement(Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        if (shapes.Count == 0) return true;

        Vector3Int lastShapeRootCoord = shapes[0].RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.RootCoord;
            if (!ValidateShapePlacement(targetCoord, shape, ignoreZone)) return false;
        }

        return true;
    }

    #endregion

    #region Zones

    public void AddZone(Zone zone) {
        foreach (Vector3Int coord in zone.AllCoords) {
            if (!IsInBounds(coord)) {
                Debug.Log("Cannot add zone to grid: out of bounds.");
                return;
            }
        }

        zones.Add(zone);
    }
    public void RemoveZone(Zone zone) { zones.Remove(zone); }

    /// <summary>
    /// Returns true if coord is within a zone and matches zone properties OR coord is outside applicable zones.
    /// </summary>
    /// <example>CheckZone(coord, prop => prop.CanPlace, prop => prop.CanTake, ...)</example>
    /// <remarks>
    /// Checking prop is false actually requires !CheckZone(coord, prop => prop.CanPlace)
    ///    NOT CheckZone(coord, prop => !prop.CanPlace)
    ///    This is due to CheckZones returns true when coord is not in any zone.
    /// TODO: this is confusing, possibly not working when checking multiple props at once.
    /// </remarks>
    bool CheckZones(Vector3Int coord, params Func<ZoneProperties, bool>[] props) {
        for (int i = 0; i < zones.Count; i++) {
            if (zones[i].AllCoords.Contains(coord)) {
                foreach (Func<ZoneProperties, bool> prop in props) {
                    if (!prop(zones[i].ZoneProps)) return false;
                }
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

    public bool IsValidPlacement(Vector3Int coord, bool ignoreZone = false) {
        return IsOpen(coord) && IsInBounds(coord) && (ignoreZone || CheckZones(coord, prop => prop.CanPlace));
    }
    public bool IsOpen(Vector3Int coord) { return !cells.ContainsKey(coord); }
    public bool IsInBounds(Vector3Int coord) { return coord.y < maxHeight && validCells.Contains(new Vector2Int(coord.x, coord.z)); }

    public bool GridIsEmpty() { return cells.Count == 0; }

    public List<IGridShape> AllShapes() {
        List<IGridShape> shapes = new();
        foreach (Cell cell in cells.Values) {
            shapes.Add(cell.Shape);
        }

        return shapes;
    }

    #endregion
}