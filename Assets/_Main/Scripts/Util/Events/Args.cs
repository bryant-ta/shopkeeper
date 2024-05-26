using UnityEngine;

public struct ClickInputArgs {
    public Vector3 HitNormal;
    public Vector3 HitPoint;
    public GameObject TargetObj;
}

public struct MoveInputArgs {
    public Vector2 MoveInput;
}