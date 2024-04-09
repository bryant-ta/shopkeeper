using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType {
    _1x1x1 = 0,
    _1x2x1 = 1,
    _3x2x1 = 2,
    _2x2x2 = 3,
}

[Serializable]
public struct ShapeData {
    [NonSerialized] public List<Vector3Int> ShapeOffsets;
}

public static class ShapeDataLookUp {
    public static Dictionary<ShapeType, ShapeData> LookUp = new Dictionary<ShapeType, ShapeData>() {
        {
            ShapeType._1x1x1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0,0,0)
                }
            }
        },
        {
            ShapeType._1x2x1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0,0,0),
                    new(1,0,0),
                }
            }
        },
        {
            ShapeType._3x2x1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0,0,0),
                    new(1,0,0),
                    new(2,0,0),
                    new(0,0,1),
                    new(1,0,1),
                    new(2,0,1),
                }
            }
        },
        {
            ShapeType._2x2x2, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
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
