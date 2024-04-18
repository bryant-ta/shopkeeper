using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Tags {
[Serializable]
public abstract class MoveTag {
    [field: SerializeField, ReadOnly] protected MoveTagID id;
    
    // Executed on each cell of a shape
    public virtual bool Check() {
        return true;
    }
}

public class MoveTagAnchored : MoveTag {
    public MoveTagAnchored() {
        id = MoveTagID.Anchored;
    }
    
    public override bool Check() {
        return false;
    }
}

public enum MoveTagID {
    None = 0,
    Anchored = 1,
}

public static class LookUpMoveTag {
    static Dictionary<MoveTagID, MoveTag> LookUpDict = new() {
        {MoveTagID.None, null},
        {MoveTagID.Anchored, new MoveTagAnchored()},
    };

    public static MoveTag LookUp(MoveTagID id) {
        MoveTag tag = LookUpDict[id];
        if (tag == null) {
            if (id == MoveTagID.None) return null;
            Debug.LogError("Unable to look up Place Tag ID.");
            return null;
        }

        return tag;
    }
}
}