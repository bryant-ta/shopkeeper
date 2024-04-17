using System.Collections.Generic;

namespace Tags {
public class ProductTags {
    public List<BasicTagID> BasicTags = new();
    public List<ScoreTag> ScoreTags = new();

    public int ScoreAllTags(int baseScore) {
        int score = 0;
        for (int i = 0; i < ScoreTags.Count; i++) {
            score += ScoreTags[i].CalculateScore(baseScore);
        }

        return score;
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