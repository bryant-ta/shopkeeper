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

    [field:SerializeField] public Grid HoldGrid { get; private set; }

    void Awake() {
        Ref.Player.PlayerInput.InputInteract += Interact;
        Ref.Player.PlayerInput.InputSecondaryDown += PickUp;
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

        if (!HoldGrid.IsEmpty() || !Ref.Player.PlayerDrag.DragGrid.IsEmpty()) {
            TweenManager.Shake(HoldGrid.AllShapes());
            TweenManager.Shake(Ref.Player.PlayerDrag.DragGrid.AllShapes());
            return;
        }
        
        IInteractable closestInteractable = FindClosestInteractable();
        if (closestInteractable == null) return;

        if (!closestInteractable.Interact(gameObject)) {
            return;
        }
        
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
            if (targetGrid == HoldGrid) return;

            List<IGridShape> heldShapes = targetGrid.SelectStackedShapes(clickedShape.RootCoord);
            if (heldShapes.Count == 0) {
                Debug.LogError("Clicked shape not registered in targetGrid. (Did you forget to initialize it with its grid?)");
                return;
            }

            Vector3Int nextOpenHoldStackCoord = Vector3Int.zero;
            if (HoldGrid.SelectLowestOpen(0, 0, out int lowestOpenY)) {
                nextOpenHoldStackCoord.y = lowestOpenY;

                if (!targetGrid.MoveShapes(HoldGrid, nextOpenHoldStackCoord, heldShapes)) {
                    TweenManager.Shake(heldShapes);
                }
                
                SoundManager.Instance.PlaySound(SoundID.ProductMove);
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

    public void ModifyMaxHoldHeight(int delta) { HoldGrid.SetMaxHeight(HoldGrid.Height + delta); }

    #endregion
}