using UnityEngine;
using UnityEngine.InputSystem;

public class CursorAutoHide : MonoBehaviour
{
    [SerializeField] private CursorLockMode lockModeWhenHidden = CursorLockMode.Confined; // or Locked
    private bool isVisible = true;

    void OnEnable()
    {
        SetCursorVisible(true);
    }

    void Update()
    {
        bool mouseUsed =
            Mouse.current != null &&
            (
                Mouse.current.delta.ReadValue().sqrMagnitude > 0 ||
                Mouse.current.scroll.ReadValue().sqrMagnitude != 0 ||
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame
            );

        var gp = Gamepad.current;
        bool gamepadUsed =
            gp != null && (
                gp.leftStick.ReadValue().sqrMagnitude > 0f ||
                gp.rightStick.ReadValue().sqrMagnitude > 0f ||
                gp.dpad.ReadValue().sqrMagnitude > 0f ||
                gp.buttonSouth.wasPressedThisFrame ||
                gp.buttonNorth.wasPressedThisFrame ||
                gp.buttonWest.wasPressedThisFrame ||
                gp.buttonEast.wasPressedThisFrame ||
                gp.startButton.wasPressedThisFrame ||
                gp.selectButton.wasPressedThisFrame ||
                gp.leftShoulder.wasPressedThisFrame ||
                gp.rightShoulder.wasPressedThisFrame ||
                gp.leftStickButton.wasPressedThisFrame ||
                gp.rightStickButton.wasPressedThisFrame ||
                gp.leftTrigger.ReadValue() > 0.5f ||
                gp.rightTrigger.ReadValue() > 0.5f
            );

        bool keyboardUsed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        bool nonMouseUsed = gamepadUsed || keyboardUsed;

        if (mouseUsed && !isVisible) SetCursorVisible(true);
        else if (nonMouseUsed && isVisible) SetCursorVisible(false);
    }

    private void SetCursorVisible(bool visible)
    {
        if (isVisible == visible) return;
        isVisible = visible;
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : lockModeWhenHidden;
    }
}