using TMPro;
using UnityEngine;

public class OrderDisplayUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI orderText;
    
    Order displayedOrder;
    
    public void DisplayNewOrder(Order order) {
        displayedOrder = order;
        string t = displayedOrder.ToString().Replace("\n", "   ");
        orderText.text = t;
    }
}
