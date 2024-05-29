using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSlice : MonoBehaviour, IPlayerTool {
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] GameObject slicePreviewObj; // a plane

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

        IGridShape selectedShape = targetGrid.SelectPosition(selectedCellCoord);
        if (selectedShape == null) return;
        
        Vector3 cellToHitPoint = localHitPoint - selectedCellCoord;
        
        // hit point must be on x parallel face, otherwise is z parallel
        bool isXFace = Math.Abs(cellToHitPoint.z) > Math.Abs(cellToHitPoint.x) ? true : false;
        
        // midpoint between the two cell centers for initial slice
        Vector3 sliceFirstPos = isXFace ?
            selectedCellCoord + new Vector3(0.5f * Math.Sign(cellToHitPoint.x), 0.5f, 0) :
            selectedCellCoord + new Vector3(0, 0.5f, 0.5f * Math.Sign(cellToHitPoint.x));
        
        
        // find cell pairs to slice
        Vector3Int leftCellCoord = Vector3Int.RoundToInt(sliceFirstPos + new Vector3(-0.1f, 0, 0));
        Vector3Int rightCellCoord = Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0.1f, 0, 0));
        
        float x = sliceFirstPos.x;
        float z = sliceFirstPos.z; // change to int
        Direction sliceDir = DirectionData.GetClosestDirection(localHitAntiNormal);
        List<float> p = new() {sliceFirstPos.z};
        int iterations = 10;
        int i = 0;
        ShapeData shapeData = selectedShape.ShapeData;
        while (shapeData.NeighborExists(leftCellCoord, sliceDir) && shapeData.NeighborExists(rightCellCoord, sliceDir)) {
            i++;
            
            p.Add(z++);
            leftCellCoord += DirectionData.DirectionVectorsInt[(int)sliceDir];
            rightCellCoord += DirectionData.DirectionVectorsInt[(int)sliceDir];

            if (i > iterations) {
                Debug.LogError("something went wrong");
                break;
            }
        }

        // size and position slice preview plane
        float slicePreviewPosZ = p.Average();
        slicePreviewObj.transform.position = new Vector3(sliceFirstPos.x, sliceFirstPos.y, slicePreviewPosZ);
        slicePreviewObj.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        slicePreviewObj.transform.localScale = new Vector3(p.Count + 0.2f, 1.2f, 1);
        
        
        // draw slice preview line around slice edge
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