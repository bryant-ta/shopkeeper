using System;
using System.Numerics;
using DG.Tweening;
using EventManager;
using TriInspector;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
        targetRotation = transform.rotation.eulerAngles;

        Ref.Player.PlayerInput.InputScroll += ZoomView;
        Ref.Player.PlayerInput.InputRotateCamera += RotateCamera;
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

    void RotateCamera(float rotateCameraInput) {
        targetRotation = transform.parent.rotation.eulerAngles + new Vector3(0f, 90f * Mathf.Sign(rotateCameraInput), 0f);
        
        transform.parent.DOKill();
        transform.parent.DORotate(targetRotation, rotationDuration, RotateMode.Fast).OnUpdate(() => OnCameraRotate?.Invoke());
    }
}