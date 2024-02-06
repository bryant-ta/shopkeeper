using System;
using System.Collections.Generic;
using UnityEngine;

public class Zone {
    public Vector3Int RootCoord { get; private set; }
    public int Length { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }

    public ZoneProperties ZoneProps;

    public HashSet<Vector3Int> AllCoords { get; private set; }
    public HashSet<Vector2Int> XZCoords { get; private set; }

    public Zone(Vector3Int rootCoord, int length, int height, int width, ZoneProperties zoneProps) {
        UpdateZonePosition(rootCoord, length, height, width);

        ZoneProps = zoneProps;
    }

    public void UpdateZonePosition(Vector3Int rootCoord, int length, int height, int width) {
        RootCoord = rootCoord;
        Length = length;
        Height = height;
        Width = width;
        
        UpdateAllCoords();
        UpdateXZCoords();
    }

    // Updates local coords of all cells belonging to this zone, according to root coord
    void UpdateAllCoords() {
        HashSet<Vector3Int> allCoords = new();

        for (int x = 0; x < Length; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Width; z++) {
                    allCoords.Add(RootCoord + new Vector3Int(x, y, z));
                }
            }
        }

        AllCoords = allCoords;
    }

    // Updates local xz coords of floor cells belonging to this zone, according to root coord
    void UpdateXZCoords() {
        HashSet<Vector2Int> xzCoords = new();

        for (int x = 0; x < Length; x++) {
            for (int y = 0; y < Height; y++) {
                for (int z = 0; z < Width; z++) {
                    xzCoords.Add(new Vector2Int(RootCoord.x, RootCoord.z) + new Vector2Int(x, z));
                }
            }
        }

        XZCoords = xzCoords;
    }
}

public struct ZoneProperties {
    public bool CanPlace;
    public bool CanTake;
}