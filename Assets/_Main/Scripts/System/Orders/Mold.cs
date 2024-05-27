using UnityEngine;

// TODO: consider just making part of Order
public class Mold {
    public ShapeData ShapeData { get; private set; }
    public Grid Grid { get; private set; }
    MoldRenderer moldRenderer;

    public Mold(ShapeData shapeData) {
        ShapeData = shapeData;
    }

    // params only available when Orderer is ready, so must separate from constructor
    public void InitByOrderer(Grid grid, MoldRenderer moldRenderer) {
        Grid = grid;
        ShapeData.RootCoord = new Vector3Int(grid.MinX, grid.MinY, grid.MinZ);
        int cwRandomRotationTimes = Random.Range(0, 4);
        for (int i = 0; i < cwRandomRotationTimes; i++) {
            ShapeData.RotateShape(true);
        }

        this.moldRenderer = moldRenderer;
        this.moldRenderer.Render(ShapeData);
    }

    public bool IsFullyOccupied() {
        if (Grid == null) {
            Debug.LogError("Mold grid is not set.");
            return false;
        }
        
        foreach (Vector3Int offset in ShapeData.ShapeOffsets) {
            if (Grid.IsOpen(ShapeData.RootCoord + offset)) {
                return false;
            }
        }

        return true;
    }
}
