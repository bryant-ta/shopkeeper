using UnityEngine;

public class Cart : MonoBehaviour, IInteractable {
    [SerializeField] Transform driverPos;

    [SerializeField] bool requireRelease;
    public bool RequireRelease => requireRelease;
    
    public void Interact(GameObject interactor) {
        // setup player input -> cartmovement instead of playermovement
        // Rigidbody interactorRb = interactor.GetComponent<Rigidbody>();
        // interactorRb.isKinematic = true;
        interactor.GetComponent<PlayerMovement>().DisableMovement();
        
        
        // move player to driverPos
        interactor.transform.SetParent(driverPos, true);
        interactor.transform.localPosition = Vector3.zero;
        interactor.transform.localRotation = driverPos.rotation;
    }

    public void Release(GameObject interactor) {
        interactor.GetComponent<PlayerMovement>().EnableMovement();
        
        interactor.transform.SetParent(null);
    }
}
