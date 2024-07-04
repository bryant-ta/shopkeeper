using System.Collections.Generic;
using UnityEngine;

public class ToolsUI : MonoBehaviour {
    [SerializeField] Transform arrowBase;

    [SerializeField] List<RectTransform> iconPositions;

    void Awake() { Ref.Player.OnToolSwitch += RotateArrow; }

    void RotateArrow(int curToolIndex) {
        RectTransform targetRectTransform = iconPositions[curToolIndex];
        Vector3 dir = targetRectTransform.position - arrowBase.transform.position;

        // Calculate the angle between the direction vector and the up vector (since RectTransform uses up vector as default)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrowBase.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}