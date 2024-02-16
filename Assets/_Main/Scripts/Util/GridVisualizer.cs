using System;
using UnityEngine;

public class GridVisualizer : MonoBehaviour {
    public Vector3 minBounds = new Vector3(-10f, 0f, -10f);
    public Vector3 maxBounds = new Vector3(10f, 0f, 10f);
    public Vector2 offset = new Vector2(0.5f, 0.5f);

    bool isEnabled = true;

    void OnEnable() { isEnabled = true; }
    void OnDisable() { isEnabled = false; }
}