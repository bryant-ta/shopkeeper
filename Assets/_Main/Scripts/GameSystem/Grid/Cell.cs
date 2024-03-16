using UnityEngine;

public class Cell {
    public Vector3Int Coord { get; }
    public IGridShape Shape;

    public Cell(Vector3Int coord, IGridShape shape) {
        Coord = coord;
        Shape = shape;
    }

    public override bool Equals(object obj) {
        if (obj is Cell otherCell) { return Coord.Equals(otherCell.Coord); }

        return false;
    }
    public override int GetHashCode() { return Coord.GetHashCode(); }
}