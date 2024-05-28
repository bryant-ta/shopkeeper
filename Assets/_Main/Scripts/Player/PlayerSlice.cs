using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlice : MonoBehaviour, IPlayerTool {
    [SerializeField] GameObject lineRendererPrefab;

    List<LineRenderer> lineRendererPool = new(); // TODO: make slice preview pooled
    
    Grid targetGrid;

    void Slice(ClickInputArgs clickInputArgs) { }

    void SlicePreview(ClickInputArgs clickInputArgs) {
        if (!SelectTargetGrid(clickInputArgs)) {
            return;
        }

        // formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));
        
        Vector3 cellToHitPoint = localHitPoint - selectedCellCoord;
        
        // hit point must be on x parallel face, otherwise is z parallel
        bool isXFace = Math.Abs(cellToHitPoint.z) > Math.Abs(cellToHitPoint.x) ? true : false;
        
        // midpoint between the two cell centers for initial slice
        Vector3 sliceFirstCellPos = isXFace ?
            cellToHitPoint + new Vector3(0.5f * Math.Sign(cellToHitPoint.x), 0, 0) :
            cellToHitPoint + new Vector3(0, 0, 0.5f * Math.Sign(cellToHitPoint.x));
        
        // draw slice preview line around slice edge
        
        // search for further cells to slice (only where both sides occupied, otherwise stop)
        ShapeData shapeData = targetGrid.SelectPosition(selectedCellCoord).ShapeData;
        MakeSliceOutline(shapeData, selectedCellCoord);
    }
    public void Render(ShapeData shapeData) {
        transform.localPosition = shapeData.RootCoord + new Vector3(0, 0.55f, 0); // should move this...
        
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            MakeCubeOutline(shapeData, offset);
        }
    }

    void MakeSliceOutline(ShapeData shapeData, Vector3Int cubeCoord) {
        // succinctly code this area..... for a whole bunch of xz cases
        
        
        // side edges
        for (int d1 = 0; d1 < 4; d1++) {
            int d2 = d1 - 1;
            if (d2 < 0) d2 = 3;
            MakeEdgeLine(shapeData, (Direction) d1, (Direction) d2, cubeCoord);
        }

        // top/bot edges
        for (int d2 = 0; d2 < 4; d2++) {
            MakeEdgeLine(shapeData, Direction.Up, (Direction) d2, cubeCoord);
            MakeEdgeLine(shapeData, Direction.Down, (Direction) d2, cubeCoord);
        }
    }

    void MakeEdgeLine(ShapeData shapeData, Direction dir1, Direction dir2, Vector3Int cubeCoord) {
        if ((shapeData.NeighborExists(cubeCoord, dir1) // side elbow || side/top/bot corner
             && shapeData.NeighborExists(cubeCoord, dir2)
             && !shapeData.NeighborExists(cubeCoord + CubeMeshData.DirectionVectorsInt[(int) dir1], dir2)) ||
            (!shapeData.NeighborExists(cubeCoord, dir1) && !shapeData.NeighborExists(cubeCoord, dir2))) {
            int d1 = (int) dir1;
            int d2 = (int) dir2;
            int i = dir1 switch {
                Direction.Up => d1 + d2,
                Direction.Down => d1 + d2 + 3,
                _ => d1
            };

            float length = 0.5f;
            Vector3 startPoint = cubeCoord + CubeMeshData.vertices[CubeMeshData.bevelEdges[i][0]] * length;
            Vector3 endPoint = cubeCoord + CubeMeshData.vertices[CubeMeshData.bevelEdges[i][1]] * length;

            GameObject lineObject = Instantiate(lineRendererPrefab, transform);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    void DetermineQuadrant(ClickInputArgs clickInputArgs) { }

    // TEMP: will consider moving to Player to consolidate among Player Tools
    GameObject lastHitObj;
    bool SelectTargetGrid(ClickInputArgs clickInputArgs) {
        if (clickInputArgs.TargetObj != lastHitObj) {
            lastHitObj = clickInputArgs.TargetObj;
            if (clickInputArgs.TargetObj.TryGetComponent(out GridFloorHelper gridFloor)) {
                targetGrid = gridFloor.Grid;
            } else if (clickInputArgs.TargetObj.TryGetComponent(out IGridShape shape)) {
                targetGrid = shape.Grid;
            } else {
                targetGrid = null;
            }
        }

        return targetGrid != null;
    }

    public void Equip() {
        Ref.Player.PlayerInput.InputPrimaryDown += Slice;
        Ref.Player.PlayerInput.InputPoint += SlicePreview;
    }
    public void Unequip() {
        Ref.Player.PlayerInput.InputPrimaryDown -= Slice;
        Ref.Player.PlayerInput.InputPoint -= SlicePreview;
    }
}