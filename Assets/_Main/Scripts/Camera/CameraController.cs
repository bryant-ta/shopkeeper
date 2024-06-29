using System;
using DG.Tweening;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    [Required] [SerializeField] Transform Target;

    [Title("Follow")]
    [SerializeField] float followSpeed;
    [SerializeField] float followOffset;

    [Title("Zoom")]
    [SerializeField] float zoomSpeed;
    [SerializeField] float zoomStep;
    [SerializeField] float zoomMin;
    [SerializeField] float zoomMax;

    float targetZoom;

    [Title("Rotation")]
    [SerializeField] float rotationDuration;

    Vector3 targetRotation;
    public event Action OnCameraRotate;
    public Vector3Int IsometricForward { get; private set; } // 45 degrees CCW (left side) from camera.forward 
    public Vector3Int IsometricRight { get; private set; }   // 45 degrees CW (right side) from camera.forward 

    Camera cam;

    void Awake() {
        if (transform.parent == null) {
            Debug.LogError("CameraController requires a parent object to be the base of the camera.");
            return;
        }

        if (Target == null) {
            Debug.LogWarning("Camera target is not set!");
            return;
        }

        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
        targetRotation = transform.parent.rotation.eulerAngles;

        Vector3 transForwardXZ = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        IsometricForward = Vector3Int.RoundToInt(Quaternion.Euler(0, -45, 0) * transForwardXZ);
        IsometricRight =  Vector3Int.RoundToInt(Quaternion.Euler(0, 45, 0) * transForwardXZ);

        Ref.Player.PlayerInput.InputScroll += ZoomView;
        Ref.Player.PlayerInput.InputRotateCameraDown += RotateCamera;
        Ref.Player.PlayerInput.InputRotateCameraUp += UnRotateCamera;
    }

    void LateUpdate() {
        FollowTarget();
        UpdateZoom();
    }

    void FollowTarget() {
        Vector3 targetPosition = new Vector3(Target.position.x, transform.parent.position.y, Target.position.z) +
                                 new Vector3(followOffset, 0, followOffset);
        transform.parent.position = Vector3.Lerp(transform.parent.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void ZoomView(float scrollInput) {
        if (!UpgradeManager.Flags.Zoom) return;

        targetZoom = cam.orthographicSize - scrollInput * zoomStep;
        targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);
    }

    void UpdateZoom() { cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime); }

    float lastRotateInput;
    void RotateCamera(float rotateCameraInput) {
        lastRotateInput = rotateCameraInput;
        
        // prevents rotation out of sync when attempting to rotate again during an active rotation
        transform.parent.rotation = Quaternion.Euler(targetRotation);

        int sign = (int) Mathf.Sign(rotateCameraInput);
        targetRotation = transform.parent.rotation.eulerAngles + new Vector3(0f, 90f * sign, 0f);
        IsometricForward = Vector3Int.RoundToInt(Quaternion.Euler(0, 90 * sign, 0) * IsometricForward);
        IsometricRight = Vector3Int.RoundToInt(Quaternion.Euler(0, 90 * sign, 0) * IsometricRight);

        transform.parent.DOKill();
        transform.parent.DORotate(targetRotation, rotationDuration).SetEase(Ease.OutQuad).OnComplete(() => {
                OnCameraRotate?.Invoke(); 
            }
        );
    }
    void UnRotateCamera(float rotateCameraInput) {
        RotateCamera(-lastRotateInput);
    }
}