using System.Collections.Generic;
using UnityEngine;

public static class VoxelMeshGenerator {
    static float scale = 0.5f;
    [Range(0f, 1f)] static float bevel = 0.2f;

    static List<Vector3> vertices = new();
    static List<int> triangles = new();

    /// <summary>
    /// Generates and sets mesh/colliders for voxel objects described by ShapeData. 
    /// </summary>
    public static void Generate(GameObject targetObj, ShapeData shapeData, bool generateColliders = true) {
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

        mesh.tangents = CalculateTangentsFromNormals(mesh.normals);

        // Outline shader uses smooth normals
        mesh.SetUVs(7, SmoothNormals(mesh.normals));

        if (generateColliders) {
            MakeVoxelCollider(targetObj, shapeData);
        }
    }

    static void MakeCube(ShapeData shapeData, Vector3Int cubeCoord) {
        // faces
        for (int i = 0; i < 6; i++) { // must match Direction enum
            if (!shapeData.NeighborExists(cubeCoord, (Direction) i)) {
                MakeFace((Direction) i, cubeCoord);
            }
        }

        // side edge bevels
        for (int d1 = 0; d1 < 4; d1++) {
            int d2 = d1 - 1;
            if (d2 < 0) d2 = 3;
            MakeEdgeBevel(shapeData, (Direction) d1, (Direction) d2, cubeCoord);
        }

        // top/bot edge bevels
        for (int d2 = 0; d2 < 4; d2++) {
            MakeEdgeBevel(shapeData, Direction.Up, (Direction) d2, cubeCoord);
            MakeEdgeBevel(shapeData, Direction.Down, (Direction) d2, cubeCoord);
        }

        // cap bevels
        for (int i = 0; i < 8; i++) {
            MakeCapBevel(shapeData, i, cubeCoord);
        }
    }

    static void MakeFace(Direction dir, Vector3Int cubeCoord) {
        vertices.AddRange(CubeMeshData.CubeFaceVertices(dir, cubeCoord, scale, bevel));
        SetQuad();
    }

    static void MakeEdgeBevel(ShapeData shapeData, Direction dir1, Direction dir2, Vector3Int cubeCoord) {
        if (shapeData.NeighborExists(cubeCoord, dir1)
            && !shapeData.NeighborExists(cubeCoord, dir2)
            && !shapeData.NeighborExists(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) { // side flat
            vertices.AddRange(CubeMeshData.FlatBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        } else if (shapeData.NeighborExists(cubeCoord, dir1)
                   && shapeData.NeighborExists(cubeCoord, dir2)
                   && !shapeData.NeighborExists(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) { // side elbow
            vertices.AddRange(CubeMeshData.ElbowBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        } else if (!shapeData.NeighborExists(cubeCoord, dir1) && !shapeData.NeighborExists(cubeCoord, dir2)) { // side/top/bot corner
            vertices.AddRange(CubeMeshData.CornerBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        } else if ((dir1 == Direction.Up || dir1 == Direction.Down) && shapeData.NeighborExists(cubeCoord, dir2)) { // top/bot flat
            vertices.AddRange(CubeMeshData.FlatBevelFaceVertices(dir1, dir2, cubeCoord, scale, bevel));
        } else {
            return;
        }

        SetQuad();
    }

    static void MakeCapBevel(ShapeData shapeData, int vertice, Vector3Int cubeCoord) {
        Direction dir0 = CubeMeshData.CapVerticeDirectionVectors[vertice][0];
        Direction dir1 = CubeMeshData.CapVerticeDirectionVectors[vertice][1];
        Direction dir2 = CubeMeshData.CapVerticeDirectionVectors[vertice][2];
        if (shapeData.NeighborExists(cubeCoord, dir1)
            && !shapeData.NeighborExists(cubeCoord, dir2)
            && !shapeData.NeighborExists(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) { // side flat
            vertices.AddRange(CubeMeshData.FlatCapBevelFaceVertices(vertice, cubeCoord, scale, bevel));
        } else if (shapeData.NeighborExists(cubeCoord, dir1)
                   && shapeData.NeighborExists(cubeCoord, dir2)
                   && !shapeData.NeighborExists(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) { // side elbow
            Vector3[] r = CubeMeshData.ElbowCapBevelFaceVertices(vertice, cubeCoord, scale, bevel);

            vertices.Add(r[0]);
            vertices.Add(r[1]);
            vertices.Add(r[2]);
            SetTriangle();

            vertices.Add(r[3]);
            vertices.Add(r[4]);
            vertices.Add(r[5]);
            vertices.Add(r[6]);
            SetQuad();

            return;
        } else if (!shapeData.NeighborExists(cubeCoord, dir1) && !shapeData.NeighborExists(cubeCoord, dir2)) { // side corner
            vertices.AddRange(CubeMeshData.CornerCapBevelFaceVertices(vertice, cubeCoord, scale, bevel));
            SetTriangle();
            return;
        } else if ((dir0 == Direction.Up || dir0 == Direction.Down)
                   && shapeData.NeighborExists(cubeCoord, dir1)
                   && shapeData.NeighborExists(cubeCoord, dir2)
                   && shapeData.NeighborExists(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) { // top/bot flat
            vertices.AddRange(CubeMeshData.FlatCapTopFaceVertices(vertice, cubeCoord, scale, bevel));
        } else {
            return;
        }

        SetQuad();
    }

    static Vector4[] CalculateTangentsFromNormals(Vector3[] normals) {
        Vector4[] tangents = new Vector4[vertices.Count];
        for (int i = 0; i < triangles.Count; i += 3) {
            for (int j = 0; j < 3; j++) {
                int index = triangles[i + j];
                Vector3 normal = normals[index];
                Vector3 tangent = TranslateNormalToTangent(normal);
                tangents[index] += new Vector4(tangent.x, tangent.y, tangent.z, 1).normalized;
            }
        }

        return tangents;
    }
    static Vector3 TranslateNormalToTangent(Vector3 normal) {
        Vector3 n = normal.normalized;
        if (n.y < 1 && n.y > -1) {
            return Quaternion.Euler(0f, -90f, 0f) * n;
        } else {
            return Quaternion.Euler(-90f, 0f, 0f) * n;
        }
    }

    static Vector3[] SmoothNormals(Vector3[] normals) {
        Vector3[] smoothedNormals = new Vector3[normals.Length];
        Dictionary<Vector3, Vector3> _smoothedNormals = new Dictionary<Vector3, Vector3>();

        for (int i = 0; i < vertices.Count; i++) {
            if (!_smoothedNormals.ContainsKey(vertices[i]))
                _smoothedNormals.Add(vertices[i], normals[i]);
            else {
                Vector3 value = Vector3.zero;
                _smoothedNormals.TryGetValue(vertices[i], out value);
                value += normals[i];
                _smoothedNormals.Remove(vertices[i]);
                _smoothedNormals.Add(vertices[i], value);
            }
        }

        for (int i = 0; i < vertices.Count; i++) {
            Vector3 n = Vector3.zero;
            _smoothedNormals.TryGetValue(vertices[i], out n);
            n.Normalize();

            smoothedNormals[i] = n;
        }

        return smoothedNormals;
    }

    // Call once after adding vertices of a quad to vertices array. Expects vertices added from top right to bottom left (ccw)
    static int lastVCount;
    static void SetQuad() {
        int vCount = vertices.Count;
        if (lastVCount == vCount) {
            Debug.LogError("Unexpected attempt to set quad: no new verticies added.");
            return;
        }

        lastVCount = vCount;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }
    // Call once after adding vertices of a triangle to vertices array. Expects vertices added ccw
    static void SetTriangle() {
        int vCount = vertices.Count;
        if (lastVCount == vCount) {
            Debug.LogError("Unexpected attempt to set quad: no new verticies added.");
            return;
        }

        lastVCount = vCount;

        triangles.Add(vCount - 3);
        triangles.Add(vCount - 3 + 1);
        triangles.Add(vCount - 3 + 2);
    }

    static void MakeVoxelCollider(GameObject targetObj, ShapeData shapeData) {
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            BoxCollider bc = targetObj.AddComponent<BoxCollider>();
            bc.center = offset;
            bc.size = Vector3.one * scale * 2;
        }
    }
}