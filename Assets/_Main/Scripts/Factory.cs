using UnityEngine;

public class Factory : Singleton<Factory> {
    [SerializeField] GameObject stackBase;

    public Stack CreateStack() {
        GameObject newStackObj = Instantiate(stackBase, Vector3.zero, Quaternion.identity);
        return newStackObj.GetComponent<Stack>();
    }
}
