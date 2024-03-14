using System;
using UnityEngine;

public interface IInteractable {
    public bool RequireRelease { get; }
    
    public event Action OnInteract;
    public event Action OnRelease;
    
    public void Interact(GameObject interactor);
    public void Release(GameObject interactor);
}
