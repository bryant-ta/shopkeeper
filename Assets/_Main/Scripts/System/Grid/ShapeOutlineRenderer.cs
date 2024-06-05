using System.Collections.Generic;
using UnityEngine;

public class ShapeOutlineRenderer : MonoBehaviour {
    [SerializeField] float scale = 1f;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] Material solidMaterial;
    [SerializeField] Material dottedMaterial;

    List<LineRenderer> lineRenderers = new();

    Camera mainCamera;

    void Awake() { mainCamera = Camera.main; }

    public void Render(ShapeData shapeData) {
        // TODO: line renderer pooling instead of many instantiates
        // reset renderer
        for (int i = 0; i < lineRenderers.Count; i++) {
            Destroy(lineRenderers[i].gameObject);
        }
        lineRenderers.Clear();

        transform.localPosition = shapeData.RootCoord + new Vector3(0, 0.55f, 0); // should move this...
        
        foreach (Vector3Int offset in shapeData.ShapeOffsets) {
            MakeCubeOutline(shapeData, offset);
        }
    }

    void MakeCubeOutline(ShapeData shapeData, Vector3Int cubeCoord) {
        // side edges
        for (int d1 = 0; d1 < 4; d1++) {
            int d2 = d1 - 1;
            if (d2 < 0) d2 = 3;
            MakeEdgeLine(shapeData, (Direction) d1, (Direction) d2, cubeCoord);
        }

        // top/bot edges
        for (int d2 = 0; d2 < 4; d2++) {
            MakeEdgeLine(shapeData, Direction.Up, (Direction) d2, cubeCoord);
            MakeEdgeLine(shapeData, Direction.Down, (Direction) d2, cubeCoord);
        }
    }

    void MakeEdgeLine(ShapeData shapeData, Direction dir1, Direction dir2, Vector3Int cubeCoord) {
        if ((shapeData.ContainsDir(cubeCoord, dir1) // side elbow || side/top/bot corner
             && shapeData.ContainsDir(cubeCoord, dir2)
             && !shapeData.ContainsDir(cubeCoord + DirectionData.DirectionVectorsInt[(int) dir1], dir2)) ||
            (!shapeData.ContainsDir(cubeCoord, dir1) && !shapeData.ContainsDir(cubeCoord, dir2))) {
            int d1 = (int) dir1;
            int d2 = (int) dir2;
            int i = dir1 switch {
                Direction.Up => d1 + d2,
                Direction.Down => d1 + d2 + 3,
                _ => d1
            };

            float length = 0.5f * scale;
            Vector3 startPoint = cubeCoord + CubeMeshData.vertices[CubeMeshData.edges[i][0]] * length;
            Vector3 endPoint = cubeCoord + CubeMeshData.vertices[CubeMeshData.edges[i][1]] * length;

            GameObject lineObject = Instantiate(lineRendererPrefab, transform);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderers.Add(lineRenderer);
        }
    }

    // TODO: figure out better way to draw "back" lines as dotted
    // void Update() {
    //     UpdateLineMaterials();
    // }

    void UpdateLineMaterials() {
        // shift center of cube for dot test to avoid results close to 0 and capture "more" of the cube's edges
        Vector3 dotTestPoint = transform.position + mainCamera.transform.forward * 0.2f;
        foreach (LineRenderer lineRenderer in lineRenderers) {
            Vector3 startPoint = transform.position + lineRenderer.GetPosition(0);
            Vector3 endPoint = transform.position + lineRenderer.GetPosition(1);

            Vector3 midPoint = (startPoint + endPoint) / 2;
            Vector3 cubeToMidPoint = midPoint - dotTestPoint;

            // If dot product is positive, line is behind cube
            lineRenderer.material = Vector3.Dot(cubeToMidPoint, mainCamera.transform.forward) > 0 ? dottedMaterial : solidMaterial;
        }
    }
}