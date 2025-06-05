using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class instructionCatInHand : MonoBehaviour //shorthand - instruction category (and) input handler
{
    CarInputActions Controls;
    public instructionHandler instructionHandler;
    private fuckshitter fuckshitter;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        instructionHandler = gameObject.GetComponent<instructionHandler>();
    }
    private void OnEnable()
    {
        Controls.Enable();
        fuckshitter = FindAnyObjectByType<fuckshitter>();
        fuckshitter.SetupFuckShit(); //jos vaikuttaa performanceen se on vaa lataukses

        Controls.CarControls.ui_advance.performed += VITTU;
    }

    private void OnDisable()
    {
        Controls.Disable();
    }
    private void OnDestroy()
    {
        Controls.Disable();
        Controls.Dispose();
    }

    //you learn something new everyday
    private void VITTU(InputAction.CallbackContext context)
    {
        if (GameManager.instance.isPaused == false)
        {
            switch (instructionHandler.boxOpen)
            {
                case true:
                    instructionHandler.ShowNextInstructionInCategory(instructionHandler.curCategory, false, 0);
                    if (instructionHandler.GetInstruction(
                        instructionHandler.curCategory,
                        instructionHandler.index)
                        == "To make sure you understand simple instructions, try clearing this course to proceed.")
                    //unity haista paska jo iha oikeasti ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    {
                        fuckshitter.DoSomeFuckShit("begin");
                    }
                    if (instructionHandler.GetInstruction(
                        instructionHandler.curCategory,
                        instructionHandler.index)
                        == "But first, we'll teach you something more important. Drive to the next zone to continue.")
                    //unity haista paska jo iha oikeasti ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    {
                        fuckshitter.DoSomeFuckShit("predriftfadeout");
                    }
                    break;

                case false:
                    Debug.Log("no voi vittu");
                    break;
            }
        }
    }
}