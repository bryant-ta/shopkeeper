using TriInspector;
using UnityEngine;

[RequireComponent(typeof(UpgradeManager))]
public class ShopExpansionManager : MonoBehaviour {
    // TEMP: until creating a real building system using tiles
    [Title("Shop Expansion 1")]
    [SerializeField] GameObject room1;
    [SerializeField] GameObject connectingWall1;
    
    [Title("Shop Expansion 2")]
    [SerializeField] GameObject room2;
    [SerializeField] GameObject connectingWall2;

    public void DoShopExpansion(int index) {
        Grid worldGrid = GameManager.WorldGrid;
        int x = worldGrid.Length / 2;
        int z = worldGrid.Width / 2;
        
        switch (index) {
            case 1:
                room1.SetActive(true);
                connectingWall1.SetActive(false);
                worldGrid.AddValidCellsRange(new Vector2Int(x, -z), new Vector2Int(x+worldGrid.Length, z));
                break;
            case 2:
                room2.SetActive(true);
                connectingWall2.SetActive(false);
                worldGrid.AddValidCellsRange(new Vector2Int(-x-worldGrid.Length, -z), new Vector2Int(-x, z));
                break;
            default:
                Debug.LogError($"Unexpected index: {index}");
                break;
        }
    }
}
