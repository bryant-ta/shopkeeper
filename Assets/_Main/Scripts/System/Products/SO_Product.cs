using System.Collections.Generic;
using Tags;
using UnityEngine;

[CreateAssetMenu(menuName = "Products/SO_Product")]
public class SO_Product : ScriptableObject {
    public ProductID ProductID;
    public Texture2D Texture;
    public ShapeData ShapeData;
    
    public List<BasicTagID> BasicTagIDs;
    public List<ScoreTagID> ScoreTagIDs;
    public List<MoveTagID> MoveTagIDs;
    public List<PlaceTagID> PlaceTagIDs;
    
    // TODO: a way to specify score mult
}

// public struct ProductInitData {
//     public ProductInitData(ProductID productID, Texture2D texture, ShapeData shapeData, List<BasicTagID> basicTagIDs, List<ScoreTagID> scoreTagIDs, List<MoveTagID> moveTagIDs, List<PlaceTagID> placeTagIDs) {
//         ProductID = productID;
//         Texture = texture;
//         ShapeData = shapeData;
//         BasicTagIDs = basicTagIDs;
//         ScoreTagIDs = scoreTagIDs;
//         MoveTagIDs = moveTagIDs;
//         PlaceTagIDs = placeTagIDs;
//     }
//     
//     public ProductID ProductID { get; }
//     public Texture2D Texture { get; }
//     public ShapeData ShapeData { get; }
//     
//     public List<BasicTagID> BasicTagIDs { get; }
//     public List<ScoreTagID> ScoreTagIDs { get; }
//     public List<MoveTagID> MoveTagIDs { get; }
//     public List<PlaceTagID> PlaceTagIDs { get; }
//     
//     // TODO: a way to specify score mult
// }
// ProductInitData productInitData = new ProductInitData {
//     ProductID = productData.ProductID,
//     Texture = productData.Texture,
//     ShapeData = productData.ShapeData,
//     BasicTagIDs = productData.BasicTagIDs,
//     ScoreTagIDs = productData.ScoreTagIDs,
//     MoveTagIDs = productData.MoveTagIDs,
//     PlaceTagIDs = productData.PlaceTagIDs,
// }

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
