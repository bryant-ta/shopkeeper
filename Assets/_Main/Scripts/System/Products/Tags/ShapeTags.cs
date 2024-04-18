using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Tags {
[Serializable]
public class ShapeTags {
    [ReadOnly] public List<MoveTag> MoveTags = new();
    [ReadOnly] public List<PlaceTag> PlaceTags = new();

    public ShapeTags(List<MoveTagID> moveTagIDs, List<PlaceTagID> placeTagIDs) {
        foreach (MoveTagID id in moveTagIDs) {
            MoveTags.Add(LookUpMoveTag.LookUp(id));
        }

        foreach (PlaceTagID id in placeTagIDs) {
            PlaceTags.Add(LookUpPlaceTag.LookUp(id));
        }
    }

    public bool CheckMoveTags() {
        for (int i = 0; i < MoveTags.Count; i++) {
            if (!MoveTags[i].Check()) return false;
        }

        return true;
    }

    public bool CheckPlaceTags(Vector3Int rootCoord) {
        for (int i = 0; i < PlaceTags.Count; i++) {
            if (!PlaceTags[i].Check(rootCoord)) return false;
        }

        return true;
    }
}
}