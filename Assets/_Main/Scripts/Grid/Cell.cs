using UnityEngine;

public class Cell {
    public Vector3Int Coord { get; }
    public IGridShape Shape;
    public Zone Zone;

    public Cell(Vector3Int coord, IGridShape shape, Zone zone = null) {
        Coord = coord;
        Shape = shape;
        Zone = zone;
    }

    public override bool Equals(object obj) {
        if (obj is Cell otherCell) { return Coord.Equals(otherCell.Coord); }
        return false;
    }
    public override int GetHashCode() { return Coord.GetHashCode(); }
}

public class Zone {
    public CellProperties CellProps { get; }

    public Zone(CellProperties cellProps) {
        CellProps = cellProps;
    }
}

public struct CellProperties {
    public bool CanPlace;
    public bool CanTake;
}