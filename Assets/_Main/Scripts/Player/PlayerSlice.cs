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
    float execXZThreshold; // in shape offset space
    Vector3Int execRightCellCoord;
    IGridShape origShape;
    Product origProduct;
    void Slice(ClickInputArgs clickInputArgs) {
        if (origShape == null) return;
        
        // convert local grid coord -> shape offset coord
        float xzThreshold = execZSlice ? execXZThreshold - origShape.ShapeData.RootCoord.x : execXZThreshold - origShape.ShapeData.RootCoord.z;
        Vector3Int rightCellOffset = execRightCellCoord - origShape.ShapeData.RootCoord;

        // Split targetShapeData into two shapes according to slicing selection
        List<Vector3Int> offsetsB = new();
        List<Vector3Int> unvisitedOffsets = origShape.ShapeData.ShapeOffsets;
        Queue<Vector3Int> searchQueue = new();
        offsetsB.Add(rightCellOffset);
        unvisitedOffsets.Remove(rightCellOffset);
        searchQueue.Enqueue(rightCellOffset);
        while (searchQueue.Count > 0) { // Find right group offsets to create one shape data. Remaining offsets make up the other shape.
            Vector3Int coord = searchQueue.Dequeue();

            for (int i = 0; i < 4; i++) {
                if (origShape.ShapeData.ContainsDir(coord, (Direction) i)) {
                    Vector3Int c = coord + DirectionData.DirectionVectorsInt[i];
                    if (((execZSlice && c.x > xzThreshold) || (!execZSlice && c.z > xzThreshold)) && unvisitedOffsets.Contains(c)) {
                        offsetsB.Add(c);
                        unvisitedOffsets.Remove(c);
                        searchQueue.Enqueue(c);
                    }
                }
            }
        }

        // Remove original shape from grid
        targetGrid.RemoveShapeCells(origShape, false);

        // Create two new shapes from slicing, replacing only shapeData from original shape
        origProduct = Util.GetProductFromShape(origShape);
        if (origProduct == null) {
            Debug.LogError("Unable to slice shape: could not get product from shape.");
            return;
        }
        IGridShape shapeA = MakeSlicedShape(unvisitedOffsets, origProduct);
        IGridShape shapeB = MakeSlicedShape(offsetsB, origProduct);
        if (shapeA == null || shapeB == null) {
            return;
        }
        targetGrid.TriggerFallOnTarget(shapeA);
        targetGrid.TriggerFallOnTarget(shapeB);

        // Destroy original shape
        origShape.DestroyShape();
        Ledger.RemoveStockedProduct(origProduct);
        origShape = null;
    }

    IGridShape MakeSlicedShape(List<Vector3Int> offsets, Product originalProduct) {
        ShapeData shapeData = new ShapeData {RootCoord = origShape.ShapeData.RootCoord, ShapeOffsets = offsets};
        shapeData.RecenterOffsets(); // modifies root coord too
        shapeData.ID = ShapeData.DetermineID(shapeData.ShapeOffsets);

        SO_Product productData = ProductFactory.Instance.CreateSOProduct(
            originalProduct.ID.Color, originalProduct.ID.Pattern, originalProduct.ShapeData
        );
        productData.ShapeData = shapeData;
        Product product = ProductFactory.Instance.CreateProduct(productData, targetGrid.transform.TransformPoint(shapeData.RootCoord));

        targetGrid.PlaceShapeNoValidate(shapeData.RootCoord, product);
        Ledger.AddStockedProduct(product);

        return product;
    }

    // works in target grid local space, positions preview in world space!
    Vector3 lastSelectedShapeCellCoord;
    bool lastIsZSlice;
    void SlicePreview(ClickInputArgs clickInputArgs) {
        targetGrid = Ref.Player.SelectTargetGrid(clickInputArgs);
        if (targetGrid == null) {
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

        // Midpoint between the two cell centers for initial slice (local pos)
        Vector3 sliceFirstPos = isZSlice ?
            selectedShapeCellCoord + new Vector3(0.5f * Math.Sign(cellToHitPoint.x), 0.5f, 0) :
            selectedShapeCellCoord + new Vector3(0, 0.5f, 0.5f * Math.Sign(cellToHitPoint.z));

        // Find cell pairs to slice past initial slice
        Vector3Int leftCellCoord = isZSlice ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(-0.1f, -0.1f, 0)) : // y = -0.1 to round down, it's relative to selectedShapeCoord
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, -0.1f, -0.1f));
        Vector3Int rightCellCoord = isZSlice ?
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0.1f, -0.1f, 0)) :
            Vector3Int.RoundToInt(sliceFirstPos + new Vector3(0, -0.1f, 0.1f));

        // Don't display preview on shape edges
        IGridShape leftShape = targetGrid.SelectPosition(leftCellCoord);
        IGridShape rightShape = targetGrid.SelectPosition(rightCellCoord);
        if (leftShape == null || rightShape == null || leftShape != rightShape) { 
            previewObj.SetActive(false);
            return;
        }

        ShapeData shapeData = selectedShape.ShapeData;
        Direction sliceDir = isZSlice ? Direction.North : Direction.East;
        if (localHitAntiNormal.y < 0) {
            // Walk backwards along slice dir from selected cell for correct first slice cell coord
            Direction antiSliceDir = DirectionData.OppositeDirection(sliceDir);
            while (shapeData.ContainsDir(leftCellCoord - shapeData.RootCoord, antiSliceDir) &&
                   shapeData.ContainsDir(rightCellCoord - shapeData.RootCoord, antiSliceDir)) {
                leftCellCoord += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
                rightCellCoord += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
                sliceFirstPos += DirectionData.DirectionVectorsInt[(int) antiSliceDir];
            }
        }

        // Save values used for executing slice
        execZSlice = isZSlice;
        execXZThreshold = isZSlice ? sliceFirstPos.x : sliceFirstPos.z;
        execRightCellCoord = rightCellCoord;
        origShape = selectedShape;

        // Walk slice direction for cell pairs
        float x = sliceFirstPos.x;
        float z = sliceFirstPos.z;
        List<float> p = new() {isZSlice ? z : x};
        Vector3Int sliceDirVector = DirectionData.DirectionVectorsInt[(int) sliceDir];
        while (shapeData.ContainsDir(leftCellCoord - shapeData.RootCoord, sliceDir) &&
               shapeData.ContainsDir(rightCellCoord - shapeData.RootCoord, sliceDir)) {
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
        bool camRotationState = camCtrl.IsometricForward == Vector3Int.forward || camCtrl.IsometricForward == Vector3Int.right;
        previewObj.transform.rotation = isZSlice ?
            Quaternion.LookRotation(camRotationState ? Vector3.left : Vector3.right, Vector3.up) :
            Quaternion.LookRotation(camRotationState ? Vector3.back : Vector3.forward, Vector3.up);
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