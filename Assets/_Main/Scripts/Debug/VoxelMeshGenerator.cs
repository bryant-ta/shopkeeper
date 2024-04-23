using System.Collections.Generic;
using UnityEngine;

public static class VoxelMeshGenerator {
    static float scale = 0.5f;
    
    static List<Vector3> vertices = new();
    static List<int> triangles = new();

    /// <summary>
    /// Generates and sets mesh/colliders for voxel objects described by ShapeData. 
    /// </summary>
    public static void Generate(GameObject targetObj, ShapeData shapeData) {
        vertices.Clear();
        triangles.Clear();
        
        Mesh mesh = targetObj.GetComponent<MeshFilter>().mesh;
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

    static void MakeCube(ShapeData shapeData, Vector3Int coord) {
        for (int i = 0; i < 6; i++) { // must match Direction enum
            if (!shapeData.NeighborExists(coord, (Direction) i)) {
                MakeFace((Direction) i, coord);
            }
        }
    }

    static void MakeFace(Direction dir, Vector3 facePos) {
        vertices.AddRange(CubeMeshData.CreateCubeFaceVertices(dir, facePos, scale));
        int vCount = vertices.Count;

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