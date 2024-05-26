using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicalButton : MonoBehaviour, IInteractable, IPointerDownHandler {
    public event Action OnInteract;
    public event Action OnRelease;

    public bool Interact(GameObject interactor) {
        // TODO: anim button pressed
        OnInteract?.Invoke();
        return true;
    }
    public void Release(GameObject interactor) {
        OnRelease?.Invoke();
        return;
    }
    
    // TEMP: until deciding on character vs. just pointer, needs to interact nicely with PlayerDrag (i.e. can click while holding a stack?)
    public void OnPointerDown(PointerEventData eventData) {
        Interact(gameObject);
    }
}
