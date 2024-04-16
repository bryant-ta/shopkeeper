using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class TweenManager {
    public const float PlaceShapeDur = 0.2f;                       // duration of shape placement to grids
    public const float DestroyShapeDur = 0.2f;                     // duration of shape destroy
    public const float IndividualDeliveryDelay = 0.1f;             // delay between delivery of individual products
    public const float OrderBubbleFadeDur = 0.75f;                 // duration of order bubble fade in/out
    public const float DragMoveDur = 0.15f;                        // duration of moving dragged shapes on grid
    public const float DragRotateDur = 0.13f;                      // duration of rotating dragged shapes
    public static readonly DOTweenShakeArgs InvalidShake = new() { // shake params for invalid product placement
        Duration = 0.2f,
        Strength = 0.1f,
        Vibrato = 30,
        Randomness = 20f
    };

    public const string PlaceShapeID = "_placeShape";
    public const string DragMoveID = "_dragMove";
    public const string DragRotateID = "_dragRotate";
    const string InvalidShakeID = "_invalidShake";
    
    public static void Shake(IGridShape shape) {
        string tweenID = shape.ShapeTransform.GetInstanceID() + InvalidShakeID;
        
        DOTween.Kill(tweenID); // Note: more efficient to replace with hash?
        shape.ShapeTransform.localPosition = shape.RootCoord;
        
        shape.ShapeTransform.DOShakePosition(
            InvalidShake.Duration,
            new Vector3(1, 0, 1) * InvalidShake.Strength,
            InvalidShake.Vibrato,
            InvalidShake.Randomness
        ).SetId(tweenID);
    }
    public static void Shake(List<IGridShape> heldShapes) {
        for (int i = 0; i < heldShapes.Count; i++) {
            Shake(heldShapes[i]);
        }
    }
}