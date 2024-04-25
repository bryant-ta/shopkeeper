using UnityEngine;

public class Deliverer : MonoBehaviour {
    [SerializeField] GameObject delivererObj;
    [field:SerializeField] public Grid Grid { get; private set; }
    [SerializeField] Zone deliveryZone;

    void Start() {
        // Create delivery zone
        deliveryZone.Setup(Vector3Int.RoundToInt(transform.localPosition));
        Grid.AddZone(deliveryZone);
    }

    public void Enable() {
        delivererObj.SetActive(true);
        
    }
    public void Disable() {
        delivererObj.SetActive(false);
    }
}