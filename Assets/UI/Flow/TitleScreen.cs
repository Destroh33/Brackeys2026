using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] float minimumHold = 0.4f;

    float elapsed;

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (elapsed < minimumHold) return;

        bool pressed =
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (pressed) ScreenFlow.GoOptions();
    }
}
