using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ID;
    public Texture2D Texture;
}

public enum ProductID {
    Blank = 0,
    Beacon,
    Bedrock,
    Cobblestone,
    DiamondOre,
    Dirt,
    Emerald,
    Glass,
    GlazedTerracotta,
    Gold,
    Iron,
    Mushroom,
    Planks,
    RedstoneLamp,
    Sand,
    StoneGranite,
    WoolPurple,
}
