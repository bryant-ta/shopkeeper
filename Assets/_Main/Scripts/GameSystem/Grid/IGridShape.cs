using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface IGridShape {
    public string Name { get; } // auto-implemented by Unity
    public Vector3Int RootCoord { get; set; }
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }
    public Collider Collider { get; }

    public ShapeData ShapeData { get; set; }
    
    public void RotateShape(Vector3Int pivot, bool clockwise) {
        int cw = clockwise ? 1 : -1;
        
        // Rotate gameObject around y-axis
        ShapeData = GetShapeDataRotated(pivot, clockwise, out Vector3Int rotatedRootCoord);
        RootCoord = rotatedRootCoord;
        ShapeTransform.Rotate(Vector3.up, 90f * cw);
    }

    public ShapeData GetShapeDataRotated(Vector3Int pivot, bool clockwise, out Vector3Int rotatedRootCoord) {
        int cw = clockwise ? 1 : -1;
        ShapeData rotatedShapeData = new ShapeData { ShapeOffsets = new List<Vector3Int>() };
        
        Vector3Int pivotOffset = pivot - RootCoord;
        
        // Output new root coord
        Vector3Int relativePosition = RootCoord - pivotOffset;
        Vector3Int rotatedRelativePosition = new Vector3Int(relativePosition.z * cw, relativePosition.y, -relativePosition.x * cw);
        Vector3Int rotatedPosition = rotatedRelativePosition + pivotOffset;
        rotatedRootCoord = rotatedPosition;
        
        // Rotate shape data
        foreach (Vector3Int offset in ShapeData.ShapeOffsets) {
            // formula for rotating around a pivot on y-axis
            relativePosition = offset - pivotOffset;
            rotatedRelativePosition = new Vector3Int(relativePosition.z * cw, relativePosition.y, -relativePosition.x * cw);
            rotatedPosition = rotatedRelativePosition + pivotOffset;
            
            rotatedShapeData.ShapeOffsets.Add(rotatedPosition);
        }
        
        return rotatedShapeData;
    }
    
    public void DestroyShape() {
        ColliderTransform.DOScale(Vector3.zero, TweenManager.DestroyShapeDur).OnComplete(() => {
            ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
            Object.Destroy(ShapeTransform.gameObject);
        });
    }
}