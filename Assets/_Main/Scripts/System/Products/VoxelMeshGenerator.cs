using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class VoxelMeshGenerator {
    static float scale = 0.5f;
    [Range(0f, 1f)] static float bevel = 0.2f;

    static List<Vector3> vertices = new();
    static List<int> triangles = new();

    /// <summary>
    /// Generates and sets mesh/colliders for voxel objects described by ShapeData. 
    /// </summary>
    public static void Generate(GameObject targetObj, ShapeData shapeData) {
        vertices.Clear();
        triangles.Clear();
        lastVCount = -1;

        Mesh mesh = targetObj.GetComponent<MeshFilter>().mesh;
        if (mesh == null) {
            Debug.LogError("targetObj should have a mesh filter component.");
        }

        mesh.Clear();
        mesh.name = "Voxel";

        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            MakeCube(shapeData, offset);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MakeVoxelCollider(targetObj, shapeData);
    }

    static void MakeCube(ShapeData shapeData, Vector3Int cubeCoord) {
        // faces
        for (int i = 0; i < 6; i++) { // must match Direction enum
            if (!shapeData.NeighborExists(cubeCoord, (Direction) i)) {
                MakeFace((Direction) i, cubeCoord);
            }
        }

        // side bevels
        for (int d1 = 0; d1 < 4; d1++) {
            int d2 = d1 - 1;
            if (d2 < 0) d2 = 3;
            MakeBevel(shapeData, (Direction) d1, (Direction) d2, cubeCoord);
        }

        // top/bot bevels 4,0 4,1 4,2 4,3
        for (int d2 = 0; d2 < 4; d2++) {
            MakeBevel(shapeData, Direction.Up, (Direction) d2, cubeCoord);
            MakeBevel(shapeData, Direction.Down, (Direction) d2, cubeCoord);
        }
    }

    static void MakeFace(Direction dir, Vector3Int cubeCoord) {
        vertices.AddRange(CubeMeshData.CreateCubeFaceVertices(dir, cubeCoord, scale, bevel));
        SetQuad();
    }

    static void MakeBevel(ShapeData shapeData, Direction dir1, Direction dir2, Vector3Int cubeCoord) {
        if (shapeData.NeighborExists(cubeCoord, dir1)
            && !shapeData.NeighborExists(cubeCoord, dir2)
            && !shapeData.NeighborExists(cubeCoord + CubeMeshData.DirectionVectorsInt[(int)dir1], dir2)) { // side flat
            vertices.AddRange(CubeMeshData.CreateFlatBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        }
        else if (shapeData.NeighborExists(cubeCoord, dir1)
                 && shapeData.NeighborExists(cubeCoord, dir2)
                 && !shapeData.NeighborExists(cubeCoord + CubeMeshData.DirectionVectorsInt[(int)dir1], dir2)) { // side flat
            vertices.AddRange(CubeMeshData.CreateElbowBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        }
        else if (!shapeData.NeighborExists(cubeCoord, dir1) && !shapeData.NeighborExists(cubeCoord, dir2)) { // side/top/bot corner
            vertices.AddRange(CubeMeshData.CreateCornerBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        }
        else if ((dir1 == Direction.Up || dir1 == Direction.Down) && shapeData.NeighborExists(cubeCoord, dir2)) { // top/bot flat
            vertices.AddRange(CubeMeshData.CreateFlatBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));

        }
        else {
            // vertices.AddRange(CubeMeshData.CreateCornerBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
            return;
        }

        SetQuad();
    }

    // Call once after adding vertices of a quad to vertices array. Expects verticies added from top right to bottom left (ccw)
    static int lastVCount;
    static void SetQuad() {
        int vCount = vertices.Count;
        if (lastVCount == vCount) {
            Debug.LogError("Unexpected attempt to set quad: no new verticies added.");
            return;
        }

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }

    static void MakeVoxelCollider(GameObject targetObj, ShapeData shapeData) {
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            BoxCollider bc = targetObj.AddComponent<BoxCollider>();
            bc.center = offset;
            bc.size = Vector3.one * scale * 2;
        }
    }
}