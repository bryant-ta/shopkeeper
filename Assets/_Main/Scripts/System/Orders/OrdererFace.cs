using Orders;
using UnityEngine;

public class OrdererFace : MonoBehaviour {
    [SerializeField] Sprite successFace;
    SpriteRenderer face;

    void Awake() {
        face = GetComponent<SpriteRenderer>();
        Orderer orderer = GetComponentInParent<Orderer>();
        orderer.OnOrderFinished += SetFaceSuccess;
    }

    void SetFaceSuccess(Order order) {
        if (order.IsFulfilled) {
            face.sprite = successFace;
        }
    }
}