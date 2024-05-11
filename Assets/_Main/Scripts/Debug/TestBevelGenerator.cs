using System;
using System.Collections.Generic;
using UnityEngine;

public class TestBevelGenerator : MonoBehaviour
{
    void Start() {
        ShapeData shapeData = new ShapeData() {
            ID = ShapeDataID.I2,
            ShapeOffsets = new List<Vector3Int>() {
                new(0, 0, 0),
                new(1, 0, 0),
                new(0, 0, 1),
            }
        };
        
        VoxelMeshGenerator.Generate(gameObject, shapeData);
    }
}
