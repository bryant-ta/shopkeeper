using System;
using EventManager;
using TriInspector;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    [Required] public Transform Target;

    [Title("Follow")]
    [SerializeField] float followSpeed;
    [SerializeField] float followOffset;

    [Title("Zoom")]
    [SerializeField] float zoomSpeed;
    [SerializeField] float zoomStep;
    [SerializeField] float zoomMin;
    [SerializeField] float zoomMax;
    float targetZoom;

    Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void Start() {
        Events.Sub<float>(gameObject, EventID.Scroll, ZoomView);
    }

    void LateUpdate() {
        FollowTarget();
        UpdateZoom();
    }

    void FollowTarget() {
        if (Target == null) {
            Debug.LogWarning("Camera target is not set!");
            return;
        }

        Vector3 targetPosition = new Vector3(Target.position.x, transform.position.y, Target.position.z) +
                                 new Vector3(followOffset, 0, followOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void ZoomView(float scrollInput) {
        targetZoom = cam.orthographicSize - scrollInput * zoomStep;
        targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);
    }
    void UpdateZoom() { cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime); }
}