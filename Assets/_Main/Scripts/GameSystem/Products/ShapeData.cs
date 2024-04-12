using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public enum ShapeType {
    O1 = 0, // length of one side of square shape
    O2 = 1,
    O3 = 2,
    O4 = 3,
    O5 = 4,
    I2 = 5, // length of I shape (straight)
    I3 = 6,
    I4 = 7,
    I5 = 8,
    Rect2x2 = 10, // length x width of rectangle shape
    Rect2x3 = 11,
    Rect2x4 = 12,
    Rect2x5 = 13,
    Rect3x3 = 14,
    Rect3x4 = 15,
    Rect3x5 = 16,
    Rect4x4 = 17,
    Rect5x5 = 18,
    L1x1 = 20, // length (x,z) of each arm of L shape, not counting corner
    L1x2 = 21,
    L1x3 = 22,
    L2x1 = 23,
    L3x1 = 24,
    L2x2 = 25,
    L3x3 = 26,
}

[Serializable]
public struct ShapeData {
    [ReadOnly] public List<Vector3Int> ShapeOffsets;
}

public static class ShapeDataLookUp {
    public static Dictionary<ShapeType, ShapeData> LookUp = new Dictionary<ShapeType, ShapeData>() {
        {
            ShapeType.O1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0)
                }
            }
        }, {
            ShapeType.O2, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                }
            }
        }, {
            ShapeType.I2, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                }
            }
        }, {
            ShapeType.Rect2x3, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                }
            }
        }, {
            ShapeType.L1x1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                }
            }
        },  {
            ShapeType.L1x3, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, 2),
                    new(0, 0, 3),
                }
            }
        }, 
        // {
        //     ShapeType.Cube2, new ShapeData() {
        //         ShapeOffsets = new List<Vector3Int>() {
        //             new(0, 0, 0),
        //             new(1, 0, 0),
        //             new(0, 0, 1),
        //             new(1, 0, 1),
        //             new(0, 1, 0),
        //             new(1, 1, 0),
        //             new(0, 1, 1),
        //             new(1, 1, 1),
        //         }
        //     }
        // },
    };
}