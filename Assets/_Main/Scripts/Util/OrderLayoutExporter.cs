using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Linq;

public class OrderLayoutExporter : MonoBehaviour {
    [SerializeField] Tilemap tilemap;
    [SerializeField] List<TileBase> tiles;

    public void ExportToScriptableObject(string filePath) {
        // Extract directory and base file name from the provided file path
        string directory = Path.GetDirectoryName(filePath);
        string baseFileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        // Search for existing files with the same base file name
        string[] existingFiles = Directory.GetFiles(directory, baseFileName + "_*.asset");

        // Determine the next available number suffix
        int nextNumber = 1;
        if (existingFiles != null && existingFiles.Length > 0) {
            List<int> numbers = new List<int>();
            foreach (string file in existingFiles) {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string numberPart = fileName.Substring(baseFileName.Length + 1);
                int number;
                if (int.TryParse(numberPart, out number)) {
                    numbers.Add(number);
                }
            }
            nextNumber = numbers.Count > 0 ? numbers.Max() + 1 : 1;
        }

        // Construct the new file name with incremented number
        string newFileName = baseFileName + "_" + nextNumber + extension;
        string newFilePath = Path.Combine(directory, newFileName);

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        List<SO_OrderLayout.TileData> tileDataList = new List<SO_OrderLayout.TileData>();

        for (int y = bounds.yMin; y < bounds.yMax; y++) {
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                TileBase tile = allTiles[x - bounds.xMin + (y - bounds.yMin) * bounds.size.x];
                if (tile != null) {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    TileBase tileBase = tilemap.GetTile(tilePos);

                    tileDataList.Add(
                        new SO_OrderLayout.TileData {
                            coord = new Vector2Int(x, y),
                            colorID = tiles.IndexOf(tileBase)
                        }
                    );
                }
            }
        }

        SO_OrderLayout orderLayoutData = ScriptableObject.CreateInstance<SO_OrderLayout>();
        orderLayoutData.tiles = tileDataList.ToArray();

        AssetDatabase.CreateAsset(orderLayoutData, newFilePath);
        AssetDatabase.SaveAssets();

        Debug.Log("Tilemap exported to " + newFilePath);
    }
}
