namespace Tags {
public abstract class MoveTag {
    
    // Executed on each cell of a shape
    public virtual bool Check() {
        return true;
    }
}

public class MoveTagAnchored : MoveTag {
    public override bool Check() {
        return false;
    }
}

public enum MoveTagID {
    None = 0,
    Anchored = 1,
}
}