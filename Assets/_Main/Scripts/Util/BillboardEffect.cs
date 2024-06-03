using UnityEngine;

public class BillboardEffect : MonoBehaviour {
    CameraController camCtrl;

    // NOTE: must be attached to parent of canvas, not directly to canvas due to rect transform rotation interaction
    void Awake() {
        if (GetComponent<Canvas>() != null) {
            Debug.LogError("Cannot use this script directly on Canvas object. Attach to regular game object as parent of Canvas.");
            return;
        }

        camCtrl = Camera.main.GetComponent<CameraController>();

        camCtrl.OnCameraRotate += RotateWithCamera;
    }

    // NOTE: this is called 4 extra times per event invoke... DoTween bug?
    void RotateWithCamera() {
        transform.rotation = Quaternion.LookRotation(-camCtrl.transform.forward);
    }
}