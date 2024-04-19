using System.Collections.Generic;
using DG.Tweening;
using Tags;
using UnityEngine;

public interface IGridShape {
    public string Name { get; } // auto-implemented by Unity
    public Vector3Int RootCoord { get; set; }
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }
    public Collider Collider { get; }

    public ShapeDataID ShapeDataID { get; }
    public ShapeData ShapeData { get; set; }
    
    public ShapeTags ShapeTags { get; }
    
    // Rotates shape data to match a CW/CCW rotation. No physical gameobject rotation
    public void RotateShape(bool clockwise) {
        int cw = clockwise ? 1 : -1;
        
        // Rotate root coord
        RootCoord = new Vector3Int(RootCoord.z * cw, RootCoord.y, -RootCoord.x * cw);
        
        // Rotate shape data
        ShapeData rotatedShapeData = new ShapeData { ShapeOffsets = new List<Vector3Int>() };
        foreach (Vector3Int offset in ShapeData.ShapeOffsets) {
            Vector3Int rotatedOffset = new Vector3Int(offset.z * cw, offset.y, -offset.x * cw);
            rotatedShapeData.ShapeOffsets.Add(rotatedOffset);
        }

        ShapeData = rotatedShapeData;
    }
    
    public void DestroyShape() {
        ColliderTransform.DOScale(Vector3.zero, TweenManager.DestroyShapeDur).OnComplete(() => {
            ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
            Object.Destroy(ShapeTransform.gameObject);
        });
    }
}