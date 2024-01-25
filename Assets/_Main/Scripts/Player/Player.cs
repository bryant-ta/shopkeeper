using System;
using EventManager;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] float interactionRange;
    public float InteractionRange => interactionRange;

    Stack stack;

    void Awake() {
        stack = GetComponentInChildren<Stack>();
    }

    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.SecondaryDown, PickUp);
        Events.Sub(gameObject, EventID.Drop, Drop);
    }

    void PickUp(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;
        GameObject targetObj = clickInputArgs.TargetObj;

        if (targetObj.TryGetComponent(out IStackable s)) {
            stack.Place(s);
        }
    }

    void Drop() {
        Transform droppedStackTrans = stack.Pop();
        droppedStackTrans.position = transform.position + Vector3.forward * 2;
    }
    
    public bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < InteractionRange; }
}
