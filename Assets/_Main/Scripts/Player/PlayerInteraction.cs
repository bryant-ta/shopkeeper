using EventManager;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    [SerializeField] float interactionRange;
    
    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Interact);
        Events.Sub<Vector3>(gameObject, EventID.Point, OnDrag);
    }

    void Interact(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;

        print(clickInputArgs.TargetObj.name);
    }

    void OnDrag(Vector3 hitPoint) {
        print("hit point: " + hitPoint);
    }
    
    bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < interactionRange; }
}
