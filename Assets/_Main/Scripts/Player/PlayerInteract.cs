using System.Collections.Generic;
using EventManager;
using UnityEngine;

public class PlayerInteract : MonoBehaviour {
    [SerializeField] float interactionRange;
    public float InteractionRange => interactionRange;
    [SerializeField] float interactionHeight;

    [SerializeField] Grid holdGrid;

    public Transform dropPos;

    void Awake() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.SecondaryDown, PickUp);
    }

    void Start() {
        GetComponent<Rigidbody>().centerOfMass = transform.position;           // required for correct rotation when holding box
        GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity; // required for not rotating on locked axes on collisions
    }

    void Interact() {
        // find closest interactable
        
        // trigger interact
    }

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

    public bool IsInRange(Vector3 targetPos) {
        Vector3 xzDif = targetPos - transform.position;
        xzDif.y = 0;
        return targetPos.y - transform.position.y < interactionHeight && xzDif.magnitude < interactionRange;
    }

    #region Upgrades

    public void ModifyMaxHoldHeight(int delta) { holdGrid.SetMaxHeight(holdGrid.Height + delta); }

    #endregion
}
