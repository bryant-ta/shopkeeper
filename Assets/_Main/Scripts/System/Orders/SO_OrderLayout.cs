using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TilemapData", menuName = "ScriptableObjects/TilemapData", order = 1)]
public class SO_OrderLayout : ScriptableObject {
    [Serializable]
    public struct TileData {
        public Vector2Int coord;
        public int colorID;
    }

    public TileData[] tiles;
}