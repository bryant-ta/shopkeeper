using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public event Action OnHoverEnter;
    public event Action OnHoverExit;

    bool isHovering = false;

    public void OnPointerEnter(PointerEventData eventData) {
        if (!isHovering) {
            isHovering = true;
            OnHoverEnter?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isHovering) {
            isHovering = false;
            OnHoverExit?.Invoke();
        }
    }
}