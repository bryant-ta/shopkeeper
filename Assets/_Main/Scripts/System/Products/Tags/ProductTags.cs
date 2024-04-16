using System.Collections.Generic;

namespace ProductTag {
public class ProductTags {
    public List<BasicTagID> BasicTags;
    public List<ScoreTag> ScoreTags;
    public List<ConstraintTag> ConstraintTags;

    public void ScoreTags() {
        
    }

    public bool CheckTags() {
        for (int i = 0; i < ConstraintTags.Count; i++) {
            if (!ConstraintTags[i].Check()) return false;
        }

        return true;
    }
}

public enum BasicTagID {
    None = 0,
    Food = 1,
    Potion = 2,
    Weapon = 3,
    Armor = 4,
    Magic = 5,
    Tool = 6,
    Trade = 7,
    Quest = 8,
}
}