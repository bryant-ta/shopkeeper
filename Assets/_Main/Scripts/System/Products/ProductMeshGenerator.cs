using System.Collections.Generic;
using UnityEngine;

public class ProductMeshGenerator {
    public static Mesh GenerateProductMesh(List<Vector3Int> shapeData, Vector3Int cellSize) {
        Mesh mesh = new Mesh();
        if (cellSize.x <= 0 || cellSize.y <= 0 || cellSize.z <= 0) {
            Debug.LogError("Cell size is negative.");
            return mesh;
        }

        // Gather unique vertices based on occupied cells
        HashSet<Vector3> vertices = new HashSet<Vector3>();
        foreach (Vector3Int offset in shapeData) {
            Vector3 coord = new Vector3(offset.x, offset.y, offset.z);
            for (int x = 0; x <= 1; x++) {
                for (int y = 0; y <= 1; y++) {
                    for (int z = 0; z <= 1; z++) {
                        Vector3 vertex = coord + new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z);
                        vertices.Add(vertex);
                    }
                }
            }
        }

        // Convert hashset to list for easier processing
        List<Vector3> verticesList = new List<Vector3>(vertices);

        // Generate triangles based on faces (assuming clockwise winding order)
        List<int> triangles = new List<int>();
        foreach (Vector3Int offset in shapeData) {
            Vector3 bottomLeft = new Vector3(offset.x, offset.y, offset.z);
            Vector3 bottomRight = new Vector3(offset.x + cellSize.x, offset.y, offset.z);
            Vector3 topLeft = new Vector3(offset.x, offset.y + cellSize.y, offset.z);
            Vector3 topRight = new Vector3(offset.x + cellSize.x, offset.y + cellSize.y, offset.z);
            Vector3 frontBottomLeft = bottomLeft + new Vector3(0, 0, cellSize.z);
            Vector3 frontBottomRight = bottomRight + new Vector3(0, 0, cellSize.z);
            Vector3 frontTopLeft = topLeft + new Vector3(0, 0, cellSize.z);
            Vector3 frontTopRight = topRight + new Vector3(0, 0, cellSize.z);

            // Bottom face
            int bottomLeftIndex = verticesList.IndexOf(bottomLeft);
            int bottomRightIndex = verticesList.IndexOf(bottomRight);
            int topLeftIndex = verticesList.IndexOf(topLeft);
            int topRightIndex = verticesList.IndexOf(topRight);
            triangles.AddRange(new int[] {bottomLeftIndex, bottomRightIndex, topLeftIndex, topRightIndex, bottomLeftIndex, topLeftIndex});

            // Front face (repeat for other faces as needed)
            int frontBottomLeftIndex = verticesList.IndexOf(frontBottomLeft);
            int frontBottomRightIndex = verticesList.IndexOf(frontBottomRight);
            int frontTopLeftIndex = verticesList.IndexOf(frontTopLeft);
            int frontTopRightIndex = verticesList.IndexOf(frontTopRight);
            triangles.AddRange(
                new int[] {
                    // frontBottomLeftIndex, frontLeftIndex, frontTopLeftIndex, frontTopRightIndex, frontBottomRightIndex, frontBottomLeftIndex
                }
            );
        }

        mesh.vertices = verticesList.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}