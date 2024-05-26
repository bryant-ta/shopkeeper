using DG.Tweening;
using UnityEngine;

public class DebugHoverAnim : MonoBehaviour {
    [SerializeField] HoverEvent he;
    
    void Awake() {
        he.OnHoverEnter += Open;
        he.OnHoverExit += Close;
    }

    void Open() {
        // TEMP: replace with open/close animation
        transform.DOKill(true);
        transform.DOShakeScale(0.2f, 0.5f, 10, 90);
    }

    void Close() {
        // TEMP: replace with open/close animation
        transform.DOKill(true);
        transform.DOShakeScale(0.2f, 0.5f, 10, 90);
    }
}
