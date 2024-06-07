using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[Serializable]
public class ShapeTags {
    [SerializeField, ReadOnly] List<ShapeTagID> Tags = new();

    public ShapeTags(List<ShapeTagID> tagIDs) { Tags.AddRange(tagIDs); }

    public bool Contains(ShapeTagID tagID) { return Tags.Contains(tagID); }
}

public enum ShapeTagID {
    None = 0,
    Anchored = 1,
    Unstackable = 2,
    Unsliceable = 10,
    Uncombinable = 11,
}