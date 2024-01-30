using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stack : MonoBehaviour {
    List<IStackable> items = new List<IStackable>();

    public bool isLocked;
    public bool destroyOnEmpty = true;

    // TEMP
    [SerializeField] GameObject stackBaseObj;

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

    // null input treated as placing stack on nothing
    public void PlaceAll(Stack destStack) { MoveRangeTo(destStack, 0, items.Count); }

    /// <summary>
    /// Returns the new stack created from taking every item above input IStackable (inclusive).
    /// </summary>
    /// <param name="s">Stack item that will be the first item in a new stack, also moving everything above it.</param>
    public Stack Take(IStackable s) {
        if (!items.Contains(s)) {
            Debug.LogWarningFormat("Item {0} does not exist in stack.", s.GetTransform().name);
            return null;
        }

        Stack newStack = SplitStack(s);
        return newStack;
    }

    public Stack Pop() { return Take(Top()); }

    // SplitStack returns a new stack (+object) containing all cards from input card to end of current stack
    Stack SplitStack(IStackable s) {
        int splitIndex = items.IndexOf(s);
        GameObject newStackObj = Instantiate(stackBaseObj, transform.position, Quaternion.identity);
        Stack newStack = newStackObj.GetComponent<Stack>();

        MoveRangeTo(newStack, splitIndex, items.Count - splitIndex);

        return newStack;
    }

    void MoveRangeTo(Stack newStack, int startIndex, int count) {
        List<IStackable> stackables = items.GetRange(startIndex, count); // Create copy for adding to newStack bc Remove() will delete objs
        for (int i = startIndex; i < items.Count; i++) {
            Remove(items[i]);
        }
        
        for (int i = 0; i < stackables.Count; i++) {
            newStack.Add(stackables[i]);
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
        if (itemTrans.TryGetComponent(out Rigidbody rb)) {
            rb.isKinematic = true;
        }
    }
    void Remove(IStackable s) {
        items.Remove(s);
        Transform itemTrans = s.GetTransform();
        itemTrans.parent = null;
        if (itemTrans.TryGetComponent(out Rigidbody rb)) {
            rb.isKinematic = false;
        }
        
        // TODO: prob flag for "temp stacks" that should be destroyed when empty vs. permanent stacks like on shelves
        if (destroyOnEmpty && items.Count == 0) {
            Destroy(gameObject);
        }
    }

    public void ModifyStackProperties(Action<IStackable> modifier) {
        for (int i = 0; i < items.Count; i++) {
            modifier(items[i]);
        }
    }

    #region Helper

    public List<T> GetObjComponents<T>() where T : Component {
        List<T> components = new List<T>();
        foreach (IStackable s in items) {
            T component = s.GetTransform().GetComponent<T>();
            if (component != null) {
                components.Add(component);
            } else {
                Debug.LogErrorFormat("Component %s missing from card stack", typeof(T));
            }
        }

        return components;
    }
    public List<string> GetObjNames() {
        List<string> cardNames = new List<string>();
        foreach (IStackable s in items) {
            cardNames.Add(s.GetTransform().name);
        }

        return cardNames;
    }
    public IStackable GetObjByName(string cardName) {
        foreach (IStackable s in items) {
            if (s.GetTransform().name == cardName) {
                return s;
            }
        }

        return null;
    }
    public List<IStackable> StackCopy() { return new List<IStackable>(items); }
    public int GetStackSize() { return items.Count; }
    // Top returns the highest (i.e. has objs under) card in the stack
    public IStackable Top() { return items.Count > 0 ? items.Last() : null; }

    // TODO: RecalculateStackPositions moves stack objs to their correct positions
    public void RecalculateStackPositions() { }
    
    #endregion
}