//Pari muutosta et se toimii sen fuckshitter wheelin kaa, mut tää on täysin copilot ku ei oo wheel nyt
using UnityEngine;
using UnityEngine.InputSystem;
using Logitech;

public class CursorAutoHide : MonoBehaviour
{
    [SerializeField] private CursorLockMode lockModeWhenHidden = CursorLockMode.Confined;
    [SerializeField] private int wheelIndex = 0; // Which controller index to check
    
    private bool isVisible = true;
    private bool logitechInitialized = false;


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

        bool wheelButtonUsed = CheckLogitechWheelButtons();

        var gp = Gamepad.current;
        bool gamepadUsed =
            gp != null && (
                gp.buttonSouth.wasPressedThisFrame ||
                gp.buttonNorth.wasPressedThisFrame ||
                gp.buttonWest.wasPressedThisFrame ||
                gp.buttonEast.wasPressedThisFrame ||
                gp.startButton.wasPressedThisFrame ||
                gp.selectButton.wasPressedThisFrame ||
                gp.leftShoulder.wasPressedThisFrame ||
                gp.rightShoulder.wasPressedThisFrame ||
                gp.leftStickButton.wasPressedThisFrame ||
                gp.rightStickButton.wasPressedThisFrame
            );

        bool keyboardUsed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        bool nonMouseUsed = gamepadUsed || keyboardUsed || wheelButtonUsed;

        if (mouseUsed && !isVisible) SetCursorVisible(true);
        else if (nonMouseUsed && isVisible) SetCursorVisible(false);
    }

    private bool CheckLogitechWheelButtons()
    {
        if (!logitechInitialized) return false;
        
        LogitechGSDK.LogiUpdate();
        
        if (!LogitechGSDK.LogiIsConnected(wheelIndex)) return false;

        // Check wheel buttons (0-23 covers most Logitech wheels)
        // LogiButtonTriggered only returns true on the frame the button is pressed
        for (int i = 0; i < 24; i++)
        {
            if (LogitechGSDK.LogiButtonTriggered(wheelIndex, i))
            {
                return true;
            }
        }

        // Check POV hat/D-pad changes
        var state = LogitechGSDK.LogiGetStateUnity(wheelIndex);
        if (state.rgdwPOV != null && state.rgdwPOV.Length > 0)
        {
            // POV returns 0-35999 for directions, 0xFFFFFFFF (-1) when centered
            if (state.rgdwPOV[0] != 0xFFFFFFFF)
            {
                return true;
            }
        }

        return false;
    }

    private void SetCursorVisible(bool visible)
    {
        if (isVisible == visible) return;
        isVisible = visible;
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : lockModeWhenHidden;
    }
}