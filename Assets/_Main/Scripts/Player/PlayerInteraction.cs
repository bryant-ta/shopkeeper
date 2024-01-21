using EventManager;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    [SerializeField] float interactionRange;
    
    void Start() {
        Events.Sub<ClickInputArgs>(gameObject, EventID.PrimaryDown, Interact);
    }

    void Interact(ClickInputArgs clickInputArgs) {
        if (!IsInRange(clickInputArgs.TargetObj.transform.position)) return;

        print(clickInputArgs.TargetObj.name);
    }
    
    bool IsInRange(Vector3 targetPos) { return (targetPos - transform.position).magnitude < interactionRange; }
}
