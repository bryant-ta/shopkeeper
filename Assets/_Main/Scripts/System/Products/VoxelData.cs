using System;
using UnityEngine;

public enum Direction {
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    Up = 4,
    Down = 5
}

public enum Direction2D {
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class CubeMeshData {
    // All these arrays must match order of Direction enum
    // DON'T CHANGE ORDER!
    static Vector3[] vertices = {
        new Vector3(1, 1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1),
        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, -1)
    };

    static int[][] faceTriangles = {
        new int[] {0, 1, 2, 3}, // N
        new int[] {5, 0, 3, 6}, // E
        new int[] {4, 5, 6, 7}, // S
        new int[] {1, 4, 7, 2}, // W
        new int[] {5, 4, 1, 0}, // U
        new int[] {3, 2, 7, 6}, // D
    };

    static int[][] bevelEdges = {
        new int[] {1, 2}, // NW
        new int[] {0, 3}, // EN
        new int[] {5, 6}, // SE
        new int[] {4, 7}, // WS
        new int[] {1, 0}, // UN
        new int[] {0, 5}, // UE
        new int[] {5, 4}, // US
        new int[] {4, 1}, // UW
        new int[] {3, 2}, // DN
        new int[] {6, 3}, // DE
        new int[] {7, 6}, // DS
        new int[] {2, 7}, // DW
    };

    static Vector3[] directionVectors = {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left,
        Vector3.up,
        Vector3.down,
    };

    public static Vector3[] CreateCubeFaceVertices(int dir, Vector3 pos, float scale, float bevel) {
        Vector3[] fv = new Vector3[4];
        for (int i = 0; i < fv.Length; i++) {
            fv[i] = pos + (vertices[faceTriangles[dir][i]] * scale * (1 - bevel)) + (directionVectors[dir] * scale * bevel);
        }

        return fv;
    }
    public static Vector3[] CreateCubeFaceVertices(Direction dir, Vector3 pos, float scale, float bevel) {
        return CreateCubeFaceVertices((int) dir, pos, scale, bevel);
    }

    // dir1,dir2 defines corner that bevel exists, going right to left as viewed from inside the mesh
    // (i.e. NW, EN, SE, WS)
    public static Vector3[] CreateBevelFaceVertices(Direction dir1, Direction dir2, Vector3 pos, float scale, float bevel) {
        int d1 = (int) dir1;
        int d2 = (int) dir2;
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        int i;
        switch (dir1) {
            case Direction.Up:
                i = d1 + d2;
                break;
            case Direction.Down:
                i = d1 + d2 + 3;
                break;
            default:
                i = d1;
                break;
        }
        
        Vector3[] fv = new Vector3[4];
        fv[0] = pos + (vertices[bevelEdges[i][0]] * innerCubeScale) + (directionVectors[d1] * bevelOffset);
        fv[1] = pos + (vertices[bevelEdges[i][0]] * innerCubeScale) + (directionVectors[d2] * bevelOffset);
        fv[2] = pos + (vertices[bevelEdges[i][1]] * innerCubeScale) + (directionVectors[d2] * bevelOffset);
        fv[3] = pos + (vertices[bevelEdges[i][1]] * innerCubeScale) + (directionVectors[d1] * bevelOffset);
        
        return fv;

        // TODO: add neightbor checking + 5 cases + bevel adjustment per case
    }

    #region Helper

    public static Direction OppositeDirection(Direction dir) {
        switch (dir) {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.South:
                return Direction.North;
            case Direction.West:
                return Direction.East;
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            default:
                Debug.LogError("Invalid direction.");
                return Direction.North;
        }
    }

    #endregion
}