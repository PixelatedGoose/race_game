using UnityEngine;

public class instructionCatInHand : MonoBehaviour //shorthand - instruction category (and) input handler
{
    CarInputActions Controls;
    public instructionHandler instructionHandler;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        instructionHandler = gameObject.GetComponent<instructionHandler>();
    }
    private void OnEnable()
    {
        Controls.Enable();
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
                    break;
                case false:
                    Debug.Log("no voi vittu");
                    break;
            }
        }
    }
}