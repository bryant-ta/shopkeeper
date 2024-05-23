using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;

public class PlayerInteract : MonoBehaviour {
    void Awake() {
        Ref.Player.PlayerInput.InputInteract += Interact;
        Ref.Player.PlayerInput.InputSecondaryDown += PickUp;
    }

    #region Interact

    // Action<GameObject> releaseAction = null;
    void Interact() {
        // // Release interactable if previously interacted with one that requires releasing
        // if (releaseAction != null) {
        //     releaseAction(gameObject);
        //     releaseAction = null;
        //     return;
        // }
        //
        // if (!HoldGrid.IsEmpty() || !Ref.Player.PlayerDrag.DragGrid.IsEmpty()) {
        //     TweenManager.Shake(HoldGrid.AllShapes());
        //     TweenManager.Shake(Ref.Player.PlayerDrag.DragGrid.AllShapes());
        //     SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
        //     return;
        // }
        //
        // if (closestInteractable == null) return;
        //
        // if (!closestInteractable.Interact(gameObject)) {
        //     return;
        // }
        //
        // if (closestInteractable.RequireRelease) {
        //     releaseAction = closestInteractable.Release;
        // }
    }
    
    #endregion

    #region PickUp

    void PickUp(ClickInputArgs clickInputArgs) {
        // GameObject targetObj = clickInputArgs.TargetObj;
        //
        // if (targetObj.TryGetComponent(out IGridShape clickedShape)) {
        //     Grid targetGrid = clickedShape.Grid;
        //     if (targetGrid == HoldGrid) return;
        //
        //     List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
        //     if (heldShapes.Count == 0) {
        //         Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
        //         return;
        //     }
        //
        //     Vector3Int nextOpenHoldStackCoord = Vector3Int.zero;
        //     if (HoldGrid.SelectLowestOpen(0, 0, out int lowestOpenY)) {
        //         nextOpenHoldStackCoord.y = lowestOpenY;
        //
        //         if (!targetGrid.MoveShapes(HoldGrid, nextOpenHoldStackCoord, heldShapes)) {
        //             TweenManager.Shake(heldShapes);
        //             SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
        //         }
        //         
        //         // play sound based on highest product in selected stack
        //         // assumes heldShapes is ordered from lowest to highest position
        //         holdGridPickUpAs.pitch = 0.7f + 0.05f * heldShapes[heldShapes.Count-1].RootCoord.y;
        //         holdGridPickUpAs.Play();
        //     } else { // no more space in hold grid
        //         TweenManager.Shake(heldShapes);
        //         SoundManager.Instance.PlaySound(SoundID.ProductInvalidShake);
        //     }
        // }
    }
    
    #endregion

    #region Upgrades

    public void ModifyMaxHoldHeight(int delta) {
        // HoldGrid.SetMaxHeight(HoldGrid.Height + delta);
    }

    #endregion
}