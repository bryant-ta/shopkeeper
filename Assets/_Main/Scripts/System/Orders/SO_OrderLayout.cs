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

    public ShapeData GetColorShapeData() {
        List<Vector3Int> offsets = new();
        foreach (TileData tile in Tiles) {
            offsets.Add(new Vector3Int(tile.coord.x, 0, tile.coord.y));
        }

        return new ShapeData(ShapeDataID.Custom, Vector3Int.zero, offsets);
    }

    public Dictionary<Vector3Int, int> GetTilesDict() {
        Dictionary<Vector3Int, int> ret = new();
        foreach (TileData tile in Tiles) {
            ret.Add(new Vector3Int(tile.coord.x, 0, tile.coord.y), tile.colorID);
        }
        return ret;
    }

    public SO_OrderLayout Copy() {
        SO_OrderLayout copy = CreateInstance<SO_OrderLayout>();
        copy.DifficultyRating = DifficultyRating;

        copy.Tiles = new TileData[Tiles.Length];
        for (int i = 0; i < Tiles.Length; i++) {
            copy.Tiles[i] = new TileData {
                coord = Tiles[i].coord,
                colorID = Tiles[i].colorID
            };
        }

        return copy;
    }
}