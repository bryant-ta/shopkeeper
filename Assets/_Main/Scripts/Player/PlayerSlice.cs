using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSlice : MonoBehaviour, IPlayerTool {
    [SerializeField] float previewScale = 0.3f;
    [SerializeField] GameObject previewObj; // a plane

    MeshFilter previewPlaneMeshFilter;
    List<LineRenderer> previewLineRenderers;

    Grid targetGrid;
    CameraController camCtrl;

    void Awake() {
        previewPlaneMeshFilter = previewObj.GetComponent<MeshFilter>();
        if (previewPlaneMeshFilter == null || previewPlaneMeshFilter.mesh == null) {
            Debug.LogError("MeshFilter or mesh is missing on slice preview plane.");
            return;
        }

        previewLineRenderers = previewObj.GetComponentsInChildren<LineRenderer>().ToList();

        camCtrl = Camera.main.GetComponent<CameraController>();
    }

    // separated slice execution vars for readability
    bool execZSlice;
    float xzThreshold_ShapeLocal; // local to shape offsset
    Vector3Int rightCellCoord_ShapeLocal;
    IGridShape origShape;
    void Slice(ClickInputArgs clickInputArgs) {
        if (origShape == null) return;

        // Split targetShapeData into two shapes according to slicing selection
        List<Vector3Int> unvisitedOffsets = origShape.ShapeData.ShapeOffsets;
        List<Vector3Int> offsetsB = new();
        Queue<Vector3Int> searchQueue = new();

        Vector3Int rightCellOffset = rightCellCoord_ShapeLocal - origShape.ShapeData.RootCoord; // now working in shape offset space
        searchQueue.Enqueue(rightCellOffset);
        // Find right group offsets to create one shape data. Remaining offsets make up the other shape.
        while (searchQueue.Count > 0) {
            Vector3Int coord = searchQueue.Dequeue();
            unvisitedOffsets.Remove(coord);
            offsetsB.Add(coord);

            for (int i = 0; i < 4; i++) {
                if (origShape.ShapeData.NeighborExists(coord, (Direction) i)) {
                    Vector3Int c = coord + DirectionData.DirectionVectorsInt[i];
                    if (((execZSlice && c.x > xzThreshold_ShapeLocal) || (!execZSlice && c.z > xzThreshold_ShapeLocal)) &&
                        unvisitedOffsets.Contains(c)) {
                        searchQueue.Enqueue(c);
                    }
                }
            }
        }

        // Remove original shape from grid
        targetGrid.RemoveShapeCells(origShape, false);

        // Create two new shapes from slicing, replacing only shapeData from original shape
        MakeSlicedShape(unvisitedOffsets);
        MakeSlicedShape(offsetsB);

        // Destroy original shape
        origShape.DestroyShape();
        origShape = null;
    }

    void MakeSlicedShape(List<Vector3Int> offsets) {
        ShapeData shapeData = new ShapeData {RootCoord = origShape.ShapeData.RootCoord, ShapeOffsets = offsets};
        shapeData.RecenterOffsets();
        shapeData.ID = ShapeData.DetermineID(shapeData.ShapeOffsets);

        Product origProduct = Util.GetProductFromShape(origShape);
        if (origProduct == null) return;

        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            origProduct.ID.Color, origProduct.ID.Pattern, origProduct.ShapeData
        );
        productData.ShapeData = shapeData;
        Product product = ProductFactory.Instance.CreateProduct(productData, targetGrid.transform.TransformPoint(shapeData.RootCoord));

        targetGrid.PlaceShapeNoValidate(shapeData.RootCoord, product);
        Ledger.AddStockedProduct(product);
    }

    // works in target grid local space, positions preview in world space!
    Vector3 lastSelectedShapeCellCoord;
    bool lastIsZSlice;
    void SlicePreview(ClickInputArgs clickInputArgs) {
        if (!SelectTargetGrid(clickInputArgs)) {
            previewObj.SetActive(false);
            return;
        }

        // Formula for selecting cell adjacent to clicked face anti-normal (when pivot is bottom center) (y ignored) (relative to local grid transform)
        Vector3 localHitPoint = targetGrid.transform.InverseTransformPoint(clickInputArgs.HitPoint);
        Vector3 localHitAntiNormal =
            targetGrid.transform.InverseTransformDirection(Vector3.ClampMagnitude(-clickInputArgs.HitNormal, 0.1f));
        Vector3Int selectedShapeCellCoord = Vector3Int.FloorToInt(localHitPoint + localHitAntiNormal + new Vector3(0.5f, 0, 0.5f));

        IGridShape selectedShape = targetGrid.SelectPosition(selectedShapeCellCoord);
        if (selectedShape == null) {
            previewObj.SetActive(false);
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
            previewObj.SetActive(false);
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

        // Save values used for executing slice
        execZSlice = isZSlice;
        xzThreshold_ShapeLocal = isZSlice ?
            sliceFirstPos.x - targetGrid.transform.TransformPoint(shapeData.RootCoord).x :
            sliceFirstPos.z - targetGrid.transform.TransformPoint(shapeData.RootCoord).z;
        rightCellCoord_ShapeLocal = Vector3Int.RoundToInt(targetGrid.transform.TransformPoint(rightCellCoord));
        origShape = selectedShape;

        // Walk slice direction for cell pairs
        float x = sliceFirstPos.x;
        float z = sliceFirstPos.z;
        List<float> p = new() {isZSlice ? z : x};
        Vector3Int sliceDirVector = DirectionData.DirectionVectorsInt[(int) sliceDir];
        while (shapeData.NeighborExists(leftCellCoord, sliceDir) && shapeData.NeighborExists(rightCellCoord, sliceDir)) {
            z++;
            x++;
            p.Add(isZSlice ? z : x);
            leftCellCoord += sliceDirVector;
            rightCellCoord += sliceDirVector;
        }

        // Place slice preview plane
        float previewPosXorZ = p.Average();
        previewObj.transform.position = isZSlice ?
            targetGrid.transform.TransformPoint(new Vector3(sliceFirstPos.x, sliceFirstPos.y, previewPosXorZ)) :
            targetGrid.transform.TransformPoint(new Vector3(previewPosXorZ, sliceFirstPos.y, sliceFirstPos.z));
        previewObj.transform.rotation = isZSlice ?
            Quaternion.LookRotation(-camCtrl.IsometricRight, Vector3.up) :
            Quaternion.LookRotation(-camCtrl.IsometricForward, Vector3.up);
        previewObj.transform.localScale = new Vector3(p.Count + previewScale, 1f + previewScale, p.Count + previewScale);

        // Draw slice preview line around slice edge
        // Convert vertices of quad to world positions
        Vector3[] localVertices = previewPlaneMeshFilter.mesh.vertices;
        Vector3[] worldVertices = new Vector3[localVertices.Length];
        for (int j = 0; j < localVertices.Length; j++) {
            worldVertices[j] = transform.TransformPoint(localVertices[j]);
        }

        // Align line renderes along quad perimeter
        previewLineRenderers[0].SetPosition(0, worldVertices[0]);
        previewLineRenderers[0].SetPosition(1, worldVertices[1]);
        previewLineRenderers[1].SetPosition(0, worldVertices[1]);
        previewLineRenderers[1].SetPosition(1, worldVertices[3]);
        previewLineRenderers[2].SetPosition(0, worldVertices[3]);
        previewLineRenderers[2].SetPosition(1, worldVertices[2]);
        previewLineRenderers[3].SetPosition(0, worldVertices[2]);
        previewLineRenderers[3].SetPosition(1, worldVertices[0]);
        previewObj.SetActive(true);
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
        previewObj.SetActive(false);
    }
}