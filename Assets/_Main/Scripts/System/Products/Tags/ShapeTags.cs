using System.Collections.Generic;
using UnityEngine;

namespace Tags {
public class ShapeTags {
    public List<MoveTag> MoveTags = new();
    public List<PlaceTag> PlaceTags = new();
    
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