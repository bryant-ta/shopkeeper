using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour {
    List<IStackable> stack;

    public void Place(IStackable stackable) {
        stackable.StackOn(this);
        stack.Add(stackable);
    }

    #region Helper

    public int GetStackSize() {
        return stack.Count;
    }
    

    #endregion
}
