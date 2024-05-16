using System.Collections.Generic;
using System.Linq;
using Tags;
using TriInspector;
using UnityEngine;


public class Product : MonoBehaviour, IGridShape {
    [SerializeField] SO_Product productData;

    #region Product
    
    [field: SerializeField, Title("Product"), ReadOnly]
    public ProductID ID { get; private set; }
    public string Name { get; private set; }

    public Color Color { get; private set; }
    public Pattern Pattern { get; private set; }

    [field: SerializeField, HideInEditMode] public ProductTags ProductTags { get; private set; }

    #endregion

    #region IGridShape

    [Title("Shape")]
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

    [field: SerializeField, HideInEditMode] public ShapeTags ShapeTags { get; private set; }

    Material mat;
    Color matOutlineOriginalColor;
    float matOutlineOriginalWeight;

    #endregion

    void Awake() {
        Colliders = new();

        if (productData == null) {
            // Debug.Log($"Product {gameObject.name} did not self-init.");
            return;
        }

        Init(productData);
    }

    public void Init(SO_Product _productData) {
        if (productData == null) productData = _productData;

        ShapeData = productData.ShapeData;
        if (ShapeData.ShapeOffsets == null || ShapeData.ShapeOffsets.Count == 0) {
            ShapeData = ShapeDataLookUp.LookUp[ShapeData.ID];
        }

        VoxelMeshGenerator.Generate(gameObject, ShapeData);

        ShapeTransform = transform.parent;
        Colliders = GetComponents<Collider>().ToList();

        ID = productData.productID;
        Name = productData.productID.ToString();
        gameObject.name = Name;
        Color = productData.Color;
        Pattern = productData.Pattern;
        
        mat = GetComponent<MeshRenderer>().material;
        matOutlineOriginalColor = MK.Toon.Properties.outlineColor.GetValue(mat);
        matOutlineOriginalWeight = MK.Toon.Properties.outlineSize.GetValue(mat);
        MK.Toon.Properties.albedoColor.SetValue(mat, Color);
        // MK.Toon.Properties.sketchMap.SetValue(mat, _productData.Pattern); // TODO: Pattern lookup

        ProductTags = new ProductTags(productData.BasicTagIDs, productData.ScoreTagIDs);
        ShapeTags = new ShapeTags(productData.MoveTagIDs, productData.PlaceTagIDs);
    }
    public void SetOutline(Color color, float weight) {
        MK.Toon.Properties.outlineColor.SetValue(mat, color);
        MK.Toon.Properties.outlineSize.SetValue(mat, weight);
    }
    public void ResetOutline() {
        SetOutline(matOutlineOriginalColor, matOutlineOriginalWeight);
    }
}