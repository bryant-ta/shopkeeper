using System.Collections.Generic;
using Orders;
using UnityEngine;

public class OrdererFace : MonoBehaviour {
    [SerializeField] List<Sprite> successFaces;
    [SerializeField] List<Sprite> failFaces;
    SpriteRenderer face;

    void Awake() {
        face = GetComponent<SpriteRenderer>();
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnOrderFinished += SetFaceSuccess;
    }

    void SetFaceSuccess(Order order) {
        if (order.IsFulfilled) {
            face.sprite = Util.GetRandomFromList(successFaces);
        } else {
            face.sprite = Util.GetRandomFromList(failFaces);
        }
    }
}