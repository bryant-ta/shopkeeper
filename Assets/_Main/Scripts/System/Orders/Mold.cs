using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mold {
    public SO_OrderLayout OrderLayoutData { get; private set; }
    
    public ShapeData ShapeData { get; private set; }
    public Grid Grid { get; private set; }

    public Dictionary<Vector3Int, Color> GridColorRequirements { get; private set; }

    // NOTE: Mold is not Monobehavior to faciliate generating Mold data before Mold gameObject exists (in Orderer)
    public Mold(SO_OrderLayout orderLayoutData) {
        OrderLayoutData = orderLayoutData;
        ShapeData = orderLayoutData.GetShapeData();

        Dictionary<Vector3Int, int> d = orderLayoutData.GetTilesDict();
        GridColorRequirements = new();
        foreach (KeyValuePair<Vector3Int,int> kv in d) {
            GridColorRequirements.Add(kv.Key, Ledger.Instance.ColorPaletteData.Colors[kv.Value]);
        }
    }

    // params only available when Orderer is ready, so must separate from constructor
    public void InitByOrderer(Grid grid) {
        Grid = grid;

        int cwRandomRotationTimes = Random.Range(0, 4);
        // for (int i = 0; i < cwRandomRotationTimes; i++) {
        //     ShapeData.RotateShape(true);
        // }

        Grid.SetGridSize(ShapeData.Length, ShapeData.Height, ShapeData.Width); // NOTE: keep ahead of setting shape data root coord!

        // TEMP: scale orderer floor grid, replaced after orderer prefabs for different sizes
        Grid.transform.parent.Find("Floor").transform.localScale = new Vector3(0.1f * ShapeData.Length, 1, 0.1f * ShapeData.Width);
        
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

        // Remove grid cells to match shape data
        List<Vector2Int> moldCells = new();
        foreach (Vector3Int offset in ShapeData.ShapeOffsets) {
            moldCells.Add(new Vector2Int(ShapeData.RootCoord.x, ShapeData.RootCoord.z) + new Vector2Int(offset.x, offset.z));
        }
        List<Vector2Int> invertedMoldCells = Grid.ValidCells.Except(moldCells).ToList();
        foreach (Vector2Int coord in invertedMoldCells) {
            Grid.RemoveValidCell(coord);
        }
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