using System;
using System.Collections.Generic;
using UnityEngine;

// DON'T CHANGE ORDER!
public enum Direction {
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    Up = 4,
    Down = 5
}

// DON'T YOU DARE!
public enum Direction2D {
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class DirectionData {
    // All these arrays must match order of Direction enum
    // DON'T CHANGE ORDER!
    public static readonly Vector3[] DirectionVectors = {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left,
        Vector3.up,
        Vector3.down,
    };

    public static readonly Vector3Int[] DirectionVectorsInt = {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
    };

    public static Direction GetClosestDirection(Vector3 inputVector) {
        float maxDot = float.MinValue;
        int bestMatchIndex = 0;

        for (int i = 0; i < DirectionVectorsInt.Length; i++) {
            float dotProduct = Vector3.Dot(inputVector.normalized, DirectionVectorsInt[i]);
            if (dotProduct > maxDot) {
                maxDot = dotProduct;
                bestMatchIndex = i;
            }
        }

        return (Direction) bestMatchIndex;
    }

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

    public static List<Direction2D> OrthogonalDirection(Direction2D dir) {
        switch (dir) {
            case Direction2D.North:
            case Direction2D.South:
                return new List<Direction2D>() {Direction2D.East, Direction2D.West};
            case Direction2D.East:
            case Direction2D.West:
                return new List<Direction2D>() {Direction2D.North, Direction2D.South};
            default:
                Debug.LogError("Invalid direction.");
                return new List<Direction2D>() {Direction2D.East, Direction2D.West};
        }
    }
}

public static class CubeMeshData {
    // All these arrays must match order of Direction enum
    // DON'T CHANGE ORDER!
    public static readonly Vector3[] vertices = {
        new Vector3(1, 1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1),
        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, -1)
    };

    public static readonly int[][] edges = {
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

    #region Faces

    static readonly int[][] faceTriangles = {
        new int[] {0, 1, 2, 3}, // N
        new int[] {5, 0, 3, 6}, // E
        new int[] {4, 5, 6, 7}, // S
        new int[] {1, 4, 7, 2}, // W
        new int[] {5, 4, 1, 0}, // U
        new int[] {3, 2, 7, 6}, // D
    };

    static Vector3[] CubeFaceVertices(int dir, Vector3 pos, float scale, float bevel) {
        Vector3[] fv = new Vector3[4];
        for (int i = 0; i < fv.Length; i++) {
            fv[i] = pos + (vertices[faceTriangles[dir][i]] * scale * (1 - bevel)) + (DirectionData.DirectionVectors[dir] * scale * bevel);
        }

        return fv;
    }
    public static Vector3[] CubeFaceVertices(Direction dir, Vector3 pos, float scale, float bevel) {
        return CubeFaceVertices((int) dir, pos, scale, bevel);
    }

    #endregion

    // dir1,dir2 defines corner that bevel exists, going right to left as viewed from inside the mesh
    // (i.e. NW, EN, SE, WS)

    #region Bevel Edges

    public static Vector3[] CornerBevelFaceVertices(Direction dir1, Direction dir2, Vector3 pos, float scale, float bevel) {
        int d1 = (int) dir1;
        int d2 = (int) dir2;
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;

        int i = dir1 switch {
            Direction.Up => d1 + d2,
            Direction.Down => d1 + d2 + 3,
            _ => d1
        };

        Vector3[] fv = new Vector3[4];
        fv[0] = pos + (vertices[edges[i][0]] * innerCubeScale) + bevelVector1;
        fv[1] = pos + (vertices[edges[i][0]] * innerCubeScale) + bevelVector2;
        fv[2] = pos + (vertices[edges[i][1]] * innerCubeScale) + bevelVector2;
        fv[3] = pos + (vertices[edges[i][1]] * innerCubeScale) + bevelVector1;

        return fv;
    }

    public static Vector3[] ElbowBevelFaceVertices(Direction dir1, Direction dir2, Vector3 pos, float scale, float bevel) {
        int d1 = (int) dir1;
        int d2 = (int) dir2;
        float bevelOffset = scale * bevel; // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;

        Vector3[] fv = CornerBevelFaceVertices(dir1, dir2, pos, scale, bevel);
        for (int i = 0; i < fv.Length; i++) {
            fv[i] = fv[i] + bevelVector1 + bevelVector2;
        }

        return fv;
    }

    public static Vector3[] FlatBevelFaceVertices(Direction dir1, Direction dir2, Vector3 pos, float scale, float bevel) {
        int d1 = (int) dir1;
        int d2 = (int) dir2;
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;

        int i = dir1 switch {
            Direction.Up => d1 + d2,
            Direction.Down => d1 + d2 + 3,
            _ => d1
        };

        Vector3[] fv = new Vector3[4];
        if (dir1 == Direction.Up || dir1 == Direction.Down) {
            fv[0] = pos + (vertices[edges[i][0]] * innerCubeScale) + bevelVector1;
            fv[1] = pos + (vertices[edges[i][0]] * innerCubeScale) + bevelVector1 + 2 * bevelVector2;
            fv[2] = pos + (vertices[edges[i][1]] * innerCubeScale) + bevelVector1 + 2 * bevelVector2;
            fv[3] = pos + (vertices[edges[i][1]] * innerCubeScale) + bevelVector1;
        } else {
            fv[0] = pos + (vertices[edges[i][0]] * innerCubeScale) + 2 * bevelVector1 + bevelVector2;
            fv[1] = pos + (vertices[edges[i][0]] * innerCubeScale) + bevelVector2;
            fv[2] = pos + (vertices[edges[i][1]] * innerCubeScale) + bevelVector2;
            fv[3] = pos + (vertices[edges[i][1]] * innerCubeScale) + 2 * bevelVector1 + bevelVector2;
        }

        return fv;
    }

    #endregion

    #region Bevel Caps

    public static readonly Direction[][] CapVerticeDirectionVectors = {
        new[] {Direction.Up, Direction.North, Direction.East},
        new[] {Direction.Up, Direction.West, Direction.North},
        new[] {Direction.Down, Direction.North, Direction.West},
        new[] {Direction.Down, Direction.East, Direction.North},
        new[] {Direction.Up, Direction.South, Direction.West},
        new[] {Direction.Up, Direction.East, Direction.South},
        new[] {Direction.Down, Direction.South, Direction.East},
        new[] {Direction.Down, Direction.West, Direction.South},
    };

    public static Vector3[] CornerCapBevelFaceVertices(int vertice, Vector3 pos, float scale, float bevel) {
        int d1 = (int) CapVerticeDirectionVectors[vertice][0];
        int d2 = (int) CapVerticeDirectionVectors[vertice][1];
        int d3 = (int) CapVerticeDirectionVectors[vertice][2];
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;
        Vector3 bevelVector3 = DirectionData.DirectionVectors[d3] * bevelOffset;

        Vector3[] fv = new Vector3[3];
        fv[0] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1;
        fv[1] = pos + (vertices[vertice] * innerCubeScale) + bevelVector2;
        fv[2] = pos + (vertices[vertice] * innerCubeScale) + bevelVector3;

        return fv;
    }

    public static Vector3[] ElbowCapBevelFaceVertices(int vertice, Vector3 pos, float scale, float bevel) {
        int d1 = (int) CapVerticeDirectionVectors[vertice][0];
        int d2 = (int) CapVerticeDirectionVectors[vertice][1];
        int d3 = (int) CapVerticeDirectionVectors[vertice][2];
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;
        Vector3 bevelVector3 = DirectionData.DirectionVectors[d3] * bevelOffset;

        Vector3[] fv = new Vector3[7];
        fv[0] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1;
        fv[1] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector2 + bevelVector1;
        fv[2] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector3 + bevelVector1;
        fv[3] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector3 + bevelVector1;
        fv[4] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector2 + bevelVector1;
        fv[5] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector2 + bevelVector3;
        fv[6] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector3 + bevelVector2;

        return fv;
    }

    public static Vector3[] FlatCapBevelFaceVertices(int vertice, Vector3 pos, float scale, float bevel) {
        int d1 = (int) CapVerticeDirectionVectors[vertice][0];
        int d2 = (int) CapVerticeDirectionVectors[vertice][1];
        int d3 = (int) CapVerticeDirectionVectors[vertice][2];
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;
        Vector3 bevelVector3 = DirectionData.DirectionVectors[d3] * bevelOffset;

        Vector3[] fv = new Vector3[4];
        fv[0] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector2 + bevelVector1;
        fv[1] = pos + (vertices[vertice] * innerCubeScale) + 2 * bevelVector2 + bevelVector3;
        fv[2] = pos + (vertices[vertice] * innerCubeScale) + bevelVector3;
        fv[3] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1;

        return fv;
    }

    // the little flat square corner pieces on top/bottom faces
    public static Vector3[] FlatCapTopFaceVertices(int vertice, Vector3 pos, float scale, float bevel) {
        int d1 = (int) CapVerticeDirectionVectors[vertice][0];
        int d2 = (int) CapVerticeDirectionVectors[vertice][1];
        int d3 = (int) CapVerticeDirectionVectors[vertice][2];
        float innerCubeScale = scale * (1 - bevel); // inner cube vertice
        float bevelOffset = scale * bevel;          // offset from inner cube vertice

        Vector3 bevelVector1 = DirectionData.DirectionVectors[d1] * bevelOffset;
        Vector3 bevelVector2 = DirectionData.DirectionVectors[d2] * bevelOffset;
        Vector3 bevelVector3 = DirectionData.DirectionVectors[d3] * bevelOffset;

        Vector3[] fv = new Vector3[4];
        fv[0] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1 + bevelVector2;
        fv[1] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1 + bevelVector2 + bevelVector3;
        fv[2] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1 + bevelVector3;
        fv[3] = pos + (vertices[vertice] * innerCubeScale) + bevelVector1;

        return fv;
    }

    #endregion
}