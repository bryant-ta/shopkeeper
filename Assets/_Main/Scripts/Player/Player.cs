using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] float interactionRange;
    public float InteractionRange => interactionRange;

    public Transform dropPos;

    [SerializeField] Grid holdGrid;

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.SecondaryDown, PickUp);
        Events.Sub(gameObject, EventID.Drop, DropOne);

        GetComponent<Rigidbody>().centerOfMass = transform.position; // required for correct rotation when holding box
        GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity; // required for not rotating on locked axes on collisions
    }

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
            List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
            if (heldShapes.Count == 0) {
                Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
                return;
            }

            Vector3Int nextOpenHoldStackCoord = Vector3Int.zero;
            if (holdGrid.SelectLowestOpen(0, 0, out int lowestOpenY)) {
                nextOpenHoldStackCoord.y = lowestOpenY;

                if (!targetGrid.MoveShapes(holdGrid, nextOpenHoldStackCoord, heldShapes)) {
                    Debug.LogFormat("Not enough space in target grid ({0}) to move shapes.", targetGrid.gameObject.name); // TEMP
                }
            }
        }
    }

    void DropOne() {
        if (holdGrid.GridIsEmpty()) return;
        
        // TODO: implement
    }
    
    public bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < InteractionRange; }
}
