using System;
using System.Collections.Generic;
using DG.Tweening;
using EventManager;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] float interactionRange;
    public float InteractionRange => interactionRange;
    [SerializeField] float interactionHeight;

    [SerializeField] Grid holdGrid;
    
    public Transform dropPos;

    void Awake() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.SecondaryDown, PickUp);
        Events.Sub(gameObject, EventID.Drop, DropOne);
    }

    void Start() {
        GetComponent<Rigidbody>().centerOfMass = transform.position;           // required for correct rotation when holding box
        GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity; // required for not rotating on locked axes on collisions
    }

    Tweener invalidPickUpTween;
    void PickUp(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;
        GameObject targetObj = clickInputArgs.TargetObj;

        // TODO: figure out interact action: use click or 'E' ?
        if (targetObj.TryGetComponent(out IInteractable interactable)) {
            interactable.Interact();
            return;
        }

        if (targetObj.TryGetComponent(out IGridShape clickedShape)) {
            Grid targetGrid = clickedShape.Grid;
            if (targetGrid == holdGrid) return;

            List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
            if (heldShapes.Count == 0) {
                Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
                return;
            }

            Vector3Int nextOpenHoldStackCoord = Vector3Int.zero;
            if (holdGrid.SelectLowestOpen(0, 0, out int lowestOpenY)) {
                nextOpenHoldStackCoord.y = lowestOpenY;

                if (!targetGrid.MoveShapes(holdGrid, nextOpenHoldStackCoord, heldShapes)) {
                    PlayInvalidPickUpAnim(heldShapes);
                }
            } else { // no more space in hold grid
                PlayInvalidPickUpAnim(heldShapes);
            }
        }
    }

    void DropOne() {
        if (holdGrid.GridIsEmpty()) return;

        // TODO: implement
    }

    void PlayInvalidPickUpAnim(List<IGridShape> heldShapes) {
        for (int i = 0; i < heldShapes.Count; i++) {
            heldShapes[i].ShapeTransform.DOKill();
            heldShapes[i].ShapeTransform.position = heldShapes[i].RootCoord;
            heldShapes[i].ShapeTransform.DOShakePosition(
                Constants.AnimInvalidShake.Duration,
                new Vector3(1, 0, 1) * Constants.AnimInvalidShake.Strength,
                Constants.AnimInvalidShake.Vibrato,
                Constants.AnimInvalidShake.Randomness
            );
        }
    }

    public bool IsInRange(Vector3 targetPos) {
        Vector3 xzDif = targetPos - transform.position;
        xzDif.y = 0;
        return targetPos.y - transform.position.y < interactionHeight && xzDif.magnitude < interactionRange;
    }
}