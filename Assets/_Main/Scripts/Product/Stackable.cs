using UnityEngine;

public interface IStackable {
    /// <summary>
    /// Returns correct position for placing this IStackable on top of a stack.
    /// </summary>
    /// <param name="stackHeight">Y value of highest obj's top edge within stack</param>
    public Vector3 CalculateStackPosition(float stackHeight);
    
    public Transform GetTransform();
    public Stack GetStack();
    public Collider GetCollider();
}

[RequireComponent(typeof(BoxCollider))]
public class BoxStackable : MonoBehaviour, IStackable, IGridShape {
    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
    }

    public Vector3 CalculateStackPosition(float stackHeight) {
        float yOffset = boxCol.bounds.extents.y;
        return new Vector3(0, stackHeight + yOffset, 0);
    }

    public Transform GetTransform() { return transform; }
    public Stack GetStack() {
        if (transform.parent.TryGetComponent(out Stack stack)) {
            return stack;
        } else {
            Debug.LogError("IStackable is missing Stack in parent object");
            return null;
        }
    }
    public Collider GetCollider() { return boxCol; }
}