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
    public List<Collider> Colliders { get; }

    public ShapeDataID ShapeDataID { get; } // TODO: consider removing
    public ShapeData ShapeData { get; set; }
    
    public ShapeTags ShapeTags { get; }
    
    // Rotates shape data to match a CW/CCW rotation. No physical gameobject rotation
    // TODO: consider moving RootCoord to ShapeData too
    public void RotateShape(bool clockwise) {
        int cw = clockwise ? 1 : -1;
        
        // Rotate root coord
        RootCoord = new Vector3Int(RootCoord.z * cw, RootCoord.y, -RootCoord.x * cw);
        
        ShapeData.RotateShape(clockwise);
    }

    public void DestroyShape() {
        ColliderTransform.DOScale(Vector3.zero, TweenManager.DestroyShapeDur).OnComplete(() => {
            ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
            Object.Destroy(ShapeTransform.gameObject);
        });
    }
}