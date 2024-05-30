using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSlice : MonoBehaviour, IPlayerTool {
    [SerializeField] float slicePreviewScale = 0.3f;
    [SerializeField] GameObject slicePreviewObj; // a plane

    MeshFilter slicePreviewPlaneMeshFilter;
    List<LineRenderer> slicePreviewLineRenderers;

    Grid targetGrid;
    CameraController camCtrl;

    void Awake() {
        slicePreviewPlaneMeshFilter = slicePreviewObj.GetComponent<MeshFilter>();
        if (slicePreviewPlaneMeshFilter == null || slicePreviewPlaneMeshFilter.mesh == null) {
            Debug.LogError("MeshFilter or mesh is missing on slice preview plane.");
            return;
        }

        slicePreviewLineRenderers = slicePreviewObj.GetComponentsInChildren<LineRenderer>().ToList();

        camCtrl = Camera.main.GetComponent<CameraController>();
    }

    void Slice(ClickInputArgs clickInputArgs) { }

    Vector3 lastSelectedShapeCellCoord;
    bool lastIsZSlice;
    void SlicePreview(ClickInputArgs clickInputArgs) {
        if (!SelectTargetGrid(clickInputArgs)) {
            slicePreviewObj.SetActive(false);
            return;
        }

        // Formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedShapeCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        IGridShape selectedShape = targetGrid.SelectPosition(selectedShapeCellCoord);
        if (selectedShape == null) {
            slicePreviewObj.SetActive(false);
            return;
        }

        // Determine if the hit point is on an X parallel face, otherwise it is on a Z parallel face
        Vector3 cellToHitPoint = localHitPoint - selectedShapeCellCoord;
        bool isZSlice = Math.Abs(cellToHitPoint.z) > Math.Abs(cellToHitPoint.x);
        if (localHitAntiNormal.y < 0) isZSlice = !isZSlice;

        // Cutoff for not repeating on same slice position
        if (selectedShapeCellCoord == lastSelectedShapeCellCoord && isZSlice == lastIsZSlice) {
            return;
        }
        lastSelectedShapeCellCoord = selectedShapeCellCoord;
        lastIsZSlice = isZSlice;

        // Midpoint between the two cell centers for initial slice (localPosition)
        Vector3 sliceFirstPos = isZSlice ?
            selectedShapeCellCoord + new Vector3(0.5f * Math.Sign(cellToHitPoint.x), 0.5f, 0) :
            selectedShapeCellCoord + new Vector3(0, 0.5f, 0.5f * Math.Sign(cellToHitPoint.z));

        // Find cell pairs to slice past initial slice
        Vector3Int leftCellCoord = isZSlice ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(-0.1f, 0, 0)) :
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, 0, -0.1f));
        Vector3Int rightCellCoord = isZSlice ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0.1f, 0, 0)) :
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, 0, 0.1f));

        if (targetGrid.SelectPosition(leftCellCoord) != targetGrid.SelectPosition(rightCellCoord)) { // Don't display on shape edges
            slicePreviewObj.SetActive(false);
            return;
        }

        ShapeData shapeData = selectedShape.ShapeData;
        Direction sliceDir = isZSlice ?
            DirectionData.GetClosestDirection(camCtrl.IsometricForward) :
            DirectionData.GetClosestDirection(camCtrl.IsometricRight);
        if (localHitAntiNormal.y < 0) {
            // Walk backwards along slice dir from selected cell for correct first slice cell coord
            Direction antiSliceDir = DirectionData.OppositeDirection(sliceDir);
            while (shapeData.NeighborExists(leftCellCoord, antiSliceDir) && shapeData.NeighborExists(rightCellCoord, antiSliceDir)) {
                leftCellCoord += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
                rightCellCoord += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
                sliceFirstPos += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
            }
        }
        
        // print($"{sliceFirstPos} | {leftCellCoord} | {rightCellCoord}");

        // Walk slice direction for cell pairs
        float x = sliceFirstPos.x;
        float z = sliceFirstPos.z;
        List<float> p = new() {isZSlice ? z : x};
        Vector3Int sliceDirVector = DirectionData.DirectionVectorsInt[(int) sliceDir];
        while (shapeData.NeighborExists(leftCellCoord, sliceDir) && shapeData.NeighborExists(rightCellCoord, sliceDir)) {
            z++; x++;
            p.Add(isZSlice ? z : x);
            leftCellCoord += sliceDirVector;
            rightCellCoord += sliceDirVector;
        }

        string a = "";
        foreach (var pp in p) {
            a += pp + " ";
        }
        print(a);

        // Place slice preview plane
        float slicePreviewPos = p.Average();
        slicePreviewObj.transform.position = isZSlice ?
            targetGrid.transform.TransformPoint(new Vector3(sliceFirstPos.x, sliceFirstPos.y, slicePreviewPos)) :
            targetGrid.transform.TransformPoint(new Vector3(slicePreviewPos, sliceFirstPos.y, sliceFirstPos.z));
        slicePreviewObj.transform.rotation = isZSlice ?
            Quaternion.LookRotation(-camCtrl.IsometricRight, Vector3.up) :
            Quaternion.LookRotation(-camCtrl.IsometricForward, Vector3.up);
        slicePreviewObj.transform.localScale = new Vector3(
            p.Count + slicePreviewScale, 1f + slicePreviewScale, p.Count + slicePreviewScale
        );

        // Draw slice preview line around slice edge
        // Convert vertices of quad to world positions
        Vector3[] localVertices = slicePreviewPlaneMeshFilter.mesh.vertices;
        Vector3[] worldVertices = new Vector3[localVertices.Length];
        for (int j = 0; j < localVertices.Length; j++) {
            worldVertices[j] = transform.TransformPoint(localVertices[j]);
        }

        // Align line renderes along quad perimeter
        slicePreviewLineRenderers[0].SetPosition(0, worldVertices[0]);
        slicePreviewLineRenderers[0].SetPosition(1, worldVertices[1]);
        slicePreviewLineRenderers[1].SetPosition(0, worldVertices[1]);
        slicePreviewLineRenderers[1].SetPosition(1, worldVertices[3]);
        slicePreviewLineRenderers[2].SetPosition(0, worldVertices[3]);
        slicePreviewLineRenderers[2].SetPosition(1, worldVertices[2]);
        slicePreviewLineRenderers[3].SetPosition(0, worldVertices[2]);
        slicePreviewLineRenderers[3].SetPosition(1, worldVertices[0]);
        slicePreviewObj.SetActive(true);
    }

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
        slicePreviewObj.SetActive(false);
    }
}