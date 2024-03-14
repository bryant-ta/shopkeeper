using TriInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxProduct : Product, IGridShape {
    [field:SerializeField, ReadOnly] public Vector3Int RootCoord { get; set; }
    public Grid Grid {
        get {
            if (ShapeTransform.parent.TryGetComponent(out Grid grid)) {
                return grid;
            }
            
            Debug.LogError("IGridShape is not in a grid.");
            return null;
        }
    }

    public Transform ShapeTransform { get; private set; }
    public Transform ColliderTransform => transform;
    public Collider Collider => boxCol;

    [SerializeField] ShapeType shapeType;
    public ShapeType ShapeType => shapeType;

    public ShapeData ShapeData => shapeData;
    ShapeData shapeData;
    
    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
        ShapeTransform = transform.parent;
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