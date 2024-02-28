using System;
using DG.Tweening;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    void Awake() {
        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(Vector3.forward * 3, 2));
        seq.Join(transform.DOShakePosition(2, 0.5f));
    }
}
