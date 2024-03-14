using System;
using UnityEngine;

public class Cart : MonoBehaviour, IInteractable {
    [SerializeField] Transform driverPos;

    [SerializeField] bool requireRelease;
    public bool RequireRelease => requireRelease;
    
    public event Action OnInteract;
    public event Action OnRelease;

    public bool Interact(GameObject interactor) {
        if (interactor.TryGetComponent(out Player player)) {
            if (!player.PlayerInteract.HoldGrid.IsEmpty() || !player.PlayerDrag.DragGrid.IsEmpty()) {
                return false;
            }
            
            player.PlayerMovement.DisableMovement();
            player.PlayerInput.SetActionMap(Constants.ActionMapNameVehicle);
        }
        
        // enable cart driver colliders
        driverPos.gameObject.SetActive(true);
        
        // move interactor
        interactor.transform.SetParent(driverPos, true);
        interactor.transform.localPosition = Vector3.zero;
        interactor.transform.rotation = driverPos.rotation;
        
        OnInteract?.Invoke();

        return true;
    }

    public void Release(GameObject interactor) {
        if (interactor.TryGetComponent(out Player player)) {
            player.PlayerMovement.EnableMovement();
            player.PlayerInput.SetActionMap(Constants.ActionMapNamePlayer);
        }
        
        driverPos.gameObject.SetActive(false);
        
        interactor.transform.SetParent(null);
        
        OnRelease?.Invoke();
    }
}
