using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType {
    O1,
}

[Serializable]
public struct ShapeData {
    [HideInInspector] public List<Vector3Int> Shape;
}

public static class ShapeDataLookUp {
    public static Dictionary<ShapeType, ShapeData> LookUp = new Dictionary<ShapeType, ShapeData>() {
        {
            ShapeType.O1, new ShapeData() {
                Shape = new List<Vector3Int>() {
                    new(0,0)
                }
            }
        },
    };
}
