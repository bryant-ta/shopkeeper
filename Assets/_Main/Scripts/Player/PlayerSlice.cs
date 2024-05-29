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

    void Awake() {
        slicePreviewPlaneMeshFilter = slicePreviewObj.GetComponent<MeshFilter>();
        if (slicePreviewPlaneMeshFilter == null || slicePreviewPlaneMeshFilter.mesh == null) {
            Debug.LogError("MeshFilter or mesh is missing on slice preview plane.");
            return;
        }

        slicePreviewLineRenderers = slicePreviewObj.GetComponentsInChildren<LineRenderer>().ToList();
    }

    void Slice(ClickInputArgs clickInputArgs) { }

    Vector3 lastSliceFirstPos;
    void SlicePreview(ClickInputArgs clickInputArgs) {
        if (!SelectTargetGrid(clickInputArgs)) {
            return;
        }

        // Formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        IGridShape selectedShape = targetGrid.SelectPosition(selectedCellCoord);
        if (selectedShape == null) return;

        // Determine if the hit point is on an X parallel face, otherwise it is on a Z parallel face
        Vector3 cellToHitPoint = localHitPoint - selectedCellCoord;
        bool isXFace = Math.Abs(cellToHitPoint.z) > Math.Abs(cellToHitPoint.x);

        // Midpoint between the two cell centers for initial slice
        Vector3 sliceFirstPos = isXFace ?
            selectedCellCoord + new Vector3(0.5f * Math.Sign(cellToHitPoint.x), 0.5f, 0) :
            selectedCellCoord + new Vector3(0, 0.5f, 0.5f * Math.Sign(cellToHitPoint.z));

        if ((sliceFirstPos - lastSliceFirstPos).sqrMagnitude < 0.001f) return;
        lastSliceFirstPos = sliceFirstPos;

        // Find cell pairs to slice past initial slice
        Vector3Int leftCellCoord = isXFace ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(-0.1f, 0, 0)) :
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, 0, -0.1f));
        Vector3Int rightCellCoord = isXFace ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0.1f, 0, 0)) :
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, 0, 0.1f));

        Direction sliceDir = DirectionData.GetClosestDirection(localHitAntiNormal);
        Vector3Int sliceDirVector = DirectionData.DirectionVectorsInt[(int) sliceDir];
        float x = sliceFirstPos.x;
        float z = sliceFirstPos.z;
        List<float> p = new() {isXFace ? sliceFirstPos.z : sliceFirstPos.x};
        ShapeData shapeData = selectedShape.ShapeData;
        int iterations = 10;
        int i = 0;
        while (shapeData.NeighborExists(leftCellCoord, sliceDir) && shapeData.NeighborExists(rightCellCoord, sliceDir)) {
            i++;

            p.Add(isXFace ? z++ : x++);
            leftCellCoord += sliceDirVector;
            rightCellCoord += sliceDirVector;

            if (i > iterations) {
                Debug.LogError("something went wrong");
                break;
            }
        }

        // Place slice preview plane
        float slicePreviewPos = p.Average();
        slicePreviewObj.transform.position = isXFace ?
            new Vector3(sliceFirstPos.x, sliceFirstPos.y, slicePreviewPos) + targetGrid.transform.position :
            new Vector3(slicePreviewPos, sliceFirstPos.y, sliceFirstPos.z) + targetGrid.transform.position;
        slicePreviewObj.transform.rotation =
            isXFace ?
                Quaternion.LookRotation(Quaternion.Euler(0, 90, 0) * localHitAntiNormal, Vector3.up) :
                Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * localHitAntiNormal, Vector3.up);
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