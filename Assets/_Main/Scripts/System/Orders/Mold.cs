using UnityEngine;

// TODO: consider just making part of Order
public class Mold {
    public ShapeData ShapeData { get; private set; }
    public Grid Grid { get; private set; }

    public Mold(ShapeData shapeData, Grid grid) {
        ShapeData = shapeData;
        Grid = grid;
    }

    public bool IsFullyOccupied() {
        foreach (Vector3Int offset in ShapeData.ShapeOffsets) {
            if (Grid.IsOpen(ShapeData.RootCoord + offset)) {
                return false;
            }
        }

        return true;
    }
}
