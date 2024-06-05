using System.Collections.Generic;
using UnityEngine;

public class Deliverer : MonoBehaviour {
    public Grid Grid { get; private set; }

    void Awake() {
        Grid = gameObject.GetComponentInChildren<Grid>();

        Grid.OnRemoveShapes += DisableOnEmpty;
    }

    // TEMP: placeholder until doing anims/theme for basic delivery
    void DisableOnEmpty(List<IGridShape> shapes) {
        if (!Grid.IsEmpty()) return;
        Disable();
    }

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}