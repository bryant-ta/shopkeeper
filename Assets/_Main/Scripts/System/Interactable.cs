using UnityEngine;

public interface IInteractable {
    public bool RequireRelease { get; }
    
    public void Interact(GameObject interactor);
    public void Release(GameObject interactor);
}
