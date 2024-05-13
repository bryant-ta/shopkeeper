using UnityEngine;

public class GridFloorHelper : MonoBehaviour {
    [Tooltip("Reference to Grid that this plane supports.")]
    [field: SerializeField] public Grid Grid { get; private set; }
}