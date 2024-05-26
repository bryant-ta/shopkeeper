using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(HoverEvent))]
public class Trash : MonoBehaviour {
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
}