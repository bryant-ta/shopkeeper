using System.Collections.Generic;
using UnityEngine;

namespace Tags {
public class ShapeTags {
    public List<PlacementTag> PlacementTags = new();

    public bool CheckAllTags(Vector3Int rootCoord) {
        for (int i = 0; i < PlacementTags.Count; i++) {
            if (!PlacementTags[i].Check(rootCoord)) return false;
        }

        return true;
    }
}
}