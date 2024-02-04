using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxStackable : MonoBehaviour, IStackable, IGridShape {
    public Vector3Int RootCoord => Vector3Int.RoundToInt(ShapeTransform.position);
    public Transform ShapeTransform => shapeTransform;
    public Transform shapeTransform;
    public Transform ColliderTransform => transform;

    public ShapeData ShapeData => shapeData;
    ShapeData shapeData;

    public ShapeType ShapeType => shapeType;
    [SerializeField] ShapeType shapeType;

    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
        shapeTransform = transform.parent;
        shapeData = ShapeDataLookUp.LookUp[shapeType];
    }

    public Vector3 CalculateStackPosition(float stackHeight) {
        float yOffset = boxCol.bounds.extents.y;
        return new Vector3(0, stackHeight + yOffset, 0);
    }

    public Transform GetTransform() { return transform; }
    public Stack GetStack() {
        if (transform.parent.TryGetComponent(out Stack stack)) {
            return stack;
        } else {
            Debug.LogError("IStackable is missing Stack in parent object");
            return null;
        }
    }
    public Collider GetCollider() { return boxCol; }
}