using UnityEngine;

public class DifficultyManager : MonoBehaviour {
    [SerializeField] SO_DeliveriesDifficultyTable deliveryDiffTable;
    [SerializeField] SO_OrdersDifficultyTable orderDiffTable;
    
    [SerializeField] SO_DeliveriesDifficultyTable deliveryDiffTableOverride;
    [SerializeField] SO_OrdersDifficultyTable orderDiffTableOverride;

    public void ApplyDifficulty() {
        ApplyDeliveryDifficulty();
        ApplyOrderDifficulty();
    }

    void ApplyDeliveryDifficulty() {
        
    }
    void ApplyOrderDifficulty() {
        
    }
}
