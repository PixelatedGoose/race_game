using UnityEngine;

public class instructionHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowInstruction(string instructText, bool manualSkippingEnabled = false, int anim = 1)
    {
        // do animation (0 = none (stays open), 1 = open, 2 = close)
        // replace text with the next one

        // if instruction == null, show error message on instruction text instead

        // KÄYTTÖ:
        // ShowInstruction(GetInstruction("intro", 0), true);
    }

    public void GetInstruction(string category, int id)
    {
        // get instruction from json
        // sorted by category
        // id for instruction, starts at 0

        // return string of instruction

        // if instruction == null, return error
    }
}
