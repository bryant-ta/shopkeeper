using System.Collections.Generic;
using Tags;
using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ProductID;
    public Texture2D Texture;
    public ShapeData ShapeData;
    public Color Color;
    public Pattern Pattern;
    
    public List<BasicTagID> BasicTagIDs;
    public List<ScoreTagID> ScoreTagIDs;
    public List<MoveTagID> MoveTagIDs;
    public List<PlaceTagID> PlaceTagIDs;
}

public enum Pattern {
    None = 0,
    StripeHor = 1,
    StripeVert = 2,
    StripeDiag = 3,
    Scratch = 10,
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
