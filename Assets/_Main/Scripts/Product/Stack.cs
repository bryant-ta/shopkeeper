using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stack : MonoBehaviour {
    List<IStackable> items = new List<IStackable>();

    public bool IsLocked;
    public bool DestroyOnEmpty = true;

    // Initialize items with any IStackable in transform children
    public void Init() {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).TryGetComponent(out IStackable s)) {
                Place(s);
            } else {
                Debug.LogError("Expected only IStackables in children");
            }
        }
    }

    public void Place(IStackable stackable) { Add(stackable); }

    /// <summary>
    /// Moves range of items from this stack to destStack. Use when directly moving items between two existing stacks.
    /// </summary>
    /// <param name="destStack">Stack to move to.</param>
    /// <param name="startIndex">Range start (inclusive).</param>
    /// <param name="endIndex">Range end (inclusive).</param>
    public void PlaceRange(Stack destStack, int startIndex, int endIndex) {
        if (destStack == null) {
            Debug.LogError("Cannot place stack on nothing.");
            return;
        }
        MoveRangeTo(destStack, startIndex, endIndex);
    }
    public void PlaceAll(Stack destStack) { PlaceRange(destStack, 0, items.Count - 1); }

    /// <summary>
    /// Creates a new stack from taking every item above input IStackable (inclusive). Use when moving items to a non-existent stack.
    /// </summary>
    /// <param name="s">Stack item that will be the first item of the new stack.</param>
    public Stack Take(IStackable s) {
        if (!items.Contains(s)) {
            Debug.LogWarningFormat("Item {0} does not exist in stack.", s.GetTransform().name);
            return null;
        }

        Stack newStack = SplitStack(s);
        return newStack;
    }

    public Stack Pop() {
        if (items.Count == 0) return null;
        return Take(Top());
    }

    // SplitStack returns a new stack (+object) containing all cards from input card to end of current stack
    Stack SplitStack(IStackable s) {
        int splitIndex = items.IndexOf(s);
        Stack newStack = Factory.Instance.CreateStack();

        MoveRangeTo(newStack, splitIndex, items.Count - 1);

        return newStack;
    }
    
    #region Internal

    void MoveRangeTo(Stack newStack, int startIndex, int endIndex) {
        // Needs to be foreach on list copy because modifying items list while iterating
        List<IStackable> stackables = items.GetRange(startIndex, endIndex - startIndex + 1);
        foreach (IStackable s in stackables) {
            Remove(s);
        }
        
        foreach (IStackable s in stackables) {
            newStack.Add(s);
        }
    }
    void Add(IStackable s) {
        float curStackHeight = 0f; // y value of highest obj's top edge within stack
        if (items.Count > 0) {
            curStackHeight = items.Last().GetTransform().position.y + items.Last().GetCollider().bounds.extents.y;
        }
        
        items.Add(s);
        Transform itemTrans = s.GetTransform();
        itemTrans.parent = transform;
        itemTrans.localPosition = s.CalculateStackPosition(curStackHeight);
        itemTrans.localRotation = Quaternion.identity;
        if (itemTrans.TryGetComponent(out Rigidbody rb)) {
            rb.isKinematic = true;
        }
    }
    void Remove(IStackable s) {
        items.Remove(s);
        Transform itemTrans = s.GetTransform();
        itemTrans.parent = null;
        // if (itemTrans.TryGetComponent(out Rigidbody rb)) {
        //     rb.isKinematic = false;
        // }

        TryDestroyStack();
    }

    public void ModifyStackProperties(Action<IStackable> modifier) {
        for (int i = 0; i < items.Count; i++) {
            modifier(items[i]);
        }
    }
    
    #endregion

    #region Helper

    public List<T> GetItemComponents<T>() {
        List<T> components = new List<T>();
        foreach (IStackable s in items) {
            if (s.GetTransform().TryGetComponent(out T component)) {
                components.Add(component);
            } else {
                Debug.LogErrorFormat("Component %s missing from card stack", typeof(T));
            }
        }

        return components;
    }
    public List<string> GetItemNames() {
        List<string> itemNames = new List<string>();
        foreach (IStackable s in items) {
            itemNames.Add(s.GetTransform().gameObject.name);
        }

        return itemNames;
    }
    public IStackable GetItemByName(string itemName) {
        foreach (IStackable s in items) {
            if (s.GetTransform().name == itemName) {
                return s;
            }
        }

        return null;
    }
    public int IndexOf(IStackable s) { return items.IndexOf(s); }
    public List<IStackable> ItemsCopy() { return new List<IStackable>(items); }
    public void TryDestroyStack() {
        if (DestroyOnEmpty && items.Count == 0) {
            Destroy(gameObject);
        }
    }
    public int Size() { return items.Count; }
    public bool IsEmpty() { return items.Count == 0; }
    // Top returns the highest (i.e. has objs under) card in the stack
    public IStackable Top() { return items.Count > 0 ? items.Last() : null; }

    // TODO: RecalculateStackPositions moves stack objs to their correct positions
    public void RecalculateStackPositions() { }
    
    #endregion
}