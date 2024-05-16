using System;
using System.Collections.Generic;
using Tags;
using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID productID;
    public ShapeData ShapeData;
    public Color Color;     // temp
    public Pattern Pattern; // temp

    public List<BasicTagID> BasicTagIDs;
    public List<ScoreTagID> ScoreTagIDs;
    public List<MoveTagID> MoveTagIDs;
    public List<PlaceTagID> PlaceTagIDs;
}

[Serializable]
public struct ProductID {
    public Color Color;
    public Pattern Pattern;
    public ShapeDataID ShapeDataID => ShapeData.ID;

    ShapeData ShapeData;

    public ProductID(Color color, Pattern pattern, ShapeData shapeData) {
        Color = default;
        Pattern = Pattern.None;
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
}

public enum Pattern {
    None = 0,
    StripeHor = 1,
    StripeVert = 2,
    StripeDiag = 3,
    Scratch = 10,
}