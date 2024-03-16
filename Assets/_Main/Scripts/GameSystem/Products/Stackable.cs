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