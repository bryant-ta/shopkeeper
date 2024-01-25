using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stack : MonoBehaviour {
    List<IStackable> items = new List<IStackable>();

    public bool isLocked;

    // TEMP
    [SerializeField] GameObject stackBaseObj;

    public void Place(IStackable stackable) { Add(stackable); }

    // null input treated as placing stack on nothing
    public void PlaceAll(Stack destStack) { MoveRangeTo(destStack, 0, items.Count); }

    public Transform Take(IStackable s) {
        if (!items.Contains(s)) {
            Debug.LogWarningFormat("Item {0} does not exist in stack.", s.GetTransform().name);
            return null;
        }

        Transform newStack = SplitStack(s).transform;
        return newStack;
    }

    public Transform Pop() { return Take(Top()); }

    // SplitStack returns a new stack (+object) containing all cards from input card to end of current stack
    Transform SplitStack(IStackable s) {
        int splitIndex = items.IndexOf(s);
        GameObject newStackObj = Instantiate(stackBaseObj, transform.position, Quaternion.identity);
        Stack newStack = newStackObj.GetComponent<Stack>();

        MoveRangeTo(newStack, splitIndex, items.Count - splitIndex);

        return newStack.transform;
    }

    void MoveRangeTo(Stack newStack, int startIndex, int count) {
        List<IStackable>
            stackables = items.GetRange(startIndex, count); // Create copy for adding to newStack bc RemoveCard() will delete objs
        foreach (IStackable s in stackables) {
            Remove(s);
        }

        foreach (IStackable s in stackables) {
            newStack.Add(s);
        }
    }
    void Add(IStackable s) {
        items.Add(s);
        s.GetTransform().parent = transform;
        s.GetTransform().localPosition = s.CalculateStackPosition(items.Count - 1);
    }
    void Remove(IStackable s) {
        items.Remove(s);
        s.GetTransform().parent = null;
        // TODO: prob flag for "temp stacks" that should be destroyed when empty vs. permanent stacks like on shelves
        // if (stack.Count == 0) {
        //     Destroy(gameObject);
        // }
    }

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
}