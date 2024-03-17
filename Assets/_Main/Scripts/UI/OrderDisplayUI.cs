using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderDisplayUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI orderText;
    [SerializeField] GameObject EndStatusStampPanel;
    [SerializeField] Image fulfilledStamp;
    [SerializeField] Image failedStamp;
    
    Order displayedOrder;
    
    public void DisplayNewOrder(Order order) {
        displayedOrder = order;
        string t = displayedOrder.ToString().Replace("\n", "   ");
        orderText.text = t;
    }

    public void DisplayEndStatusStamp(bool success) {
        EndStatusStampPanel.SetActive(enabled);
        
        if (success) {
            fulfilledStamp.gameObject.SetActive(true);
            failedStamp.gameObject.SetActive(false);
        } else {
            fulfilledStamp.gameObject.SetActive(false);
            failedStamp.gameObject.SetActive(true);
        }
    }
    public void HideEndStatusStamp() { EndStatusStampPanel.SetActive(false); }
}
