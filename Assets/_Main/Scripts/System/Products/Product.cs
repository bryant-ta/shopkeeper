using System.Collections.Generic;
using System.Linq;
using MK.Toon;
using TriInspector;
using UnityEngine;

public class Product : MonoBehaviour, IGridShape {
    [field: SerializeField] public SO_Product ProductData { get; private set; }

    #region Product

    [field: SerializeField, Title("Product"), ReadOnly]
    public ProductID ID { get; private set; }

    #endregion

    #region IGridShape

    [Title("Shape")]
    public string Name { get; private set; }
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
    public List<Collider> Colliders { get; private set; }

    [field: SerializeField, ReadOnly] public ShapeData ShapeData { get; set; }

    [field: SerializeField, HideInEditMode]
    public ShapeTags ShapeTags { get; private set; }

    Material mat;
    Color matOutlineOriginalColor;
    float matOutlineOriginalWeight;

    #endregion

    void Awake() {
        if (ProductData == null) {
            // Debug.Log($"Product {gameObject.name} did not self-init.");
            return;
        }

        Init(ProductData);
    }

    public void Init(SO_Product productData) {
        if (ProductData == null) ProductData = productData;

        ShapeData = ProductData.ShapeData;
        if (ShapeData.ShapeOffsets == null || ShapeData.ShapeOffsets.Count == 0) {
            ShapeData = ShapeDataLookUp.LookUp(ShapeData.ID);
        }

        VoxelMeshGenerator.Generate(gameObject, ShapeData);

        ShapeTransform = transform.parent;
        Colliders = GetComponents<Collider>().ToList();

        ID = ProductData.ID;
        gameObject.name = Name = ProductData.ID.ToString();

        mat = GetComponent<MeshRenderer>().material;
        matOutlineOriginalColor = Properties.outlineColor.GetValue(mat);
        matOutlineOriginalWeight = Properties.outlineSize.GetValue(mat);
        Properties.albedoColor.SetValue(mat, ID.Color);
        // MK.Toon.Properties.sketchMap.SetValue(mat, _productData.Pattern); // TODO: Pattern lookup

        ShapeTags = new ShapeTags(ProductData.ShapeTagIDs);
    }
    
    public void SetOutline(Color color, float weight) {
        Properties.outlineColor.SetValue(mat, color);
        Properties.outlineSize.SetValue(mat, weight);
    }
    public void ResetOutline() { SetOutline(matOutlineOriginalColor, matOutlineOriginalWeight); }
}