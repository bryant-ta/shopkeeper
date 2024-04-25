using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

[Serializable]
public class ShapeData {
    public ShapeDataID ID;
    [field: SerializeField, ReadOnly] public Vector3Int RootCoord { get; set; }
    [ReadOnly] public List<Vector3Int> ShapeOffsets = new();

    // Rotates shape data to match a CW/CCW rotation. No physical gameobject rotation
    public void RotateShape(bool clockwise) {
        int cw = clockwise ? 1 : -1;

        // Rotate root coord
        RootCoord = new Vector3Int(RootCoord.z * cw, RootCoord.y, -RootCoord.x * cw);

        List<Vector3Int> rotatedShapeOffsets = new();
        foreach (Vector3Int offset in ShapeOffsets) {
            Vector3Int rotatedOffset = new Vector3Int(offset.z * cw, offset.y, -offset.x * cw);
            rotatedShapeOffsets.Add(rotatedOffset);
        }

        ShapeOffsets = rotatedShapeOffsets;
    }

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

    /// <summary>
    /// Get ShapeDataID from ShapeOffsets, including if ShapeOffsets is rotated
    /// </summary>
    public static ShapeDataID DetermineID(List<Vector3Int> shapeOffsets) {
        if (shapeOffsets == null || shapeOffsets.Count == 0) {
            Debug.LogError("Unable to match shape data ID: ShapeOffset is not set.");
            return ShapeDataID.None;
        }

        foreach (KeyValuePair<ShapeDataID, ShapeData> kv in ShapeDataLookUp.LookUp) {
            // First match offsets length
            if (shapeOffsets.Count != kv.Value.ShapeOffsets.Count) continue;

            // match offsets considering rotation
            ShapeData sd = new ShapeData {ShapeOffsets = new List<Vector3Int>(shapeOffsets)};
            for (int i = 0; i < 4; i++) {
                if (shapeOffsets.All(sd.ShapeOffsets.Contains)) {
                    return kv.Key;
                }

                sd.RotateShape(true);
            }
        }

        Debug.LogError("Unable to match shape data ID: Did not match any shape.");
        return ShapeDataID.None;
    }
}

public enum ShapeDataID {
    None = 0,
    O1 = 1, // length of one side of square shape
    // O2 = 2,
    // O3 = 3,
    // O4 = 4,
    // O5 = 5,
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

public static class ShapeDataLookUp {
    public static Dictionary<ShapeDataID, ShapeData> LookUp = new() {
        {
            ShapeDataID.O1, new ShapeData() {
                ID = ShapeDataID.O1,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0)
                }
            }
        }, {
            ShapeDataID.I2, new ShapeData() {
                ID = ShapeDataID.I2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                }
            }
        }, {
            ShapeDataID.I3, new ShapeData() {
                ID = ShapeDataID.I3,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                }
            }
        }, {
            ShapeDataID.I4, new ShapeData() {
                ID = ShapeDataID.I4,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                }
            }
        }, {
            ShapeDataID.I5, new ShapeData() {
                ID = ShapeDataID.I5,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                    new(4, 0, 0),
                }
            }
        }, {
            ShapeDataID.Rect2x2, new ShapeData() {
                ID = ShapeDataID.Rect2x2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                }
            }
        }, {
            ShapeDataID.Rect2x3, new ShapeData() {
                ID = ShapeDataID.Rect2x3,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                }
            }
        }, {
            ShapeDataID.Rect2x4, new ShapeData() {
                ID = ShapeDataID.Rect2x4,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                }
            }
        }, {
            ShapeDataID.Rect2x5, new ShapeData() {
                ID = ShapeDataID.Rect2x5,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                    new(0, 0, 4),
                    new(1, 0, 4),
                }
            }
        }, {
            ShapeDataID.Rect3x3, new ShapeData() {
                ID = ShapeDataID.Rect3x3,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(2, 0, 2),
                }
            }
        }, {
            ShapeDataID.Rect3x4, new ShapeData() {
                ID = ShapeDataID.Rect3x4,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(2, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                    new(2, 0, 3),
                }
            }
        }, {
            ShapeDataID.Rect3x5, new ShapeData() {
                ID = ShapeDataID.Rect3x5,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(2, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                    new(2, 0, 3),
                    new(0, 0, 4),
                    new(1, 0, 4),
                    new(2, 0, 4),
                }
            }
        }, {
            ShapeDataID.Rect4x4, new ShapeData() {
                ID = ShapeDataID.Rect4x4,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                    new(3, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(2, 0, 2),
                    new(3, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                    new(2, 0, 3),
                    new(3, 0, 3),
                }
            }
        }, {
            ShapeDataID.Rect5x5, new ShapeData() {
                ID = ShapeDataID.Rect5x5,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                    new(4, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                    new(2, 0, 1),
                    new(3, 0, 1),
                    new(4, 0, 1),
                    new(0, 0, 2),
                    new(1, 0, 2),
                    new(2, 0, 2),
                    new(3, 0, 2),
                    new(4, 0, 2),
                    new(0, 0, 3),
                    new(1, 0, 3),
                    new(2, 0, 3),
                    new(3, 0, 3),
                    new(4, 0, 3),
                    new(0, 0, 4),
                    new(1, 0, 4),
                    new(2, 0, 4),
                    new(3, 0, 4),
                    new(4, 0, 4),
                }
            }
        }, {
            ShapeDataID.L1x1, new ShapeData() {
                ID = ShapeDataID.L1x1,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                }
            }
        }, {
            ShapeDataID.L1x2, new ShapeData() {
                ID = ShapeDataID.L1x2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, 2),
                }
            }
        }, {
            ShapeDataID.L1x3, new ShapeData() {
                ID = ShapeDataID.L1x3,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, 2),
                    new(0, 0, 3),
                }
            }
        }, {
            ShapeDataID.L2x1, new ShapeData() {
                ID = ShapeDataID.L2x1,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                }
            }
        }, {
            ShapeDataID.L3x1, new ShapeData() {
                ID = ShapeDataID.L3x1,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                    new(0, 0, 1),
                }
            }
        }, {
            ShapeDataID.L2x2, new ShapeData() {
                ID = ShapeDataID.L2x2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, 2),
                }
            }
        }, {
            ShapeDataID.L3x3, new ShapeData() {
                ID = ShapeDataID.L3x3,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                    new(3, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, 2),
                    new(0, 0, 3),
                }
            }
        },
    };
}