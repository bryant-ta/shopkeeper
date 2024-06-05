using UnityEngine;

public class LookAtOnCameraRotation : MonoBehaviour {
    [SerializeField] LookAtMode lookAtMode;
    [SerializeField] bool flip;
    
    CameraController camCtrl;

    // NOTE: must be attached to regular game object, not directly to canvas due to rect transform rotation interaction
    void Awake() {
        if (GetComponent<Canvas>() != null) {
            Debug.LogError("Cannot use this script directly on Canvas object. Attach to regular game object as parent of Canvas.");
            return;
        }

        camCtrl = Camera.main.GetComponent<CameraController>();
        camCtrl.OnCameraRotate += RotateWithCamera;
    }

    // NOTE: this is called 4 extra times per event invoke... DoTween bug?
    public void RotateWithCamera() {
        int neg = flip ? -1 : 1;
        switch (lookAtMode) {
            case LookAtMode.IsometricForward:
                transform.rotation = Quaternion.LookRotation(camCtrl.IsometricForward * neg);
                break;
            case LookAtMode.IsometricRight:
                transform.rotation = Quaternion.LookRotation(camCtrl.IsometricRight * neg);
                break;
            case LookAtMode.CameraForward:
                transform.rotation = Quaternion.LookRotation(camCtrl.transform.forward * neg);
                break;
            default:
                break;
        }
    }

    enum LookAtMode {
        IsometricForward = 0,
        IsometricRight = 1,
        CameraForward = 2,
    }
}
