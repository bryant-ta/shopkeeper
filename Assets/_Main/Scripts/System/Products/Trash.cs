using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(HoverEvent))]
public class Trash : MonoBehaviour {
    void Awake() {
        HoverEvent he = GetComponent<HoverEvent>();
        he.OnHoverEnter += Open;
        he.OnHoverExit += Close;
    }
    
    public void TrashShape(IGridShape shape, Grid originGrid) {
        originGrid.DestroyShape(shape);
        if (shape.ColliderTransform.TryGetComponent(out Product product)) {
            Ledger.RemoveStockedProduct(product);
        }
    }

    public void TrashShapes(List<IGridShape> shapes, Grid originGrid) {
        foreach (IGridShape shape in shapes) {
            TrashShape(shape, originGrid);
        }
    }

    void Open() {
        // TEMP: replace with open/close animation
        transform.DOKill(true);
        transform.DOShakeScale(0.2f, 0.5f, 10, 90);
    }

    void Close() {
        // TEMP: replace with open/close animation
        transform.DOKill(true);
        transform.DOShakeScale(0.2f, 0.5f, 10, 90);
        
    }
}