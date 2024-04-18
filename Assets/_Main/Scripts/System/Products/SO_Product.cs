using System;
using System.Collections.Generic;
using Tags;
using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ID;
    public Texture2D Texture;
    
    public List<BasicTagID> BasicTagIDs;
    public List<ScoreTagID> ScoreTagIDs;
    public List<MoveTagID> MoveTagIDs;
    public List<PlaceTagID> PlaceTagIDs;
    
    // TODO: a way to specify score mult
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