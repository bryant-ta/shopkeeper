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
            MakeOutline(shapeData, offset);
        }
    }

    void MakeOutline(ShapeData shapeData, Vector3Int cubeCoord) {
        // bot edges
        for (int d1 = 0; d1 < 4; d1++) {
            MakeEdgeLine(shapeData, (Direction) d1, cubeCoord);
        }
    }

    void MakeEdgeLine(ShapeData shapeData, Direction dir1, Vector3Int cubeCoord) {
        if (!shapeData.ContainsDir(cubeCoord, dir1)) {
            int d1 = (int) dir1;

            GameObject wallObj = Instantiate(cellOutlineWallObj, transform);
            wallObj.transform.localPosition = cubeCoord + DirectionData.DirectionVectors[d1] * 0.505f;
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
