using UnityEngine;

public class Mold {
    public ShapeData ShapeData { get; private set; }
    public Grid Grid { get; private set; }
    ShapeOutlineRenderer shapeOutlineRenderer;

    // NOTE: Mold is not Monobehavior to faciliate generating Mold data before Mold gameObject exists (in Orderer)
    public Mold(ShapeData shapeData) { ShapeData = shapeData; }

    // params only available when Orderer is ready, so must separate from constructor
    public void InitByOrderer(Grid grid, ShapeOutlineRenderer shapeOutlineRenderer) {
        Grid = grid;
        
        int cwRandomRotationTimes = Random.Range(0, 4);
        for (int i = 0; i < cwRandomRotationTimes; i++) {
            ShapeData.RotateShape(true);
        }

        // TEMP: scale orderer floor grid, replaced after orderer prefabs for different sizes
        Grid.SetGridSize(ShapeData.Length, ShapeData.Height, ShapeData.Width);
        Grid.transform.parent.Find("Floor").transform.localScale = new Vector3(
            0.1f * ShapeData.Length + 0.05f, 1, 0.1f * ShapeData.Width + 0.05f
        );
        ShapeData.RootCoord = new Vector3Int(grid.MinX, grid.MinY, grid.MinZ);

        switch (cwRandomRotationTimes) {
            case 0: // WS corner
                ShapeData.RootCoord = new Vector3Int(grid.MinX, grid.MinY, grid.MinZ);
                break;
            case 1: // NW corner
                ShapeData.RootCoord = new Vector3Int(grid.MinX, grid.MinY, grid.MaxZ);
                break;
            case 2: // EN corner
                ShapeData.RootCoord = new Vector3Int(grid.MaxX, grid.MinY, grid.MaxZ);
                break;
            case 3: // SE corner
                ShapeData.RootCoord = new Vector3Int(grid.MaxX, grid.MinY, grid.MinZ);
                break;
        }

        this.shapeOutlineRenderer = shapeOutlineRenderer;
        this.shapeOutlineRenderer.Render(ShapeData);
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