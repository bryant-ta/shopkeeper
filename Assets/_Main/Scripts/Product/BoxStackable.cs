using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxStackable : MonoBehaviour, IStackable {
    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
    }

    // CalculatePosition returns correct obj position when placed on stack at index
    public Vector3 CalculateStackPosition(int index) {
        float yOffset = boxCol.bounds.extents.y * 2;
        return new Vector3(0, index * yOffset, 0);
    }

    public Transform GetTransform() { return transform; }
}
