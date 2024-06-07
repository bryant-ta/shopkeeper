using System.Collections.Generic;
using System.Linq;
using MK.Toon;
using TriInspector;
using UnityEngine;

public class DeliveryBox : MonoBehaviour, IGridShape {
    #region DeliveryBox

    #endregion

    #region IGridShape

    [Title("Shape")]
    public string Name { get; private set; }
    public Grid Grid {
        get {
            if (ObjTransform.parent.TryGetComponent(out Grid grid)) {
                return grid;
            }

            Debug.LogError("IGridShape is not in a grid.");
            return null;
        }
    }

    public Transform ObjTransform => transform.parent;
    public Transform ColliderTransform => transform;
    public List<Collider> Colliders { get; private set; }

    [field: SerializeField] public ShapeData ShapeData { get; private set; }

    [field: SerializeField] public ShapeTags ShapeTags { get; private set; }

    Material mat;
    Color matOutlineOriginalColor;
    float matOutlineOriginalWeight;

    #endregion

    void Awake() {
        Init();
        
        Ref.Player.PlayerInput.InputSecondaryDown += HandleInput;
    }

    void Init() {
        if (ShapeData.ShapeOffsets == null || ShapeData.ShapeOffsets.Count == 0) {
            ShapeData = ShapeDataLookUp.LookUp(ShapeData.ID);
        }

        Colliders = GetComponents<Collider>().ToList();

        Name = gameObject.name;

        mat = GetComponent<MeshRenderer>().material;
        matOutlineOriginalColor = Properties.outlineColor.GetValue(mat);
        matOutlineOriginalWeight = Properties.outlineSize.GetValue(mat);
    }

    void HandleInput(ClickInputArgs clickInputArgs) {
        if (clickInputArgs.TargetObj == ColliderTransform.gameObject) {
            Open();
        }
    }

    void Open() {
        if (Grid != GameManager.WorldGrid) return;

        Grid.RemoveShapeCells(this, false);

        Ref.Instance.DeliveryMngr.GenerateBasicDelivery(ShapeData, Grid);
        
        // TEMP: until box open animation
        Destroy(ObjTransform.gameObject);
    }
    
    public void SetOutline(Color color) {
        Properties.outlineColor.SetValue(mat, color);
    }
    public void ResetOutline() { SetOutline(matOutlineOriginalColor); }
}
