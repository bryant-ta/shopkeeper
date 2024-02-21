using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberCounter : MonoBehaviour {
    [SerializeField] float countingDuration;
    [SerializeField] string prefix;
    
    TextMeshProUGUI valueText;
    int value;

    void Awake() { valueText = GetComponent<TextMeshProUGUI>(); }
    
    public void SetValue(int newValue) {
        AnimateNumberCounter(value, newValue);
        value = newValue;
    }
    
    void AnimateNumberCounter(int oldValue, int newValue) {
        DOVirtual.Int(oldValue, newValue, countingDuration, (x) => valueText.text = prefix + x).SetEase(Ease.OutFlash);
    }
}