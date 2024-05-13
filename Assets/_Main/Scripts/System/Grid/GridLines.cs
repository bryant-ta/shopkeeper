using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GridLines : MonoBehaviour {
    [SerializeField] float gridLinesFadeDistance;
    
    Material gridLinesMat;
    float gridLinesMatOriginalFadeDistance;

    void Awake() {
        gridLinesMat = GetComponent<MeshRenderer>().materials[1];

        Ref.Player.PlayerDrag.OnGrab += SetGridLinesFade;
        Ref.Player.PlayerDrag.OnDrag += SetGridLinesCursorPosition;
        Ref.Player.PlayerDrag.OnRelease += ResetGridLinesFade;
    }
    
    public void SetGridLinesCursorPosition(Vector3 pos) {
        gridLinesMat.SetVector("_CursorHitPosition", pos);
    }

    public void SetGridLinesFade(Vector3 pos) {
        gridLinesMat.SetFloat("_FadeDistance", gridLinesFadeDistance);
        SetGridLinesCursorPosition(pos);
    }
    public void ResetGridLinesFade() {
        gridLinesMat.SetFloat("_FadeDistance", 0);
    }
}
