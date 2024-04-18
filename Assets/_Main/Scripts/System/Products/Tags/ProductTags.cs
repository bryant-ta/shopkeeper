using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Tags {
[Serializable]
public class ProductTags {
    public List<BasicTagID> BasicTags = new();
    [ReadOnly] public List<ScoreTag> ScoreTags = new();

    public ProductTags(List<BasicTagID> basicTagIDs, List<ScoreTagID> scoreTagIDs) {
        BasicTags = basicTagIDs;

        foreach (ScoreTagID id in scoreTagIDs) {
            // TEMP: 0 is placeholder value until working on scoring values
            ScoreTags.Add(LookUpScoreTag.LookUp(id, 0)); 
        }
    }

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