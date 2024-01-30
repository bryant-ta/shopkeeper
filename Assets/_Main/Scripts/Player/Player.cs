using System;
using EventManager;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] float interactionRange;
    public float InteractionRange => interactionRange;

    public Transform dropPos;
    
    Stack heldStack;

    void Awake() {
        heldStack = GetComponentInChildren<Stack>();
    }

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

        if (targetObj.TryGetComponent(out IStackable s)) {
            Stack clickedStack = s.GetStack();
            clickedStack.PlaceRange(heldStack, clickedStack.IndexOf(s), clickedStack.Size() - 1);
        }
    }

    void DropOne() {
        if (heldStack.Size() == 0) return;
        
        Stack droppedStack = heldStack.Pop();
        droppedStack.transform.position = dropPos.position;
    }
    
    public bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < InteractionRange; }
}
