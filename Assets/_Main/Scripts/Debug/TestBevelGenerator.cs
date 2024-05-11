using System.Collections.Generic;
using UnityEngine;

public class TestBevelGenerator : MonoBehaviour {
    [SerializeField] GameObject baseObj;

    void Start() {
        ShapeData[] shapeDatas = new ShapeData[] {
            new ShapeData() {
                ID = ShapeDataID.I2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(1, 0, 1),
                }
            },
            new ShapeData() {
                ID = ShapeDataID.I2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(-1, 0, 0),
                    new(0, 0, 1),
                    new(0, 0, -1),
                }
            },
            new ShapeData() {
                ID = ShapeDataID.I2,
                ShapeOffsets = new List<Vector3Int>() {
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(2, 0, 0),
                }
            }
        };


        GameObject a = Instantiate(baseObj, transform.position + Vector3.zero, Quaternion.identity);
        VoxelMeshGenerator.Generate(a, shapeDatas[0]);
        
        GameObject b = Instantiate(baseObj, transform.position + Vector3.forward * 4, Quaternion.identity);
        VoxelMeshGenerator.Generate(b, shapeDatas[1]);
        
        GameObject c = Instantiate(baseObj, transform.position + Vector3.right * 4, Quaternion.identity);
        VoxelMeshGenerator.Generate(c, shapeDatas[2]);
    }
}