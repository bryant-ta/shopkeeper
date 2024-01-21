using System;
using UnityEngine;

public interface IStackable {
    public bool StackOn(Stack stack);
}

[RequireComponent(typeof(BoxCollider))]
public class BoxStackable : MonoBehaviour, IStackable {
    BoxCollider boxCol;

    void Awake() {
        boxCol = GetComponent<BoxCollider>();
    }

    public bool StackOn(Stack stack) {
        // Move self to top of stack
        float yOffset = boxCol.bounds.extents.y;
        transform.SetParent(stack.transform);
        transform.position = new Vector3(0, stack.GetStackSize() * yOffset, 0);

        return true;
    }
}