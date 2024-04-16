using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxProduct : Product, IGridShape {
    [field: SerializeField, ReadOnly] public Vector3Int RootCoord { get; set; }

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

    [field:SerializeField] public ShapeData ShapeData { get; set; }

    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
        ShapeTransform = transform.parent;
        ShapeData = ShapeDataLookUp.LookUp[shapeType];
    }
}