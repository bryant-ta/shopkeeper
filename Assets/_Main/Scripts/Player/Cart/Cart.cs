using System;
using UnityEngine;

public class Cart : MonoBehaviour, IInteractable {
    [SerializeField] Transform driverPos;

    [SerializeField] bool requireRelease;
    public bool RequireRelease => requireRelease;
    
    public event Action OnInteract;
    public event Action OnRelease;

    public void Interact(GameObject interactor) {
        interactor.GetComponent<PlayerMovement>().DisableMovement();
        
        interactor.transform.SetParent(driverPos, true);
        interactor.transform.localPosition = Vector3.zero;
        interactor.transform.localRotation = driverPos.rotation;
        
        OnInteract?.Invoke();
    }

    public void Release(GameObject interactor) {
        interactor.GetComponent<PlayerMovement>().EnableMovement();
        
        interactor.transform.SetParent(null);
        
        OnRelease?.Invoke();
    }
}
