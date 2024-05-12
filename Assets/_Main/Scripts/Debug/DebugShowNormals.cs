using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DebugShowNormals : MonoBehaviour
{
    public float lineLength = 0.1f; // Length of the normal lines

    void OnDrawGizmos()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            if (vertices != null && normals != null)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldVertex = transform.TransformPoint(vertices[i]);
                    Vector3 worldNormal = transform.TransformDirection(normals[i]);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(worldVertex, worldVertex + worldNormal * lineLength);
                }
            }
        }
    }
}