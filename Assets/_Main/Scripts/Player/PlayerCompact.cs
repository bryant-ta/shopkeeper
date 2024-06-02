using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCompact : MonoBehaviour, IPlayerTool {
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
    
    void Compact(ClickInputArgs clickInputArgs) {
        
    }

    // works in target grid local space, positions preview in world space!
    Vector3 lastSelectedShapeCellCoord;
    bool lastIsZSlice;
    void CompactPreview(ClickInputArgs clickInputArgs) {
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

        
    }
    
    public void Equip() {
        Ref.Player.PlayerInput.InputPrimaryDown += Compact;
        Ref.Player.PlayerInput.InputPoint += CompactPreview;
    }
    public void Unequip() {
        Ref.Player.PlayerInput.InputPrimaryDown -= Compact;
        Ref.Player.PlayerInput.InputPoint -= CompactPreview;
        previewObj.SetActive(false);
    }
}

