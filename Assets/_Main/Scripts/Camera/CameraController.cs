using TriInspector;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [Required] public Transform Target;
    public float followSpeed;
    public float offset;

    void LateUpdate() {
        if (Target == null) {
            Debug.LogWarning("Camera target is not set!");
            return;
        }

        Vector3 targetPosition = new Vector3(Target.position.x, transform.position.y, Target.position.z) + 
                                 new Vector3(offset, 0, offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}