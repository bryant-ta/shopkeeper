using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

public enum ShapeDataID {
    None = 0,
    O1 = 1, // length of one side of square shape
    O2 = 2,
    O3 = 3,
    O4 = 4,
    O5 = 5,
    I2 = 6, // length of I shape (straight)
    I3 = 7,
    I4 = 8,
    I5 = 9,
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

    public bool NeighborExists(Vector3Int coord, Direction dir) {
        return dir switch {
            Direction.North => ShapeOffsets.Contains(coord + Vector3Int.forward),
            Direction.East => ShapeOffsets.Contains(coord + Vector3Int.right),
            Direction.South => ShapeOffsets.Contains(coord + Vector3Int.back),
            Direction.West => ShapeOffsets.Contains(coord + Vector3Int.left),
            Direction.Up => ShapeOffsets.Contains(coord + Vector3Int.up),
            Direction.Down => ShapeOffsets.Contains(coord + Vector3Int.down),
            _ => false
        };
    }
}

public static class ShapeDataLookUp {
    public static Dictionary<ShapeDataID, ShapeData> LookUp = new Dictionary<ShapeDataID, ShapeData>() {
        {
            ShapeDataID.O1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0)
                }
            }
        }, {
            ShapeDataID.O2, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                }
            }
        }, {
            ShapeDataID.I2, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                }
            }
        }, {
            ShapeDataID.Rect2x3, new ShapeData() {
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
            ShapeDataID.L1x1, new ShapeData() {
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                }
            }
        },  {
            ShapeDataID.L1x3, new ShapeData() {
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
        //     ShapeDataID.Cube2, new ShapeData() {
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