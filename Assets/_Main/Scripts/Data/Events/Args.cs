using UnityEngine;

namespace EventManager {
    public struct ClickInputArgs {
        public GameObject TargetObj;
    }

    public struct MoveInputArgs {
        public Vector2 MoveInput;
    }
}