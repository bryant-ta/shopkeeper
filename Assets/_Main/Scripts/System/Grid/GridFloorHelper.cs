using System;
using UnityEngine;

public class GridFloorHelper : MonoBehaviour {
    [Tooltip("Reference to Grid that this plane supports.")]
    [field:SerializeField] public Grid Grid { get; private set; }
    
    Material gridLinesMat;
    float gridLinesMatOriginalFadeDistance;

    void Awake() {
        gridLinesMat = GetComponent<MeshRenderer>().materials[1];
    }
    
    public void SetGridLinesCursorPosition(Vector3 pos) {
        gridLinesMat.SetVector("_CursorHitPosition", pos);
    }

    public void SetGridLinesFade(float fadeDistance) {
        gridLinesMat.SetFloat("_FadeDistance", fadeDistance);
    }
    public void ResetGridLinesFade() {
        gridLinesMat.SetFloat("_FadeDistance", gridLinesMatOriginalFadeDistance);
    }
    
}
