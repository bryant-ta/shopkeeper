using Tags;
using TriInspector;
using UnityEngine;

public class Product : MonoBehaviour {
    [field: SerializeField, ReadOnly] public ProductID ID { get; private set; }
    public string Name { get; private set; }

    public ProductTags Tags;

    public void Init(SO_Product productData, ProductTags tags) {
        ID = productData.ID;
        Name = productData.ID.ToString();
        gameObject.name = Name;
        Tags = tags;
        
        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", productData.Texture);
    }
}