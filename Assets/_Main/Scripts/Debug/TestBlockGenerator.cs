using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestBlockGenerator : MonoBehaviour {
    public int xSize, ySize, zSize; // cell size
    public int height = 1;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void Awake() {
        ShapeData shapeData;
        shapeData = new ShapeData() {
            ShapeOffsets = new List<Vector3Int>() {
                new(0, 0, 0)
            }
        };

        Generate(shapeData.ShapeOffsets);
    }

    void Generate(List<Vector3Int> shapeOffsets) {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Shape";
        CreateVertices(shapeOffsets, out List<Vector3> vertsList);
        CreateTriangles(shapeOffsets, vertsList);
    }

    void CreateVertices(List<Vector3Int> shapeOffsets, out List<Vector3> vertsList) {
        HashSet<Vector3> botFaceVertsSet = new();

        // add verts of bot face using shape data
        for (var i = 0; i < shapeOffsets.Count; i++) {
            Vector3Int offset = shapeOffsets[i];

            botFaceVertsSet.Add(offset);
            botFaceVertsSet.Add(offset + new Vector3Int(xSize, 0, 0));
            botFaceVertsSet.Add(offset + new Vector3Int(0, 0, zSize));
            botFaceVertsSet.Add(offset + new Vector3Int(xSize, 0, zSize));
        }

        vertsList = new List<Vector3>(botFaceVertsSet);

        // duplicate bot face to add top face
        vertsList.AddRange(botFaceVertsSet.Select(v => new Vector3(v.x, height, v.z)));

        // duplicate bot face outline to reach height
        if (height > 1) {
            List<Vector3> botFaceOutlineVerts = RemoveInnerVertices(botFaceVertsSet).ToList();
            for (int y = 1; y < height; y++) {
                for (int i = 0; i < botFaceOutlineVerts.Count; i++) {
                    vertsList.Add(new Vector3(botFaceOutlineVerts[i].x, y, botFaceOutlineVerts[i].z));
                }
            }
        }

        // TODO: sort by x -> z, -> y increasing
        // prob not needed bc using indexOf for triangles...
        // verticesList = verticesList.OrderBy(v => v.x).ThenBy(v => v.z).ThenBy(v => v.y).ToList();

        mesh.vertices = vertsList.ToArray();
    }

    void CreateTriangles(List<Vector3Int> shapeOffsets, List<Vector3> vertsList) {
        List<int> trianglesList = new();

        // add triangles of bot face
        for (var i = 0; i < shapeOffsets.Count; i++) {
            Vector3Int offset = shapeOffsets[i];

            int v00Index = vertsList.IndexOf(offset);
            int v01Index = vertsList.IndexOf(offset + new Vector3Int(0, 0, zSize));
            int v10Index = vertsList.IndexOf(offset + new Vector3Int(xSize, 0, 0));
            int v11Index = vertsList.IndexOf(offset + new Vector3Int(xSize, 0, zSize));

            trianglesList.Add(v00Index);
            trianglesList.Add(v10Index);
            trianglesList.Add(v01Index);

            trianglesList.Add(v10Index);
            trianglesList.Add(v11Index);
            trianglesList.Add(v01Index);
        }

        // add triangles of top face
        for (var i = 0; i<shapeOffsets.Count; i++) {
            Vector3Int offset = shapeOffsets[i];

            int v00Index = vertsList.IndexOf(offset);
            int v01Index = vertsList.IndexOf(offset + new Vector3Int(0, height, zSize));
            int v10Index = vertsList.IndexOf(offset + new Vector3Int(xSize, height, 0));
            int v11Index = vertsList.IndexOf(offset + new Vector3Int(xSize, height, zSize));

            trianglesList.Add(v00Index);
            trianglesList.Add(v01Index);
            trianglesList.Add(v10Index);

            trianglesList.Add(v10Index);
            trianglesList.Add(v01Index);
            trianglesList.Add(v11Index);
        }
        
        // add triangles of wall faces
        

        mesh.triangles = trianglesList.ToArray();
        mesh.RecalculateNormals();
    }

    #region Helper

    static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11) {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    // test each vertex: if 4 neighbors can be found in vertices, add to list of "inner vertices", to be removed at end
    static HashSet<Vector3> RemoveInnerVertices(HashSet<Vector3> verts) {
        HashSet<Vector3> outlineVerts = new();

        foreach (Vector3 v in verts) {
            int neighborCount = 0;

            // Check for neighbor vertices
            for (int dx = -1; dx <= 1; dx++) {
                for (int dz = -1; dz <= 1; dz++) {
                    if (dx == 0 && dz == 0) continue;

                    Vector3 neighborV = v + new Vector3(dx, 0, dz);
                    if (verts.Contains(neighborV)) {
                        neighborCount++;
                    }
                }
            }

            // If vertex has less than 4 neighbors, it's on the outline
            if (neighborCount < 4) {
                outlineVerts.Add(v);
            }
        }

        return outlineVerts;
    }

    #endregion
}