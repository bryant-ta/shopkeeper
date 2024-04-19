using System;
using System.Collections.Generic;
using Tags;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Product : MonoBehaviour, IGridShape {
    [SerializeField] SO_Product productData;

    #region Product
    
    [field: SerializeField, Title("Product"), ReadOnly] public ProductID ID { get; private set; }
    public string Name { get; private set; }
    
    [field: SerializeField, HideInEditMode] public ProductTags ProductTags { get; private set; }

    #endregion
    
    #region IGridShape
    
    [field: SerializeField, Space, ReadOnly] public Vector3Int RootCoord { get; set; }

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

    [SerializeField] ShapeDataID shapeDataID;
    public ShapeDataID ShapeDataID => shapeDataID;

    [field:SerializeField] public ShapeData ShapeData { get; set; }
    
    [field: SerializeField, HideInEditMode] public ShapeTags ShapeTags { get; private set; }

    BoxCollider boxCol;
    
    #endregion

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
        ShapeTransform = transform.parent;
        ShapeData = ShapeDataLookUp.LookUp[shapeDataID];
        
        if (productData == null) return;
        Init(productData);
    }

    public void Init(SO_Product _productData) {
        if (productData == null) productData = _productData;
        
        ID = productData.ID;
        Name = productData.ID.ToString();
        gameObject.name = Name;

        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", _productData.Texture);

        ProductTags = new ProductTags(productData.BasicTagIDs, productData.ScoreTagIDs);
        ShapeTags = new ShapeTags(productData.MoveTagIDs, productData.PlaceTagIDs);
    }
}