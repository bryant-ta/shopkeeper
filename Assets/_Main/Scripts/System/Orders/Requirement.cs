using UnityEngine;

namespace Orders {
public class Requirement {
    public Color Color;
    public Pattern? Pattern;
    public ShapeDataID? ShapeDataID;
    public int TargetQuantity;
    public int CurQuantity;
    public bool IsFulfilled => QuantityUntilTarget() == 0;

    public Requirement(Color color, Pattern? pattern, ShapeDataID? shapeDataID, int targetQuantity = -1) {
        if (targetQuantity == 0) { Debug.LogError("Requirement should not be created with TargetQuantity = 0"); }

        Color = color;
        Pattern = pattern;
        ShapeDataID = shapeDataID;
        TargetQuantity = targetQuantity;
    }

    public int QuantityUntilTarget() {
        int r = TargetQuantity - CurQuantity;
        if (r < 0) r = 0;
        return r;
    }

    public bool Match(ProductID productID) {
        return (Color == productID.Color) &&
               (Pattern == null || Pattern == productID.Pattern) &&
               (ShapeDataID == null || ShapeDataID == productID.ShapeDataID);
    }

    public bool Match(Requirement requirement) {
        return Color == requirement.Color && Pattern == requirement.Pattern && ShapeDataID == requirement.ShapeDataID;
    }
}
}