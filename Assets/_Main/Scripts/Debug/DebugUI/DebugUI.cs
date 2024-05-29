using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI InputPointTargetObjText;
    [SerializeField] TextMeshProUGUI InputPointHitPointText;
    [SerializeField] TextMeshProUGUI InputPointHitNormalText;

    void Awake() { Ref.Player.PlayerInput.InputPoint += GetInputPoint; }

    void Update() {
        if (Keyboard.current[Key.F3].wasPressedThisFrame) {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        if (!gameObject.activeSelf) return;

        if (inputPointArgs.TargetObj != null) {
            InputPointTargetObjText.text = $"InputPoint.TargetObj: {inputPointArgs.TargetObj.name}";
        }

        InputPointHitPointText.text = $"InputPoint.HitPoint: {inputPointArgs.HitPoint}";
        InputPointHitNormalText.text = $"InputPoint.HitNormal: {inputPointArgs.HitNormal}";
    }

    ClickInputArgs inputPointArgs;
    void GetInputPoint(ClickInputArgs clickInputArgs) { inputPointArgs = clickInputArgs; }
}