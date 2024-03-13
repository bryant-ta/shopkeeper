using UnityEngine;

public class GridPlaneHelper : MonoBehaviour {
    [Tooltip("Reference to Grid that this plane supports.")]
    [field:SerializeField] public Grid Grid { get; private set; }
}
