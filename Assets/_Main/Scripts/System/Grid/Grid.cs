using System;
using System.Collections.Generic;
using DG.Tweening;
using TriInspector;
using UnityEngine;

public class Grid : MonoBehaviour {
    [Title("Dimensions")]
    [InfoBox("Center defined as (0,0,0).")]
    [SerializeField] int length;
    public int Length => length;
    int height; // controlled thru GameManager config
    public int Height => height;
    [SerializeField] int width;
    public int Width => width;
    public int MinX => -length / 2;
    public int MinY => 0;
    public int MinZ => -width / 2;
    public int MaxX => length / 2;
    public int MaxY => height - 1;
    public int MaxZ => width / 2;

    [Title("Other")]
    [SerializeField] bool smoothPlaceMovement = true;

    Dictionary<Vector3Int, Cell> cells = new();
    public Dictionary<Vector3Int, Cell> Cells => cells;

    List<Zone> zones = new();
    HashSet<Vector2Int> validCells = new();

    public event Action<List<IGridShape>> OnPlaceShapes;  // triggered once per move of a STACK of shape on both origin and target grid
    public event Action<List<IGridShape>> OnRemoveShapes; // triggered once per move of a STACK of shape on both origin and target grid

    // Requires Init at Start since requires IGridShape setup which occurs in Awake. This also means everything relying on Grid can
    // only occur in Start. Thus, Grid Start is executed before most other gameObjects.
    void Start() { Init(); }

    void Init() {
        // Set grid bounds
        // actual length/width rounds to odd num due to centering on (0,0,0)
        for (int x = MinX; x <= MaxX; x++) {
            for (int z = MinZ; z <= MaxZ; z++) {
                validCells.Add(new Vector2Int(x, z));
            }
        }

        height = GameManager.Instance.GlobalGridHeight;

        // Add pre-existing scene shapes to grid
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).childCount == 0) continue;
            if (transform.GetChild(i).GetChild(0).TryGetComponent(out IGridShape shape)) {
                if (!PlaceShape(Vector3Int.FloorToInt(shape.ShapeTransform.position), shape, true)) {
                    Debug.LogError("Unable to place shape. Pre-existing scene shape overlaps with another shape in grid.");
                }
            }
        }
    }

    #region Manipulation

    public bool PlaceShape(Vector3Int targetCoord, IGridShape shape, bool ignoreZone = false) {
        if (!ValidateShapePlacement(targetCoord, shape, ignoreZone).IsValid) return false;
        PlaceShapeNoValidate(targetCoord, shape);

        return true;
    }
    public void PlaceShapeNoValidate(Vector3Int targetCoord, IGridShape shape) {
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cells[targetCoord + offset] = new Cell(targetCoord + offset, shape);
        }

        DOTween.Kill(shape.ShapeTransform.GetInstanceID() + TweenManager.PlaceShapeID);

        shape.ShapeTransform.SetParent(transform, true);
        shape.ShapeData.RootCoord = targetCoord;

        if (smoothPlaceMovement) {
            for (var i = 0; i < shape.Colliders.Count; i++) {
                shape.Colliders[i].enabled = false;
            }

            Sequence seq = DOTween.Sequence().SetId(shape.ShapeTransform.GetInstanceID() + TweenManager.PlaceShapeID);
            seq.Append(shape.ShapeTransform.DOLocalMove(targetCoord, TweenManager.PlaceShapeDur));
            seq.Play().OnComplete(
                () => {
                    for (var i = 0; i < shape.Colliders.Count; i++) {
                        shape.Colliders[i].enabled = true;
                    }
                }
            );
        } else {
            shape.ShapeTransform.localPosition = targetCoord;
        }

        SoundManager.Instance.PlaySound(SoundID.ProductPlace);
    }

    // Returns false if any placement of shape in shapes is invalid.
    // Placement is relative to first shape's root coord placed at targetCoord.
    public bool PlaceShapes(Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        if (shapes.Count == 0) return false;
        if (!ValidateShapesPlacement(targetCoord, shapes, ignoreZone).IsValid) return false;

        // Shapes must be sorted by y value or targetCoord offset calculation will add in the wrong direction!
        // TODO: move if actually has performance impact
        shapes.Sort((a, b) => a.ShapeData.RootCoord.y.CompareTo(b.ShapeData.RootCoord.y));

        Vector3Int lastShapeRootCoord = shapes[0].ShapeData.RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.ShapeData.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.ShapeData.RootCoord;
            PlaceShapeNoValidate(targetCoord, shape);
        }
        
        OnPlaceShapes?.Invoke(shapes);

        return true;
    }

    public bool MoveShapes(Grid targetGrid, Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        if (shapes == null || shapes.Count == 0) {
            Debug.LogWarning("MoveShapes was called with empty/null shapes list.");
            return false;
        }

        // Check shape move rules
        if (!ignoreZone) {
            for (int i = 0; i < shapes.Count; i++) {
                if (!CheckZones(shapes[i].ShapeData.RootCoord, prop => prop.CanTake)) {
                    return false;
                }
            }
        }

        for (int i = 0; i < shapes.Count; i++) {
            if (!shapes[i].ShapeTags.CheckMoveTags()) return false;
        }

        // Save original shape coords in original grid, remove from original grid
        List<Vector3Int> origRootCoords = new();
        for (int i = 0; i < shapes.Count; i++) {
            origRootCoords.Add(shapes[i].ShapeData.RootCoord);
            RemoveShapeCells(shapes[i], false);
        }

        if (!targetGrid.PlaceShapes(targetCoord, shapes, ignoreZone)) {
            // Replace shapes in original position if new placement failed
            for (int i = 0; i < shapes.Count; i++) {
                PlaceShape(origRootCoords[i], shapes[i], true);
            }

            return false;
        }

        OnRemoveShapes?.Invoke(shapes);

        return true;
    }

    public void DestroyShape(IGridShape shape) {
        RemoveShapeCells(shape, true);

        // TODO: prob call IGridShape cleanup tasks on its destruction
        shape.DestroyShape();
    }

    /// <summary>
    /// Removes shapes from grid.
    /// </summary>
    /// <param name="coord">Original coord of shape to remove. Can be different from that shape's current RootCoord,
    /// such as when the shape was just placed in the new grid, but still needs to be removed from the old.</param>
    /// <param name="shape">Shape of cells to remove</param>
    /// <param name="triggerAllFall">If false, shapes directly above coord will ignore falling. Set false to correctly move
    /// a stack of shapes.</param>
    void RemoveShapeCells(IGridShape shape, bool triggerAllFall) {
        Queue<Vector3Int> gapCoords = new();
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cells.Remove(shape.ShapeData.RootCoord + offset);
            gapCoords.Enqueue(shape.ShapeData.RootCoord + offset);
        }

        // Trigger falling for any shapes above removed shape cells
        while (triggerAllFall && gapCoords.Count > 0) {
            Vector3Int aboveCoord = gapCoords.Dequeue() + Vector3Int.up;
            if (IsInBounds(aboveCoord) && !IsOpen(aboveCoord)) {
                // Check every cell beneath the above shape is open
                IGridShape aboveShape = cells[aboveCoord].Shape;
                bool canFall = false;
                foreach (var offset in aboveShape.ShapeData.ShapeOffsets) {
                    if (IsOpen(aboveCoord + offset + Vector3Int.down)) {
                        canFall = true;
                    } else {
                        canFall = false;
                        break;
                    }
                }

                if (canFall) {
                    // Will recursively cause all shapes above to fall as well
                    MoveShapes(this, aboveShape.ShapeData.RootCoord + Vector3Int.down, new List<IGridShape> {aboveShape}, true);
                    gapCoords.Enqueue(aboveCoord);
                }
            }
        }
    }

    // no validation
    public void RotateShapes(List<IGridShape> shapes, bool clockwise) {
        for (int i = 0; i < shapes.Count; i++) {
            RemoveShapeCells(shapes[i], false);
        }

        for (int i = 0; i < shapes.Count; i++) {
            shapes[i].ShapeData.RotateShape(clockwise);
            PlaceShapeNoValidate(shapes[i].ShapeData.RootCoord, shapes[i]);
        }
    }

    #endregion

    #region Selection

    /// <summary>
    /// Selects stack of shapes.
    /// </summary>
    /// <param name="coord">Coord of shape at base of stack. This shape is used for stack's footprint.</param>
    /// <param name="shapeOutOfFootprint">Set if shape is outside of footprint.</param>
    /// <returns>List of shape stack. Null if a shape above base is outside stack footprint or if coord does not contain a shape.</returns>
    public List<IGridShape> SelectStackedShapes(Vector3Int coord, out IGridShape shapeOutOfFootprint) {
        shapeOutOfFootprint = null;
        if (!IsInBounds(coord) || IsOpen(coord)) {
            return null;
        }

        IGridShape shape = cells[coord].Shape;

        List<IGridShape> stackedShapes = new();  // Return list of shape stack
        Queue<Vector3Int> cellsToCheck = new();  // Work queue for cells to recursively check shapes stack on top of cell
        List<Vector2Int> stackFootprint = new(); // Tracks cells of bottom shape of stack, the "footprint"

        // Determine footprint, enqueue every cell above footprint for checking for stacked shapes
        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            cellsToCheck.Enqueue(shape.ShapeData.RootCoord + offset);
            stackFootprint.Add(new Vector2Int(shape.ShapeData.RootCoord.x + offset.x, shape.ShapeData.RootCoord.z + offset.z));
        }

        stackedShapes.Add(shape);

        while (cellsToCheck.Count > 0) {
            Vector3Int checkCoord = cellsToCheck.Dequeue();
            checkCoord += Vector3Int.up; // Search above check cell

            if (IsInBounds(checkCoord) && !IsOpen(checkCoord)) {
                shape = cells[checkCoord].Shape;

                if (!stackedShapes.Contains(shape)) { // Add shape if new
                    foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
                        // Current shape falls outside stack footprint
                        if (!stackFootprint.Contains(
                                new Vector2Int(shape.ShapeData.RootCoord.x + offset.x, shape.ShapeData.RootCoord.z + offset.z)
                            )) {
                            shapeOutOfFootprint = shape;
                            return null;
                        }

                        cellsToCheck.Enqueue(checkCoord + offset); // Add cells of current shape to the check queue
                    }

                    stackedShapes.Add(shape);
                }
            }
        }

        return stackedShapes;
    }

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
        for (int y = 0; y < height; y++) {
            Vector3Int coord = new Vector3Int(x, y, z);
            if (IsOpen(coord)) {
                lowestOpenY = coord.y;
                return true;
            }
        }

        lowestOpenY = -1;
        return false;
    }

    /// <summary>
    /// Returns true if valid highest open cell exists in column at (x, z). Starts searching from input cell downwards until hitting
    /// any occupied cell or the floor
    /// </summary>
    public bool SelectLowestOpenFromCell(Vector3Int startCell, out int lowestOpenY) {
        for (int y = startCell.y; y >= -1; y--) {
            Vector3Int coord = new Vector3Int(startCell.x, y, startCell.z);

            if (!IsOpen(coord) || y == -1) {
                coord.y++;

                if (!IsInBounds(coord) || !IsOpen(coord)) break;

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
    public PlacementValidation ValidateShapePlacement(Vector3Int targetCoord, IGridShape shape, bool ignoreZone = false) {
        PlacementValidation pv = new PlacementValidation();
        if (shape == null) {
            Debug.LogError("Cannot validate shape placement: shape is null");
            return pv;
        }

        if (!shape.ShapeTags.CheckPlaceTags(targetCoord)) {
            pv.SetFlag(PlacementInvalidFlag.ShapeTagRule);
            return pv;
        }

        foreach (Vector3Int offset in shape.ShapeData.ShapeOffsets) {
            Vector3Int checkPos = new Vector3Int(targetCoord.x + offset.x, targetCoord.y + offset.y, targetCoord.z + offset.z);
        
            if (!IsInBoundsXZ(checkPos)) { pv.SetFlag(PlacementInvalidFlag.OutOfBoundsXZ); }
            if (!IsInBoundsY(checkPos)) { pv.SetFlag(PlacementInvalidFlag.OutOfBoundsY); }
            if (!IsOpen(checkPos)) { pv.SetFlag(PlacementInvalidFlag.Overlap); }
            if (!ignoreZone && !CheckZones(checkPos, prop => prop.CanPlace)) { pv.SetFlag(PlacementInvalidFlag.ZoneRule); }

            if (!pv.IsValid) break;
        }

        return pv;
    }

    /// <summary>
    /// Validates placement of shapes in input list using current positioning in their current grid.
    /// </summary>
    /// <param name="targetCoord">First shape's root coord</param>
    /// <param name="shapes">List of shapes to validate</param>
    /// <param name="ignoreZone">Ignore zone rules, optional</param>
    /// <returns>List of PlacementValidation, indexed in same order as input shapes list</returns>
    /// <remarks>Placement is relative to first shape's root coord placed at targetCoord;</remarks>
    public PlacementValidations ValidateShapesPlacement(Vector3Int targetCoord, List<IGridShape> shapes, bool ignoreZone = false) {
        PlacementValidations validations = new();
        if (shapes.Count == 0) return validations;

        // Shapes must be sorted by y value or targetCoord offset calculation will add in the wrong direction!
        // NOTE: move if actually has performance impact
        shapes.Sort((a, b) => a.ShapeData.RootCoord.y.CompareTo(b.ShapeData.RootCoord.y));

        Vector3Int lastShapeRootCoord = shapes[0].ShapeData.RootCoord;
        foreach (IGridShape shape in shapes) {
            targetCoord += shape.ShapeData.RootCoord - lastShapeRootCoord;
            lastShapeRootCoord = shape.ShapeData.RootCoord;
            validations.ValidationList.Add(ValidateShapePlacement(targetCoord, shape, ignoreZone));
        }

        return validations;
    }

    // bool ValidateShapeRotate(IGridShape shape, bool clockwise, bool ignoreZone = false) {
    //     if (shape == null) {
    //         Debug.LogError("Cannot validate shape placement: shape is null");
    //         return false;
    //     }
    //     
    //     ShapeData rotatedShapeData = shape.GetShapeDataRotated(clockwise);
    //     
    //     foreach (Vector3Int offset in rotatedShapeData.ShapeOffsets) {
    //         Vector3Int checkPos = new Vector3Int(shape.RootCoord.x + offset.x, shape.RootCoord.y + offset.y, shape.RootCoord.z + offset.z);
    //         if (!IsValidPlacement(checkPos, ignoreZone)) {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
    // bool ValidateShapesRotate(List<IGridShape> shapes, bool clockwise, bool ignoreZone = false) {
    //     if (shapes == null || shapes.Count == 0) {
    //         Debug.LogError("Cannot validate shapes rotated placement: shapes is null/empty");
    //         return false;
    //     }
    //
    //     List<ShapeData> rotatedShapesData = new();
    //     for (int i = 0; i < shapes.Count; i++) {
    //         rotatedShapesData.Add(shapes[i].GetShapeDataRotated(clockwise));
    //     }
    //
    //     for (int i = 0; i < rotatedShapesData.Count; i++) {
    //         foreach (Vector3Int offset in rotatedShapesData[i].ShapeOffsets) {
    //             Vector3Int checkPos = new Vector3Int(shapes[i].RootCoord.x + offset.x, shape.RootCoord.y + offset.y, shape.RootCoord.z + offset.z);
    //             if (!IsValidPlacement(checkPos, ignoreZone)) {
    //                 return false;
    //             }
    //         }
    //         
    //     }
    //
    //     return true;
    // }

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
    /// - Note: currently does not update length/width bc only used in initial grid setup
    /// </summary>
    public void AddValidCellsRange(Vector2Int startPos, Vector2Int endPos) {
        for (int x = startPos.x; x <= endPos.x; x++) {
            for (int y = startPos.y; y <= endPos.y; y++) {
                validCells.Add(new Vector2Int(x, y));
            }
        }
    }

    public void SetMaxHeight(int height) { this.height = height; }

    #endregion

    #region Helper

    public bool IsOpen(Vector3Int coord) { return !cells.ContainsKey(coord); }
    public bool IsInBounds(Vector3Int coord) { return IsInBoundsY(coord) && IsInBoundsXZ(coord); }
    bool IsInBoundsXZ(Vector3Int coord) { return validCells.Contains(new Vector2Int(coord.x, coord.z)); }
    bool IsInBoundsY(Vector3Int coord) { return coord.y < height; }

    public bool IsEmpty() { return cells.Count == 0; }

    public List<IGridShape> AllShapes() {
        List<IGridShape> shapes = new();
        foreach (Cell cell in cells.Values) {
            if (shapes.Contains(cell.Shape)) continue;

            shapes.Add(cell.Shape);
        }

        shapes.Sort((a, b) => a.ShapeData.RootCoord.y.CompareTo(b.ShapeData.RootCoord.y));

        return shapes;
    }

    #endregion

    public class PlacementValidations {
        public List<PlacementValidation> ValidationList = new();

        public bool IsValid {
            get {
                for (int i = 0; i < ValidationList.Count; i++) {
                    if (!ValidationList[i].IsValid) return false;
                }

                return true;
            }
        }
    }

    public struct PlacementValidation {
        PlacementInvalidFlag flags;
        public bool IsValid => (flags | 0) == 0;

        public void SetFlag(PlacementInvalidFlag flag) { flags |= flag; }
        public void UnsetFlag(PlacementInvalidFlag flag) { flags &= ~flag; }
        public bool HasFlag(PlacementInvalidFlag flag) { return (flags & flag) == flag; }
    }

    [Flags]
    public enum PlacementInvalidFlag {
        None = 0,
        OutOfBoundsXZ = 1 << 0,
        OutOfBoundsY = 1 << 1,
        Overlap = 1 << 2,
        ZoneRule = 1 << 3,
        ShapeTagRule = 1 << 4,
    }

    // TEMP: prob, until think of better way with shaders to do invalid/overlap feedbakc
    public void ChangeColorAllShapes(Color color) {
        List<IGridShape> shapes = AllShapes();
        for (int i = 0; i < shapes.Count; i++) {
            // Ensure the object has a renderer
            if (shapes[i].ColliderTransform.TryGetComponent<Renderer>(out Renderer rd)) {
                rd.material.SetColor("_BaseColor", color);
            } else {
                Debug.LogError("Shape is missing a renderer.");
            }
        }
    }
}