using UnityEngine;

public class Deliverer : MonoBehaviour {
    [field: SerializeField] public Grid Grid { get; private set; }
    [SerializeField] Zone deliveryZone;
    [SerializeField] Vector3 deliveryZoneRootCoord;

    void Start() {
        deliveryZone.Setup(Vector3Int.RoundToInt(deliveryZoneRootCoord));
        Grid.AddZone(deliveryZone);
    }

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}