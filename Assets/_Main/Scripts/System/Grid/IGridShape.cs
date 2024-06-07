using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface IGridShape {
    public string Name { get; } // auto-implemented by Unity
    public Grid Grid { get; }

    public Transform ShapeTransform { get; }
    public Transform ColliderTransform { get; }
    public List<Collider> Colliders { get; }

    public ShapeData ShapeData { get; }

    public ShapeTags ShapeTags { get; }

    public void SetOutline(Color color, float weight);
    public void ResetOutline();

    public void DestroyShape(bool doAnim = true) {
        if (doAnim) {
            ColliderTransform.DOScale(Vector3.zero, TweenManager.DestroyShapeDur).OnComplete(
                () => {
                    ShapeTransform.DOKill();
                    ColliderTransform.DOKill(); // Note: may need to use manual tween ID when tweening other things on this object
                    Object.Destroy(ShapeTransform.gameObject);
                }
            );
        } else {
            ShapeTransform.DOKill();
            ColliderTransform.DOKill();
            Object.Destroy(ShapeTransform.gameObject);
        }
    }
}