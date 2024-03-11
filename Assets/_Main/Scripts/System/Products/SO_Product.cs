using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ID;
    public Texture2D Texture;
}

public enum ProductID {
    Blank = 0,
    Beacon = 1,
    Bedrock = 2,
    Cobblestone = 3,
    DiamondOre = 4,
    Dirt = 5,
    Emerald = 6,
    Glass = 7,
    GlazedTerracotta = 8,
    Gold = 9,
    Iron = 10,
    Mushroom = 11,
    Planks = 12,
    RedstoneLamp = 13,
    Sand = 14,
    StoneGranite = 15,
    WoolPurple = 16,
}
