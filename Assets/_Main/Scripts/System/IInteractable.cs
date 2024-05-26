using System;
using UnityEngine;

// TODO: rethink this interface in context of character vs. just pointer
public interface IInteractable {
    public event Action OnInteract;
    public event Action OnRelease;
    
    public bool Interact(GameObject interactor);
    public void Release(GameObject interactor);
    // public void InteractInvalid();
}
