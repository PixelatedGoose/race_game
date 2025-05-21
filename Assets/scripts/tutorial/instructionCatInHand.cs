using UnityEngine;

public class instructionCatInHand : MonoBehaviour //shorthand - instruction category (and) input handler
{
    public instructionHandler instructionHandler;

    void Awake()
    {
        instructionHandler = GameObject.Find("instructionHandler").GetComponent<instructionHandler>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && GameManager.instance.isPaused == false)
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