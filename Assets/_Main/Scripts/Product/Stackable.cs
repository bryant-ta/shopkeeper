using UnityEngine;

public interface IStackable {
    public Vector3 CalculateStackPosition(int index);
    public Transform GetTransform();
}