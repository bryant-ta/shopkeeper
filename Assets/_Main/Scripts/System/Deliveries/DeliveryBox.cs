using System.Collections.Generic;
using System.Linq;
using MK.Toon;
using TriInspector;
using UnityEngine;

public class DeliveryBox : MonoBehaviour, IGridShape {
    #region DeliveryBox

    [SerializeField] ShapeData shapeData;

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

    public ShapeData ShapeData => shapeData;

    [field: SerializeField, HideInEditMode]
    public ShapeTags ShapeTags { get; private set; }

    Material mat;
    Color matOutlineOriginalColor;
    float matOutlineOriginalWeight;

    #endregion

    void Awake() {
        Init();
    }

    public void Init() {
        if (ShapeData.ShapeOffsets == null || ShapeData.ShapeOffsets.Count == 0) {
            shapeData = ShapeDataLookUp.LookUp(ShapeData.ID);
        }

        ShapeTransform = transform.parent;
        Colliders = GetComponents<Collider>().ToList();

        Name = gameObject.name;

        mat = GetComponent<MeshRenderer>().material;
        matOutlineOriginalColor = Properties.outlineColor.GetValue(mat);
        matOutlineOriginalWeight = Properties.outlineSize.GetValue(mat);
    }
    
    public void SetOutline(Color color, float weight) {
        Properties.outlineColor.SetValue(mat, color);
        Properties.outlineSize.SetValue(mat, weight);
    }
    public void ResetOutline() { SetOutline(matOutlineOriginalColor, matOutlineOriginalWeight); }
}
