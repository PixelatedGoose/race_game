using UnityEngine;

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

    void Update()
    {
        if (Controls.CarControls.ui_advance.triggered && GameManager.instance.isPaused == false)
        {
            switch (instructionHandler.boxOpen)
            {
                case true:
                    instructionHandler.ShowNextInstructionInCategory(instructionHandler.curCategory, false, 0);
                    if (instructionHandler.GetInstruction(
                        instructionHandler.curCategory,
                        instructionHandler.index)
                        == "To make sure you understand, try clearing this course to proceed.")
                        //unity haista paska jo iha oikeasti ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    {
                        fuckshitter.DoSomeFuckShit("begin");
                    }
                    break;
                case false:
                    Debug.Log("no voi vittu");
                    break;
            }
        }
    }
}