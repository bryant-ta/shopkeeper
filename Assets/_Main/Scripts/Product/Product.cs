using TriInspector;
using UnityEngine;

public class Product : MonoBehaviour {
    [field: SerializeField, ReadOnly] public ProductID ID { get; private set; }
    public string Name { get; private set; }

    public void Init(SO_Product productData) {
        ID = productData.ID;
        Name = productData.ID.ToString();
        gameObject.name = Name;

        GetComponent<MeshRenderer>().material.SetTexture("_MainTex", productData.Texture);
    }
}