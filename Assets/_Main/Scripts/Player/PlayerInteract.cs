using System;
using System.Collections.Generic;
using EventManager;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInteract : MonoBehaviour {
    [FormerlySerializedAs("interactionRange")] [SerializeField]
    float interactRange;
    public float InteractRange => interactRange;
    [FormerlySerializedAs("interactionHeight")] [SerializeField]
    float interactHeight;

    [SerializeField] Grid holdGrid;

    void Awake() {
        Events.Sub(gameObject, EventID.Interact, Interact);
        Events.Sub<ClickInputArgs>(gameObject, EventID.SecondaryDown, PickUp); 
    }

    void Start() {
        GetComponent<Rigidbody>().centerOfMass = transform.position;           // required for correct rotation when holding box
        GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity; // required for not rotating on locked axes on collisions
    }

    #region Interact

    Action<GameObject> releaseAction = null;
    void Interact() {
        // Release interactable if previously interacted with one that requires releasing
        if (releaseAction != null) {
            releaseAction(gameObject);
            releaseAction = null;
            return;
        }
        
        IInteractable closestInteractable = FindClosestInteractable();
        if (closestInteractable == null) return;

        closestInteractable.Interact(gameObject);
        
        if (closestInteractable.RequireRelease) {
            releaseAction = closestInteractable.Release;
        }
    }

    Collider[] nearInteractables = new Collider[50];
    IInteractable FindClosestInteractable() {
        int nearInteractablesSize = Physics.OverlapSphereNonAlloc(transform.position, interactRange, nearInteractables);

        IInteractable closestInteractable = null;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < nearInteractablesSize; i++) {
            if (nearInteractables[i].TryGetComponent(out IInteractable interactable)) {
                float distance = Vector3.Distance(transform.position, nearInteractables[i].transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }
    
    #endregion

    #region PickUp

    void PickUp(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;
        GameObject targetObj = clickInputArgs.TargetObj;

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
                    TweenManager.Shake(heldShapes);
                }
            } else { // no more space in hold grid
                TweenManager.Shake(heldShapes);
            }
        }
    }
    
    #endregion

    public bool IsInRange(Vector3 targetPos) {
        Vector3 xzDif = targetPos - transform.position;
        xzDif.y = 0;
        return targetPos.y - transform.position.y < interactHeight && xzDif.magnitude < interactRange;
    }

    #region Upgrades

    public void ModifyMaxHoldHeight(int delta) { holdGrid.SetMaxHeight(holdGrid.Height + delta); }

    #endregion
}