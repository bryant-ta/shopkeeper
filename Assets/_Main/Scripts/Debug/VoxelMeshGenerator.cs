using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelMeshGenerator : MonoBehaviour {
    public float scale;
    
    Mesh mesh;

    List<Vector3> vertices = new();
    List<int> triangles = new();

    void Awake() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Voxel";
    }

    void Start() {
        ShapeData shapeData = new ShapeData() {
            ShapeOffsets = new List<Vector3Int>() {
                new(0, 0, 0),
                new(1, 0, 0),
                new(0, 0, 1),
                new(0, 0, 2),
                new(0, 0, 3),
            }
        };
        
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            MakeCube(shapeData, offset);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void MakeCube(ShapeData shapeData, Vector3Int coord) {
        for (int i = 0; i < 6; i++) { // must match Direction enum
            if (!shapeData.NeighborExists(coord, (Direction) i)) {
                MakeFace((Direction) i, coord);
            }
        }
    }

    void MakeFace(Direction dir, Vector3 facePos) {
        vertices.AddRange(CubeMeshData.CreateCubeFaceVertices(dir, facePos, scale));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }
}