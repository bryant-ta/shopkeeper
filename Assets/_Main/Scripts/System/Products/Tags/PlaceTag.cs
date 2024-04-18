using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Tags {
[Serializable]
public abstract class PlaceTag {
    [field: SerializeField, ReadOnly] protected PlaceTagID id;
    
    // Executed on each cell of a shape
    // TODO: determine way to "pass different operator" to get all true/all false/at least one true
    public abstract bool Check(Vector3Int coord);
}

public class PlaceTagHeavy : PlaceTag {
    public PlaceTagHeavy() {
        id = PlaceTagID.Heavy;
    }

    public override bool Check(Vector3Int coord) {
        return coord.y == 0;
    }
}

public class PlaceTagFragile : PlaceTag {
    public PlaceTagFragile() {
        id = PlaceTagID.Fragile;
    }
    
    public override bool Check(Vector3Int coord) {
        return coord.y == GameManager.Instance.GlobalGridHeight - 1;
    }
}

// TODO: Check() prob needs to take Grid as parameter
// public class ConstraintTagExposed : PlaceTag {
//     public override bool Check(Vector3Int coord) {
//         return coord.y == 0;
//     }
// }

public enum PlaceTagID {
    None = 0,
    Heavy = 1,
    Fragile = 2,
    // Exposed = 3,
}

public static class LookUpPlaceTag {
    static Dictionary<PlaceTagID, PlaceTag> LookUpDict = new() {
        {PlaceTagID.None, null},
        {PlaceTagID.Heavy, new PlaceTagHeavy()},
        {PlaceTagID.Fragile, new PlaceTagFragile()},
    };

    public static PlaceTag LookUp(PlaceTagID id) {
        PlaceTag tag = LookUpDict[id];
        if (tag == null) {
            if (id == PlaceTagID.None) return null;
            Debug.LogError("Unable to look up Place Tag ID.");
            return null;
        }

        return tag;
    }
}
}