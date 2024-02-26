using UnityEngine;

public class CutoutEffect : MonoBehaviour {
    [SerializeField] Transform targetObject;
    [SerializeField] LayerMask wallMask;

    Camera mainCamera;

    void Awake() { mainCamera = GetComponent<Camera>(); }

    void Update() {
        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(targetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - transform.position;
        RaycastHit[] hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);

        for (int i = 0; i < hitObjects.Length; ++i) {
            Material[] materials = hitObjects[i].transform.GetComponent<Renderer>().materials;

            for (int m = 0; m < materials.Length; ++m) {
                materials[m].SetVector("_CutoutPos", cutoutPos);
                materials[m].SetFloat("_CutoutSize", 0.1f);
                materials[m].SetFloat("_FalloffSize", 0.05f);
            }
        }
    }
}