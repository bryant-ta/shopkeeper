using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[Serializable]
public class ShapeTags {
    [SerializeField] List<ShapeTagID> Tags = new();

    public ShapeTags(List<ShapeTagID> tagIDs) { Tags.AddRange(tagIDs); }

    public bool Contains(ShapeTagID tagID) { return Tags.Contains(tagID); }

    public static bool CheckTags(List<IGridShape> shapes, ShapeTagID tagID) {
        foreach (IGridShape shape in shapes) {
            if (shape.ShapeTags.Contains(tagID)) {
                return true;
            }
        }
        return false;
    }
}

public enum ShapeTagID {
    None = 0,
    NoMove = 1,
    NoStack = 2,
    NoPlaceInOrder = 3,
    NoPlaceInTrash = 4,
    NoSlice = 10,
    NoCombine = 11,
}