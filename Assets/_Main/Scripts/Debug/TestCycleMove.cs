using System;
using DG.Tweening;
using UnityEngine;

public class TestCycleMove : MonoBehaviour {
    [SerializeField] Vector3 targetOffset;
    [SerializeField] float cycleDuration = 2f;

    void Start() {
        transform.DOMove(transform.position + targetOffset, cycleDuration)
            .SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }
}
