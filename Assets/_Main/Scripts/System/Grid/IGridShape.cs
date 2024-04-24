using System.Collections.Generic;
using DG.Tweening;
using Tags;
using UnityEngine;

public interface IGridShape {
    public string Name { get; } // auto-implemented by Unity
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }
    public List<Collider> Colliders { get; }

    public ShapeData ShapeData { get; set; }
    
    public ShapeTags ShapeTags { get; }

    public void DestroyShape() {
        ColliderTransform.DOScale(Vector3.zero, TweenManager.DestroyShapeDur).OnComplete(() => {
            ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
            Object.Destroy(ShapeTransform.gameObject);
        });
    }
}