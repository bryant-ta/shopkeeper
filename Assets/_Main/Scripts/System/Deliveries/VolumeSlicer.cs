using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VolumeSlicer : MonoBehaviour {
    [SerializeField] int maxShapeLength;
    [SerializeField] int maxShapeWidth;

    [SerializeField, Range(0f,1f)] float chanceOfShapeExtension;

    [SerializeField] int maxIterations = 100;

    /*
    1. Within 2D y layer, pick 1 random point
    2. random roll choose a valid neighbor/direction
    3. random roll to add neighbor to group
         success → repeat in same direction or until not valid direction
         fail → random roll orthogonal direction, random roll try add all adjacent cells to group in that direction 
    (to form a rectangle) (can fail if not all cells are open)
    */
    public Dictionary<Vector3Int, ShapeData> Slice(Vector3Int minBounds, Vector3Int maxBounds) {
        HashSet<Vector2Int> validLayerCells = new();
        for (int x = minBounds.x; x <= maxBounds.x; x++) {
            for (int z = minBounds.z; z <= maxBounds.z; z++) {
                validLayerCells.Add(new Vector2Int(x, z));
            }
        }
        
        Dictionary<Vector3Int, ShapeData> volumeData = new(); // shape data and their root coords
        for (int y = minBounds.y; y <= maxBounds.y; y++) {
            HashSet<Vector2Int> curValidLayerCells = new HashSet<Vector2Int>(validLayerCells);
            int iterations = 0;
            while (curValidLayerCells.Count > 0 && iterations < maxIterations) {
                iterations++;
                ShapeData shapeData = new ShapeData {ShapeOffsets = new List<Vector3Int> {Vector3Int.zero}};

                // 1
                Vector2Int baseCoord = curValidLayerCells.ToList()[Random.Range(0, curValidLayerCells.Count)]; // can optimize
                curValidLayerCells.Remove(baseCoord);
                Vector3Int baseCoord3D = new Vector3Int(baseCoord.x, y, baseCoord.y);

                // 2
                List<Direction2D> validDirs = ValidDirections(curValidLayerCells, baseCoord);
                if (validDirs.Count == 0) { // is 1x1x1
                    shapeData.ID = ShapeData.DetermineID(shapeData.ShapeOffsets);
                    volumeData[baseCoord3D] = shapeData;
                    continue;
                }

                Direction2D shapeLengthDir = validDirs[Random.Range(0, validDirs.Count)];

                // 3
                Vector2Int curCoord = baseCoord;
                Vector2Int curOffset = Vector2Int.zero;
                int curShapeLength = 1;
                while (Random.Range(0, 1f) > chanceOfShapeExtension && NeighborExists(curValidLayerCells, curCoord, shapeLengthDir)) {
                    if (curShapeLength >= maxShapeLength) break;

                    Vector2Int n = GetNeighbor(curOffset, shapeLengthDir);
                    Vector3Int newOffset = new Vector3Int(n.x, 0, n.y);
                    shapeData.ShapeOffsets.Add(newOffset);

                    curCoord = GetNeighbor(curCoord, shapeLengthDir);
                    curValidLayerCells.Remove(curCoord);
                    curOffset = n;
                    curShapeLength++;
                }

                //TODO: randomly roll rectangle
                
                shapeData.ID = ShapeData.DetermineID(shapeData.ShapeOffsets);
                volumeData[baseCoord3D] = shapeData;
            }
        }
        
        // Check
        HashSet<Vector3Int> claimedCells = new();
        foreach (KeyValuePair<Vector3Int,ShapeData> kv in volumeData) {
            foreach (Vector3Int offset in kv.Value.ShapeOffsets) {
                if (claimedCells.Contains(kv.Key + offset)) {
                    Debug.LogWarning($"shapes overlapping in VolumeSlicer at {offset}");
                }

                claimedCells.Add(kv.Key + offset);
            }
        }

        return volumeData;
    }

    List<Direction2D> ValidDirections(HashSet<Vector2Int> validLayerCells, Vector2Int baseCoord) {
        int[] allDirs = (int[]) Enum.GetValues(typeof(Direction2D));
        List<Direction2D> validDirs = new();
        for (int i = 0; i < allDirs.Length; i++) {
            if (NeighborExists(validLayerCells, baseCoord, (Direction2D)allDirs[i])) {
                validDirs.Add((Direction2D)allDirs[i]);
            }
        }

        return validDirs;
    }
    
    bool NeighborExists(HashSet<Vector2Int> validLayerCells, Vector2Int coord, Direction2D dir) {
        return validLayerCells.Contains(GetNeighbor(coord, dir));
    }

    Vector2Int GetNeighbor(Vector2Int coord, Direction2D dir) {
        return dir switch {
            Direction2D.North => coord + Vector2Int.up,
            Direction2D.East => coord + Vector2Int.right,
            Direction2D.South => coord + Vector2Int.down,
            Direction2D.West => coord + Vector2Int.left,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}
