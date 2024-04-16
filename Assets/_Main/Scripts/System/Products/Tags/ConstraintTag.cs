using UnityEngine;

namespace ProductTag {
public abstract class ConstraintTag {
    
    // Executed on each cell of a shape
    // TODO: determine way to "pass different operator" to get all true/all false/at least one true
    public abstract bool Check(Vector3Int coord);
}

public class ConstraintTagHeavy : ConstraintTag {
    public override bool Check(Vector3Int coord) {
        return coord.y == 0;
    }
}

public class ConstraintTagFragile : ConstraintTag {
    public override bool Check(Vector3Int coord) {
        return coord.y == GameManager.Instance.GlobalGridHeight - 1;
    }
}

public class ConstraintTagExposed : ConstraintTag {
    public override bool Check(Vector3Int coord) {
        return coord.y == 0;
    }
}

public enum ConstraintTagID {
    None = 0,
    Heavy = 1,
    Fragile = 2,
    Exposed = 3,
}
}