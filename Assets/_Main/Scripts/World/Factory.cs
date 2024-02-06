using UnityEngine;

public class Factory : Singleton<Factory> {
    [SerializeField] GameObject productBase;
    [SerializeField] GameObject stackBase;

    public IGridShape CreateProduct() {
        GameObject newProductObj = Instantiate(productBase, Vector3.zero, Quaternion.identity);
        return newProductObj.transform.GetChild(0).GetComponent<IGridShape>();
    }
    
    public Stack CreateStack() {
        GameObject newStackObj = Instantiate(stackBase, Vector3.zero, Quaternion.identity);
        return newStackObj.GetComponent<Stack>();
    }
}
