using UnityEngine;

public class Deliverer : MonoBehaviour {
    [SerializeField] Zone deliveryZone;
    public Grid Grid => grid;
    Grid grid;

    void Awake() { grid = GetComponentInParent<Grid>(); }

    void Start() {
        // Create delivery zone
        deliveryZone.Setup(Vector3Int.RoundToInt(transform.localPosition));
        grid.AddZone(deliveryZone);
    }
}