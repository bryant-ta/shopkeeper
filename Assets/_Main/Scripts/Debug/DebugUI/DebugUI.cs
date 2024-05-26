using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI InputPointTargetObjText;

    void Awake() { Ref.Player.PlayerInput.InputPoint += GetInputPoint; }

    void Update() {
        if (Keyboard.current[Key.F3].wasPressedThisFrame) {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        if (!gameObject.activeSelf) return;

        if (inputPointArgs.TargetObj != null) {
            InputPointTargetObjText.text = $"InputPoint.TargetObj: {inputPointArgs.TargetObj.name}";
        }
    }

    ClickInputArgs inputPointArgs;
    void GetInputPoint(ClickInputArgs clickInputArgs) { inputPointArgs = clickInputArgs; }
}