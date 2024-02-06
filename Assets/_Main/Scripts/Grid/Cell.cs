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
    public Vector3Int RootCoord { get; }
    public int Length { get; }
    public int Height { get; }
    public int Width { get; }

    public ZoneProperties ZoneProps { get; }

    public Zone(Vector3Int rootCoord, int length, int height, int width, ZoneProperties zoneProps) {
        RootCoord = rootCoord;
        Length = length;
        Height = height;
        Width = width;

        ZoneProps = zoneProps;
    }

    public Vector3Int[] AllCoords() {
        Vector3Int[] allCoords = new Vector3Int[Length * Width * Height];

        int i = 0;
        for (int x = 0; x < Length; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Width; z++) {
                    allCoords[i] = new Vector3Int(x, y, z);
                    i++;
                }
            }
        }

        return allCoords;
    }

    public Vector2Int[] XZCoords() {
        Vector2Int[] xzCoords = new Vector2Int[Length * Width];

        int i = 0;
        for (int x = 0; x < Length; x++) {
            for (int z = 0; z < Width; z++) {
                xzCoords[i] = new Vector2Int(x, z);
                i++;
            }
        }

        return xzCoords;
    }
}

public struct ZoneProperties {
    public bool CanPlace;
    public bool CanTake;
}