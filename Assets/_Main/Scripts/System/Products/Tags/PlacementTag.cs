using UnityEngine;

namespace Tags {
public abstract class PlacementTag {
    
    // Executed on each cell of a shape
    // TODO: determine way to "pass different operator" to get all true/all false/at least one true
    public abstract bool Check(Vector3Int coord);
}

public class PlacementTagHeavy : PlacementTag {
    public override bool Check(Vector3Int coord) {
        return coord.y == 0;
    }
}

public class PlacementTagFragile : PlacementTag {
    public override bool Check(Vector3Int coord) {
        return coord.y == GameManager.Instance.GlobalGridHeight - 1;
    }
}

// TODO: Check() prob needs to take Grid as parameter
// public class ConstraintTagExposed : PlacementTag {
//     public override bool Check(Vector3Int coord) {
//         return coord.y == 0;
//     }
// }

public enum ConstraintTagID {
    None = 0,
    Heavy = 1,
    Fragile = 2,
    // Exposed = 3,
}
}