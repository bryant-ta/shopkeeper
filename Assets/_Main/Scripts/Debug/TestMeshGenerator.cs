using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TestMeshGenerator : MonoBehaviour {
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start() {
        MakeMeshData();
        CreateMesh();
    }

    void MakeMeshData() {
        vertices = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0)};
        triangles = new[] {0, 1, 2};
    }

    void CreateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

    }
}
