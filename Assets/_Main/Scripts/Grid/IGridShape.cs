using DG.Tweening;
using UnityEngine;

public interface IGridShape {
    public string Name { get; } // auto-implemented by Unity
    public Vector3Int RootCoord { get; set; }
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }
    public Collider Collider { get; }

    public ShapeData ShapeData { get; }
    
    public void DestroyShape() {
        ColliderTransform.DOScale(Vector3.zero, Constants.AnimDestroyShapeDur).OnComplete(() => {
            ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
            Object.Destroy(ShapeTransform.gameObject);
        });
    }
}