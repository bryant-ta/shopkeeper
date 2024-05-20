using UnityEngine;

public class Orderer : MonoBehaviour {
    public Grid Grid { get; private set; }

    void Awake() { Grid = gameObject.GetComponentInChildren<Grid>(); }

    public void Enable() { gameObject.SetActive(true); }
    public void Disable() { gameObject.SetActive(false); }
}