using UnityEngine;

public class loadArea : MonoBehaviour
{
    public new Collider collider;
    public string prefix;
    public instructionHandler instructionHandler; //jotta tää on paljon helpompaa



    void Awake()
    {
        prefix = gameObject.name.Substring(0, 1);
        collider = GetComponent<Collider>();
        instructionHandler = GameObject.Find("instructionHandler").GetComponent<instructionHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (prefix)
        {
            //ei valmis
            case "1":
                instructionHandler.ShowNextInstructionInCategory(instructionHandler.nextCategory, true, 1);
                break;
        }
        Debug.Log("Hello! My name is Gustavo, but you can call me Gus");
    }
} 