using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilemapData", menuName = "ScriptableObjects/TilemapData", order = 1)]
public class SO_OrderLayout : ScriptableObject {
    public int DifficultyRating;
    public TileData[] Tiles;
    
    [Serializable]
    public struct TileData {
        public Vector2Int coord;
        public int colorID;
    }

    public Dictionary<int, int> ColorIDCounts() {
        Dictionary<int, int> ret = new();
        foreach (TileData tile in Tiles) {
            Util.DictIntAdd(ret, tile.colorID, 1);
        }
        return ret;
    }
}