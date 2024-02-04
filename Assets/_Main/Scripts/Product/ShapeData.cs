using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType {
    _1x1x1,
    _1x2x1,
    _2x2x2,
}

[Serializable]
public struct ShapeData {
    [HideInInspector] public List<Vector3Int> Shape;
}

public static class ShapeDataLookUp {
    public static Dictionary<ShapeType, ShapeData> LookUp = new Dictionary<ShapeType, ShapeData>() {
        {
            ShapeType._1x1x1, new ShapeData() {
                Shape = new List<Vector3Int>() {
                    new(0,0,0)
                }
            }
        },
        {
            ShapeType._1x2x1, new ShapeData() {
                Shape = new List<Vector3Int>() {
                    new(0,0,0),
                    new(1,0,0),
                }
            }
        },
        {
            ShapeType._2x2x2, new ShapeData() {
                Shape = new List<Vector3Int>() {
                    new(0,0,0),
                    new(1,0,0),
                    new(0,0,1),
                    new(1,0,1),
                    new(0,1,0),
                    new(1,1,0),
                    new(0,1,1),
                    new(1,1,1),
                }
            }
        },
    };
}
