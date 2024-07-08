using System.Collections.Generic;
using UnityEngine;

public class CellOutlineRenderer : MonoBehaviour {
    [SerializeField] GameObject cellOutlineWallObj;

    List<GameObject> cellOutlineWalls = new();

    public void Render(ShapeData shapeData) {
        // TODO: pooling instead of many instantiates
        // reset renderer
        Clear();

        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            if (offset.y == 0) {
                MakeCellOutline(shapeData, offset);
            }
        }
    }

    // Renders square outline with edges that are radius number of cells away from origin
    public void Render(Vector3Int origin, int radius) {
        // reset renderer
        Clear();
        
        List<Vector3Int> cells = new();
        for (int x = -radius; x <= radius; x++) {
            for (int z = -radius; z <= radius; z++) {
                cells.Add(new Vector3Int(x, 0, z));
            }
        }
        
        ShapeData shapeData = new ShapeData(ShapeDataID.None, origin, cells);
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            if (offset.y == 0) {
                MakeCellOutline(shapeData, offset);
            }
        }
    }

    void MakeCellOutline(ShapeData shapeData, Vector3Int cell) {
        // bot edges
        for (int d1 = 0; d1 < 4; d1++) {
            MakeEdgeLine(shapeData, (Direction) d1, cell);
        }
    }

    void MakeEdgeLine(ShapeData shapeData, Direction dir1, Vector3Int cubeCoord) {
        if (!shapeData.ContainsDir(cubeCoord, dir1)) {
            int d1 = (int) dir1;

            GameObject wallObj = Instantiate(cellOutlineWallObj, transform);
            wallObj.transform.localPosition = shapeData.RootCoord + cubeCoord + DirectionData.DirectionVectors[d1] * 0.505f;
            wallObj.transform.rotation = Quaternion.Euler(0, 180 + 90f * d1, 0);
            cellOutlineWalls.Add(wallObj);
        }
    }

    public void Clear() {
        for (int i = 0; i < cellOutlineWalls.Count; i++) {
            Destroy(cellOutlineWalls[i].gameObject);
        }
        cellOutlineWalls.Clear();
    }
}
