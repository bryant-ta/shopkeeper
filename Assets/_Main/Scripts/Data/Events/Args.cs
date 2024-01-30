using UnityEngine;

namespace EventManager {
    public struct ClickInputArgs {
        public Vector3 hitPoint;
        public GameObject TargetObj;
    }

    public struct MoveInputArgs {
        public Vector2 MoveInput;
    }
}