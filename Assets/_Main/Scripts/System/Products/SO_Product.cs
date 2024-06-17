using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ID;
    [SerializeField] ShapeData shapeData;
    public ShapeData ShapeData {
        get => shapeData;
        set {
            shapeData = value;
            ID.SetShapeData(value);
        }
    }

    public List<ShapeTagID> ShapeTagIDs = new();
}

[Serializable]
public struct ProductID {
    public Color Color;
    public Pattern Pattern;
    public ShapeDataID ShapeDataID => ShapeData.ID; // this pattern kinda sucks to use... don't do this again

    public ShapeData ShapeData { get; private set; }

    public ProductID(Color color, Pattern pattern, ShapeData shapeData) {
        Color = color;
        Pattern = pattern;
        ShapeData = shapeData;
    }

    public override int GetHashCode() {
        unchecked {
            int hash = 17;
            hash = hash * 23 + Color.GetHashCode();
            hash = hash * 23 + Pattern.GetHashCode();
            hash = hash * 23 + ShapeDataID.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object obj) {
        if (!(obj is ProductID other)) return false;

        return Color == other.Color && Pattern == other.Pattern && ShapeDataID == other.ShapeDataID;
    }

    public override string ToString() {
        if (ShapeData == null) return $"None_{Color}_{Pattern}";
        return $"{ShapeDataID}_{Color}_{Pattern}";
    }

    /// <summary>
    /// Generally should never be called manually!! Is called by SO_Product.ShapeData setter.
    /// </summary>
    public void SetShapeData(ShapeData shapeData) { ShapeData = shapeData; }
}

public enum Pattern {
    None = 0,
    StripeHor = 1,
    StripeVert = 2,
    StripeDiag = 3,
    Scratch = 10,
}