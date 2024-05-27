using UnityEngine;

// TODO: consider just making part of Order
public class Mold {
    public ShapeData ShapeData { get; private set; }
    public Grid Grid { get; private set; }
    MoldRenderer moldRenderer;

    public Mold(ShapeData shapeData) {
        ShapeData = shapeData;
    }

    public void Init(Grid grid, MoldRenderer moldRenderer) {
        Grid = grid;
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
