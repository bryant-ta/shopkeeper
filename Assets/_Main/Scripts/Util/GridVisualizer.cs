using System;
using Unity.Collections;
using UnityEngine;

public class GridVisualizer : MonoBehaviour {
    [SerializeField] Grid grid;

    public Vector2 offset = new Vector2(0.5f, 0.5f);
    [ReadOnly] public Vector3 minBounds = new Vector3(-10f, 0f, -10f);
    [ReadOnly] public Vector3 maxBounds = new Vector3(10f, 0f, 10f);

    bool isEnabled = true;

    void OnEnable() { isEnabled = true; }
    void OnDisable() { isEnabled = false; }

    void OnValidate() {
        minBounds = new Vector3(-(int) (grid.Length / 2)-1, 0, -(int) (grid.Width / 2)-1);
        maxBounds = new Vector3((int) (grid.Length / 2), 0, (int) (grid.Width / 2));
    }
}